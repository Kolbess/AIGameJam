using FlaxEngine;
using System;

namespace Game;

public class SanityManager : Script
{
    [Serialize] public GameSettings gameSettings;

    private float currentSanity;

    private VisualEffectsManager visualEffectsManager;
    private SoundManager soundManager;
    private GameManager gameManager;

    public event Action<float> OnSanityChanged;
    public event Action OnSanityZero;

    public override void OnStart()
    {
        visualEffectsManager = Actor?.GetScript<VisualEffectsManager>();
        if (visualEffectsManager == null)
            Debug.LogError("SanityManager: VisualEffectsManager not found!");

        soundManager = Actor?.GetScript<SoundManager>();
        if (soundManager == null)
            Debug.LogError("SanityManager: SoundManager not found!");

        gameManager = Actor?.GetScript<GameManager>();
        if (gameManager == null)
            Debug.LogError("SanityManager: GameManager not found!");

        if (gameSettings == null)
            Debug.LogError("SanityManager: GameSettings not assigned!");

        currentSanity = gameSettings != null ? gameSettings.maxSanity : 100f;
        NotifySanityChanged();
    }

    public override void OnUpdate()
    {
        if (gameManager != null && gameManager.GetCurrentGameState() != GameManager.GameState.Playing)
            return;

        float drainRate = gameSettings != null ? gameSettings.SanityDrainRate : 1f;
        ReduceSanity(drainRate * Time.DeltaTime);
    }

    public void AddSanity(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("SanityManager: Attempted to add negative sanity.");
            return;
        }

        currentSanity += amount;
        ClampSanity();
        NotifySanityChanged();
    }

    public void ReduceSanity(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("SanityManager: Attempted to reduce negative sanity.");
            return;
        }

        currentSanity -= amount;
        ClampSanity();
        NotifySanityChanged();

        if (currentSanity <= 0 && gameManager != null && gameManager.GetCurrentGameState() == GameManager.GameState.Playing)
        {
            OnSanityZero?.Invoke();
            gameManager.EndGame(GameManager.EndReason.SanityZero);
        }
    }

    public float GetCurrentSanity()
    {
        return currentSanity;
    }

    private void ClampSanity()
    {
        float maxSanity = gameSettings != null ? gameSettings.maxSanity : 100f;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
    }

    private void NotifySanityChanged()
    {
        float maxSanity = gameSettings != null ? gameSettings.maxSanity : 100f;
        float normalizedSanity = currentSanity / maxSanity;
        OnSanityChanged?.Invoke(normalizedSanity);

        visualEffectsManager?.UpdateSanityEffects(normalizedSanity);
        soundManager?.AdjustSanityAudio(normalizedSanity);
    }
}
