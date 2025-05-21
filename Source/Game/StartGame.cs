using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

/// <summary>
/// StartGame Script.
/// </summary>
public class Menu : Script
{
    public UIControl PlayControl;
    public UIControl QuitControl;
    private Button playButton;
    private Button quitButton;
    public SceneReference GameScene;
    /// <inheritdoc/>
    public override void OnStart()
    {
        playButton = PlayControl.Get<Button>();
        quitButton = QuitControl.Get<Button>();
    }
    

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        if (playButton.IsPressed)
        {
            StartGame();
        } else if (quitButton.IsPressed)
        {
            Engine.RequestExit();
        }
    }

    private void StartGame()
    {
        Level.ChangeSceneAsync(GameScene);
    }
}
