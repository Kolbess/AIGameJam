namespace Game;

using FlaxEngine;
using System;
using System.Collections.Generic;

public class SoundManager : Script
{
    [Header("Audio Sources")]
    public AudioSource ambientSource;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource sanityEffectSource;

    [Header("Audio Clips")]
    public AudioClip ambientDesertWind;
    public AudioClip backgroundMusic;
    public AudioClip playerFootstep;
    public AudioClip itemCollectedSound;
    public AudioClip mirageEncounterSound;
    public AudioClip fallOffPathSound;
    public AudioClip sanityZeroSound;

    [Header("Sanity Audio Settings")]
    public BezierCurve<float> sanityPitchCurve = new BezierCurve<float>();
    public BezierCurve<float> sanityVolumeCurve = new BezierCurve<float>();
    public float lowSanityEchoDelay = 0.3f;
    public float lowSanityEchoDecayRatio = 0.5f;
    public int lowSanityEchoRepeats = 3;
    public float lowSanityDistortionThreshold = 0.3f;

    private SanityManager sanityManager;
    private PlayerController playerController;
    private ItemManager itemManager;
    private GameManager gameManager;

    private float currentSanityNormalized = 1.0f;

    public override void OnAwake()
    {
        // Auto-assign AudioSources if not set
        ambientSource ??= Actor.AddChild<AudioSource>();
        musicSource ??= Actor.AddChild<AudioSource>();
        sfxSource ??= Actor.AddChild<AudioSource>();
        sanityEffectSource ??= Actor.AddChild<AudioSource>();
    }

    public override void OnStart()
    {
        sanityManager = Scene.FindScript<SanityManager>();
        playerController = Scene.FindScript<PlayerController>();
        itemManager = Scene.FindScript<ItemManager>();
        gameManager = Scene.FindScript<GameManager>();

        if (sanityManager != null)
        {
            sanityManager.OnSanityChanged += AdjustSanityAudio;
            sanityManager.OnSanityZero += HandleSanityZero;
        }

        if (playerController != null)
        {
            playerController.OnStepTaken += PlayFootstepSound;
            playerController.OnFallOffPath += HandleFallOffPath;
        }

        PlayAmbientSound(ambientDesertWind, true);
        PlayMusic(backgroundMusic, true);

        if (sanityManager != null)
        {
            float maxSanity = sanityManager.gameSettings != null ? sanityManager.gameSettings.maxSanity : 100f;
            AdjustSanityAudio(sanityManager.GetCurrentSanity() / maxSanity);
        }
        else
        {
            AdjustSanityAudio(1.0f);
        }
    }

    public override void OnDisable()
    {
        if (sanityManager != null)
        {
            sanityManager.OnSanityChanged -= AdjustSanityAudio;
            sanityManager.OnSanityZero -= HandleSanityZero;
        }

        if (playerController != null)
        {
            playerController.OnStepTaken -= PlayFootstepSound;
            playerController.OnFallOffPath -= HandleFallOffPath;
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.Volume = volume;
            sfxSource.Clip = clip;
            sfxSource.Play();
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.Clip = clip;
            musicSource.IsLooping = loop;
            musicSource.Play();
        }
    }

    public void PlayAmbientSound(AudioClip clip, bool loop = true)
    {
        if (clip != null && ambientSource != null)
        {
            ambientSource.Clip = clip;
            ambientSource.IsLooping = loop;
            ambientSource.Play();
        }
    }

    public void AdjustSanityAudio(float sanityLevelNormalized)
    {
        currentSanityNormalized = Mathf.Clamp(sanityLevelNormalized, 0f, 1f);

        // Flax's Evaluate method requires more parameters
        float pitch;
        sanityPitchCurve.Evaluate(out pitch, currentSanityNormalized, false);
        ambientSource.Pitch = pitch;
        musicSource.Pitch = pitch;
        sfxSource.Pitch = pitch;
        sanityEffectSource.Pitch = pitch;

        ApplyLowSanityEffects(currentSanityNormalized);

        if (sanityEffectSource != null && sanityVolumeCurve != null)
        {
            float volume;
            sanityVolumeCurve.Evaluate(out volume, 1.0f - currentSanityNormalized, false);

            sanityEffectSource.Volume = volume;
        }
    }

    private void ApplyLowSanityEffects(float sanityLevelNormalized)
    {
        if (sanityLevelNormalized < lowSanityDistortionThreshold)
        {
            float factor = 1.0f - (sanityLevelNormalized / lowSanityDistortionThreshold);

            // Flax doesn’t have built-in echo/distortion as Unity Mixers do.
            // You would need to implement custom audio effects here or use AudioDSP if available in your Flax version.
            // Here we simulate logic that *would* adjust parameters.

            // Example: Adjust volume as sanity drops
            sanityEffectSource.Volume = Mathf.Lerp(0f, 1f, factor);
        }
        else
        {
            sanityEffectSource.Volume = 0f;
        }
    }

    public void StopAllAudio()
    {
        ambientSource?.Stop();
        musicSource?.Stop();
        sfxSource?.Stop();
        sanityEffectSource?.Stop();
    }

    private void PlayFootstepSound()
    {
        PlaySound(playerFootstep, 0.5f);
    }

    private void HandleFallOffPath()
    {
        StopAllAudio();
        PlaySound(fallOffPathSound, 1.0f);
    }

    private void HandleSanityZero()
    {
        PlaySound(sanityZeroSound, 1.0f);
    }

    // Możesz też dodać metody obsługi innych dźwięków np. zbierania przedmiotów:
    public void PlayItemCollectedSound()
    {
        PlaySound(itemCollectedSound);
    }

    public void PlayMirageEncounterSound()
    {
        PlaySound(mirageEncounterSound);
    }
}
