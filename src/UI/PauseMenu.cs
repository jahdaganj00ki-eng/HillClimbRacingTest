using System;
using Godot;

namespace HillClimbRacing.UI;

/// <summary>
/// Pause menu overlay.
/// </summary>
[GlobalClass]
public partial class PauseMenu : Panel
{
    private Button _btnResume;
    private Button _btnRestart;
    private Button _btnMainMenu;
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _btnResume = GetNode<Button>("%BtnResume");
        _btnRestart = GetNode<Button>("%BtnRestart");
        _btnMainMenu = GetNode<Button>("%BtnMainMenu");
        _animPlayer = GetNode<AnimationPlayer>("%AnimPlayer");

        _btnResume.Pressed += OnResumePressed;
        _btnRestart.Pressed += OnRestartPressed;
        _btnMainMenu.Pressed += OnMainMenuPressed;

        // Pause game when shown
        GetTree().Paused = true;
        
        _animPlayer?.Play("show");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            OnResumePressed();
        }
    }

    private void OnResumePressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().Paused = false;
        QueueFree();
    }

    private void OnRestartPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    private void OnMainMenuPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }
}