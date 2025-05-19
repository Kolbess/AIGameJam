using System;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace Game;

public class CameraController : Script
{
    [EditorOrder(0), Tooltip("Assign the player's Actor here")]
    public Actor PlayerActor;

    [EditorOrder(1), Tooltip("Over-the-shoulder offset")]
    public Vector3 Offset = new Vector3(0, 2, -3);

    [EditorOrder(2), Tooltip("How smoothly the camera follows")]
    public float SmoothSpeed = 0.125f;

    [EditorOrder(3), Tooltip("Overall intensity of sanity effects")]
    public float SanityEffectIntensity = 1.0f;

    [EditorOrder(4), Tooltip("Sanity level to start distortion")]
    public float LowSanityDistortionStart = 0.3f;

    [EditorOrder(5), Tooltip("Max position distortion")]
    public float MaxDistortionMagnitude = 0.1f;

    [EditorOrder(6), Tooltip("Max rotation distortion (degrees)")]
    public float MaxRotationDistortion = 2.0f;

    [EditorOrder(7), Tooltip("Speed of distortion changes")]
    public float DistortionSpeed = 1.0f;

    // Camera shake params
    private float _shakeDuration = 0f;
    private float _shakeMagnitude = 0f;
    private Vector3 _initialLocalPosition;

    private SanityManager _sanityManager;
    private VisualEffectsManager _visualEffectsManager;

    public override void OnStart()
    {
        if (PlayerActor == null)
        {
            Debug.LogError("CameraController: PlayerActor is not assigned!");
        }

        _sanityManager = Actor.Scene.FindScript<SanityManager>();
        if (_sanityManager == null)
        {
            Debug.LogError("CameraController: SanityManager not found!");
        }
        else
        {
            _sanityManager.OnSanityChanged += ApplySanityCameraEffects;
        }

        _visualEffectsManager = Actor.Scene.FindScript<VisualEffectsManager>();
        if (_visualEffectsManager == null)
        {
            Debug.LogWarning("CameraController: VisualEffectsManager not found. Sanity post-processing effects will not be applied.");
        }

        _initialLocalPosition = Actor.Transform.Translation;
    }

    public override void OnDestroy()
    {
        if (_sanityManager != null)
        {
            _sanityManager.OnSanityChanged -= ApplySanityCameraEffects;
        }
    }

    public override void OnLateUpdate()
    {
        if (PlayerActor == null)
            return;

        // Calculate desired position based on player's forward direction
        var playerTransform = PlayerActor.Transform;
        Vector3 desiredPosition = playerTransform.Translation + Offset * playerTransform.Orientation;
        Vector3 smoothedPosition = Vector3.Lerp(Actor.Transform.Translation, desiredPosition, SmoothSpeed);
        Actor.Transform = new Transform(smoothedPosition, Actor.Transform.Orientation);

        // Look at the player slightly above
        Vector3 lookTarget = PlayerActor.Transform.Translation + Vector3.Up * 0.5f;
        Actor.LookAt(lookTarget);

        // Apply shake if active
        if (_shakeDuration > 0)
        {
            Vector3 shakeOffset = RandomInsideUnitSphere() * _shakeMagnitude;
            Actor.Transform = new Transform(_initialLocalPosition + shakeOffset, Actor.Transform.Orientation);
            _shakeDuration -= Time.DeltaTime;
        }
        else
        {
            _shakeDuration = 0f;
            // No shake offset applied; smooth follow already done above
        }
    }

    private static Vector3 RandomInsideUnitSphere()
    {
        // Similar to Unity's Random.insideUnitSphere
        // Generates a random point inside a unit sphere
        Vector3 point;
        do
        {
            point = new Vector3(
                (float)(RandomUtil.Rand() * 2 - 1),
                (float)(RandomUtil.Rand() * 2 - 1),
                (float)(RandomUtil.Rand() * 2 - 1));
        } while (point.LengthSquared > 1);
        return point;
    }
    
    public float Noise(float x, PerlinNoise perlin)
    {
        return perlin.Sample(x, 0f);
    }

    /// <summary>
    /// Adjust camera effects based on the current sanity level.
    /// </summary>
    ///
    public void ApplySanityCameraEffects(float sanityLevel)
    {
        if (_visualEffectsManager != null)
            _visualEffectsManager.UpdateSanityEffects(sanityLevel);

        float distortionFactor = 1.0f - Mathf.Clamp((sanityLevel - LowSanityDistortionStart) / (1.0f - LowSanityDistortionStart), 0, 1);

        float time = Time.GameTime;
        float speed = DistortionSpeed;

        // Create PerlinNoise instances for each axis
        var noiseX = new PerlinNoise(time * speed, 1.0f, 1.0f, 1);
        var noiseY = new PerlinNoise(time * speed + 100f, 1.0f, 1.0f, 1);
        var noiseZ = new PerlinNoise(time * speed + 200f, 1.0f, 1.0f, 1);

        Vector3 posDistortion = new Vector3(
            Noise(0, noiseX) * MaxDistortionMagnitude,
            Noise(0, noiseY) * MaxDistortionMagnitude,
            Noise(0, noiseZ) * MaxDistortionMagnitude
        ) * distortionFactor * SanityEffectIntensity;

        // Rotation distortion
        var noiseRotX = new PerlinNoise(time * speed + 300f, 1.0f, 1.0f, 1);
        var noiseRotY = new PerlinNoise(time * speed + 400f, 1.0f, 1.0f, 1);
        var noiseRotZ = new PerlinNoise(time * speed + 500f, 1.0f, 1.0f, 1);

        float factor = distortionFactor * SanityEffectIntensity;

        Quaternion rotDistortion = Quaternion.Euler(
            Noise(0, noiseRotX) * MaxRotationDistortion * factor,
            Noise(0, noiseRotY) * MaxRotationDistortion * factor,
            Noise(0, noiseRotZ) * MaxRotationDistortion * factor
        );

        Actor.Transform = new Transform(
            Actor.Transform.Translation + posDistortion,
            Actor.Transform.Orientation * rotDistortion
        );
    }


    /// <summary>
    /// Triggers a camera shake effect.
    /// </summary>
    public void ShakeCamera(float duration, float magnitude)
    {
        _shakeDuration = duration;
        _shakeMagnitude = magnitude;
        _initialLocalPosition = Actor.Transform.Translation;
    }
}
