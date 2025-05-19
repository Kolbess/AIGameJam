using System.Threading.Tasks;
using FlaxEngine;

namespace Game;

public class VisualEffectsManager : Script
{
    [Serialize] private MaterialInstance _mirageEffectMaterial;
    [Serialize] private Camera _camera;
    [Serialize] private Prefab _unstableTileEffectPrefab;
    [Serialize] private Script _cameraController;

    private SanityManager _sanityManager;
    private GameSettings _gameSettings;

    private float _shakeDuration;
    private float _shakeMagnitude;

    public float MirageEffectDuration = 1.0f;

    private void Start()
    {
        _sanityManager = Actor.GetScript<SanityManager>();
        _gameSettings = Actor.GetScript<GameSettings>();
    }

    public void UpdateSanityEffects(float sanity)
    {
        float normalizedSanity = Mathf.Clamp(sanity / _gameSettings.maxSanity, 0f, 1f);

        float chromaticAberrationIntensity = Mathf.Lerp(
            _gameSettings.MaxChromaticAberrationIntensity,
            0f,
            normalizedSanity
        );

        float lensDistortionIntensity = Mathf.Lerp(
            _gameSettings.MaxLensDistortionIntensity,
            0f,
            normalizedSanity
        );

        if (_mirageEffectMaterial != null)
        {
            _mirageEffectMaterial.SetParameterValue("ChromaticAberrationIntensity", chromaticAberrationIntensity);
            _mirageEffectMaterial.SetParameterValue("LensDistortionIntensity", lensDistortionIntensity);
        }
    }

    public void TriggerCameraShake(float duration, float magnitude)
    {
        _shakeDuration = duration;
        _shakeMagnitude = magnitude;
        _ = ShakeCameraAsync(duration, magnitude);
    }

    private async Task ShakeCameraAsync(float duration, float magnitude)
    {
        Vector3 originalPosition = _camera?.LocalPosition ?? Vector3.Zero;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = ((float)RandomUtil.Rand() / int.MaxValue) * 2f - 1f;
            float offsetY = ((float)RandomUtil.Rand() / int.MaxValue) * 2f - 1f;

            Vector3 shakeOffset = new Vector3(offsetX, offsetY, 0f) * magnitude;

            Scripting.InvokeOnUpdate(() =>
            {
                if (_camera != null)
                    _camera.LocalPosition = originalPosition + shakeOffset;
            });

            await Task.Delay(16);
            elapsed += 0.016f;
        }

        Scripting.InvokeOnUpdate(() =>
        {
            if (_camera != null)
                _camera.LocalPosition = originalPosition;
        });
    }

    public void TriggerMirageEffect()
    {
        _ = MirageEffectAsync();
    }

    private async Task MirageEffectAsync()
    {
        float timer = 0f;
        float duration = MirageEffectDuration;

        float startChromaticAberration = (float)_mirageEffectMaterial.GetParameterValue("ChromaticAberrationIntensity");
        float startLensDistortion = (float)_mirageEffectMaterial.GetParameterValue("LensDistortionIntensity");

        while (timer < duration)
        {
            float t = timer / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            float chroma = Mathf.Lerp(startChromaticAberration, _gameSettings.MirageChromaticAberrationIntensity, eased);
            float distortion = Mathf.Lerp(startLensDistortion, _gameSettings.MirageLensDistortionIntensity, eased);

            Scripting.InvokeOnUpdate(() =>
            {
                _mirageEffectMaterial?.SetParameterValue("ChromaticAberrationIntensity", chroma);
                _mirageEffectMaterial?.SetParameterValue("LensDistortionIntensity", distortion);
            });

            await Task.Delay(16);
            timer += 0.016f;
        }

        Scripting.InvokeOnUpdate(() =>
        {
            if (_sanityManager != null)
                UpdateSanityEffects(_sanityManager.GetCurrentSanity());
        });
    }

    public void TriggerTileInstabilityEffect(Vector3 position)
    {
        if (_unstableTileEffectPrefab != null)
        {
            PrefabManager.SpawnPrefab(_unstableTileEffectPrefab, position, Quaternion.Identity);
        }
        else
        {
            Debug.Log("VisualEffectsManager: Unstable Tile Effect Prefab is not assigned!");
        }

        if (_cameraController != null && _gameSettings != null)
        {
            var cameraShakeScript = _cameraController as VisualEffectsManager;
            cameraShakeScript?.TriggerCameraShake(
                _gameSettings.tileInstabilityShakeDuration,
                _gameSettings.tileInstabilityShakeMagnitude
            );
        }
    }
}
