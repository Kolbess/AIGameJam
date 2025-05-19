using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

public class UIManager : Script
{
    [Header("UI Elements")]
    public Slider sanityMeterSlider;
    public Label scoreText;
    public Actor gameOverScreen; // Actor z UI, który pokazujemy/ukrywamy
    public Label finalScoreText;
    public Button restartButton;

    public SanityManager sanityManager;  // Zakładam, że są też Flaxowe skrypty
    public GameManager gameManager;

    public override void OnStart()
    {
        // Sprawdzamy czy referencje są przypisane (można też w edytorze je przypisać)
        if (sanityManager == null)
            Debug.LogError("UIManager: SanityManager not assigned!");

        if (gameManager == null)
            Debug.LogError("UIManager: GameManager not assigned!");

        if (gameOverScreen != null)
            gameOverScreen.IsActive = false;

        if (restartButton != null)
            restartButton.Clicked += OnRestartButtonClick;

        // Subskrypcje eventów
        if (sanityManager != null)
            sanityManager.OnSanityChanged += UpdateSanityMeter;

        if (gameManager != null)
            gameManager.OnGameOver += ShowGameOverScreen;
    }

    public override void OnDestroy()
    {
        if (sanityManager != null)
            sanityManager.OnSanityChanged -= UpdateSanityMeter;

        if (gameManager != null)
            gameManager.OnGameOver -= ShowGameOverScreen;

        if (restartButton != null)
            restartButton.Clicked -= OnRestartButtonClick;
    }

    public void UpdateSanityMeter(float sanityPercentage)
    {
        if (sanityMeterSlider != null)
            sanityMeterSlider.Value = sanityPercentage;
        else
            Debug.LogWarning("UIManager: Sanity Meter Slider not assigned!");
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.Text = "Score: " + score.ToString();
        else
            Debug.LogWarning("UIManager: Score Text not assigned!");
    }

    public void ShowGameOverScreen(GameManager.EndReason reason)
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.IsActive = true;

            if (finalScoreText != null && gameManager != null)
                finalScoreText.Text = "Final Score: " + gameManager.GetFinalScore().ToString();
            else if (finalScoreText == null)
                Debug.LogWarning("UIManager: Final Score Text not assigned!");
        }
        else
        {
            Debug.LogWarning("UIManager: Game Over Screen Actor not assigned!");
        }

        if (sanityMeterSlider != null)
            sanityMeterSlider.Visible = false;
        if (scoreText != null)
            scoreText.Visible = false;
    }

    public void HideGameOverScreen()
    {
        if (gameOverScreen != null)
            gameOverScreen.IsActive = false;
        else
            Debug.LogWarning("UIManager: Game Over Screen Actor not assigned!");

        if (sanityMeterSlider != null)
            sanityMeterSlider.Visible = true;
        if (scoreText != null)
            scoreText.Visible = true;
    }

    private void OnRestartButtonClick()
    {
        if (gameManager != null)
            gameManager.StartGame();
        else
            Debug.LogError("UIManager: GameManager not found to restart game!");
    }
}
