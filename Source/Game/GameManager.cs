using FlaxEngine;
using System;

namespace Game;

public class GameManager : Script
{
    public enum GameState { Initializing, Playing, GameOver }
    public enum EndReason { FellOffPath, SanityZero }

    private GameState currentGameState = GameState.Initializing;
    private int currentScore = 0;

    private PlayerController playerController;
    private SanityManager sanityManager;
    private UIManager uiManager;

    public event Action<EndReason> OnGameOver;

    public override void OnStart()
    {
        // Znajdź referencje do komponentów skryptowych
        playerController = Actor.GetScript<PlayerController>();
        if (playerController == null)
            Debug.LogError("GameManager: PlayerController not found!");

        sanityManager = Actor.GetScript<SanityManager>();
        if (sanityManager == null)
            Debug.LogError("GameManager: SanityManager not found!");

        uiManager = Actor.GetScript<UIManager>();
        if (uiManager == null)
            Debug.LogError("GameManager: UIManager not found!");
    }

    public override void OnEnable()
    {
        if (playerController != null)
        {
            playerController.OnFallOffPath += HandlePlayerFall;
            playerController.OnStepTaken += HandleStepTaken;
        }

        if (sanityManager != null)
        {
            sanityManager.OnSanityZero += HandleSanityZero;
        }
    }

    public override void OnDisable()
    {
        if (playerController != null)
        {
            playerController.OnFallOffPath -= HandlePlayerFall;
            playerController.OnStepTaken -= HandleStepTaken;
        }

        if (sanityManager != null)
        {
            sanityManager.OnSanityZero -= HandleSanityZero;
        }
    }

    public void StartGame()
    {
        if (currentGameState == GameState.Playing)
        {
            Debug.LogWarning("GameManager: StartGame called while game is already playing.");
            return;
        }

        currentScore = 0;
        currentGameState = GameState.Playing;

        if (uiManager != null)
        {
            uiManager.UpdateScoreDisplay(currentScore);
            uiManager.HideGameOverScreen();
        }

        Debug.Log("Game Started!");
    }

    public void EndGame(EndReason reason)
    {
        if (currentGameState == GameState.GameOver)
        {
            Debug.LogWarning("GameManager: EndGame called while game is already over.");
            return;
        }

        currentGameState = GameState.GameOver;

        Debug.Log($"Game Over! Reason: {reason}. Final Score: {currentScore}");

        OnGameOver?.Invoke(reason);

        if (uiManager != null)
        {
            uiManager.ShowGameOverScreen(reason);
        }
    }

    public void UpdateScore(int distance)
    {
        if (currentGameState != GameState.Playing)
            return;

        currentScore = distance;

        if (uiManager != null)
        {
            uiManager.UpdateScoreDisplay(currentScore);
        }
    }

    public int GetFinalScore()
    {
        return currentScore;
    }

    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }

    // --- Event Handlers ---

    private void HandlePlayerFall()
    {
        Debug.Log("GameManager: Received Player Fall event.");
        // EndGame(EndReason.FellOffPath); // Jeśli już wywołano gdzie indziej
    }

    private void HandleSanityZero()
    {
        Debug.Log("GameManager: Received Sanity Zero event.");
        // EndGame(EndReason.SanityZero); // Jeśli już wywołano gdzie indziej
    }

    private void HandleStepTaken()
    {
        UpdateScore(currentScore + 1);
    }
}
