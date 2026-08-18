using System;
using Godot;

namespace HillClimbRacing.UI;

/// <summary>
/// Settings menu for audio, graphics, controls.
/// </summary>
[GlobalClass]
public partial class SettingsMenu : Control
{
    private HSlider _sliderMasterVolume;
    private HSlider _sliderMusicVolume;
    private HSlider _sliderSfxVolume;
    private HSlider _sliderEngineVolume;
    private CheckButton _cbFullscreen;
    private CheckButton _cbVSync;
    private Button _btnBack;
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _sliderMasterVolume = GetNode<HSlider>("%SliderMasterVolume");
        _sliderMusicVolume = GetNode<HSlider>("%SliderMusicVolume");
        _sliderSfxVolume = GetNode<HSlider>("%SliderSfxVolume");
        _sliderEngineVolume = GetNode<HSlider>("%SliderEngineVolume");
        _cbFullscreen = GetNode<CheckButton>("%CbFullscreen");
        _cbVSync = GetNode<CheckButton>("%CbVSync");
        _btnBack = GetNode<Button>("%BtnBack");
        _animPlayer = GetNode<AnimationPlayer>("%AnimPlayer");

        _sliderMasterVolume.ValueChanged += (v) => AudioManager.Instance?.SetMasterVolume((float)v);
        _sliderMusicVolume.ValueChanged += (v) => AudioManager.Instance?.SetMusicVolume((float)v);
        _sliderSfxVolume.ValueChanged += (v) => AudioManager.Instance?.SetSfxVolume((float)v);
        _sliderEngineVolume.ValueChanged += (v) => AudioManager.Instance?.SetEngineVolume((float)v);
        _cbFullscreen.Toggled += (pressed) => DisplayServer.WindowSetMode(pressed ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        _cbVSync.Toggled += (pressed) => RenderingServer.SetUseVSync(pressed);
        _btnBack.Pressed += OnBackPressed;

        LoadSettings();
        _animPlayer?.Play("entrance");
    }

    private void LoadSettings()
    {
        var config = new ConfigFile();
        string configPath = "user://settings.cfg";
        config.Load(configPath);

        _sliderMasterVolume.Value = (float)config.GetValue("audio", "master_volume", 1.0);
        _sliderMusicVolume.Value = (float)config.GetValue("audio", "music_volume", 0.7);
        _sliderSfxVolume.Value = (float)config.GetValue("audio", "sfx_volume", 1.0);
        _sliderEngineVolume.Value = (float)config.GetValue("audio", "engine_volume", 0.8);
        _cbFullscreen.ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        _cbVSync.ButtonPressed = RenderingServer.IsUsingVSync();
    }

    private void SaveSettings()
    {
        var config = new ConfigFile();
        config.SetValue("audio", "master_volume", _sliderMasterVolume.Value);
        config.SetValue("audio", "music_volume", _sliderMusicVolume.Value);
        config.SetValue("audio", "sfx_volume", _sliderSfxVolume.Value);
        config.SetValue("audio", "engine_volume", _sliderEngineVolume.Value);
        config.Save("user://settings.cfg");
    }

    private void OnBackPressed()
    {
        SaveSettings();
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }
}