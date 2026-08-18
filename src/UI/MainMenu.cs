using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HillClimbRacing.UI;

/// <summary>
/// Main menu - entry point for the game.
/// </summary>
[GlobalClass]
public partial class MainMenu : Control
{
    private Button _btnPlay;
    private Button _btnGarage;
    private Button _btnSettings;
    private Button _btnQuit;
    private Label _lblCoins;
    private Label _lblVersion;
    private Panel _panelStats;
    private Label _lblTotalDistance;
    private Label _lblTotalCoins;
    private Label _lblVehiclesUnlocked;
    private Label _lblTracksUnlocked;
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        // Get node references
        _btnPlay = GetNode<Button>("%BtnPlay");
        _btnGarage = GetNode<Button>("%BtnGarage");
        _btnSettings = GetNode<Button>("%BtnSettings");
        _btnQuit = GetNode<Button>("%BtnQuit");
        _lblCoins = GetNode<Label>("%LblCoins");
        _lblVersion = GetNode<Label>("%LblVersion");
        _panelStats = GetNode<Panel>("%PanelStats");
        _lblTotalDistance = GetNode<Label>("%LblTotalDistance");
        _lblTotalCoins = GetNode<Label>("%LblTotalCoins");
        _lblVehiclesUnlocked = GetNode<Label>("%LblVehiclesUnlocked");
        _lblTracksUnlocked = GetNode<Label>("%LblTracksUnlocked");
        _animPlayer = GetNode<AnimationPlayer>("%AnimPlayer");

        // Connect signals
        _btnPlay.Pressed += OnPlayPressed;
        _btnGarage.Pressed += OnGaragePressed;
        _btnSettings.Pressed += OnSettingsPressed;
        _btnQuit.Pressed += OnQuitPressed;

        // Update UI with current game state
        UpdateStats();
        
        // Play entrance animation
        _animPlayer?.Play("entrance");
        
        // Play menu music
        AudioManager.Instance?.PlayMusic("res://assets/audio/music_menu.ogg");
    }

    public override void _Process(double delta)
    {
        // Update coin display in real-time
        if (GameManager.Instance != null)
        {
            _lblCoins.Text = $"{GameManager.Instance.Coins:N0} 🪙";
        }
    }

    private void OnPlayPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        
        // Transition to vehicle selection
        GetTree().ChangeSceneToFile("res://scenes/VehicleSelect.tscn");
    }

    private void OnGaragePressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().ChangeSceneToFile("res://scenes/Garage.tscn");
    }

    private void OnSettingsPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().ChangeSceneToFile("res://scenes/Settings.tscn");
    }

    private void OnQuitPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().Quit();
    }

    private void UpdateStats()
    {
        if (GameManager.Instance == null) return;

        var gm = GameManager.Instance;
        var trackSelector = TrackSelector.Instance;
        var vehicleFactory = VehicleFactory.Instance;

        _lblCoins.Text = $"{gm.Coins:N0} 🪙";
        _lblTotalDistance.Text = $"Total Distance: {gm.TotalCoinsCollected / 100f:F1} km"; // Approximate
        _lblTotalCoins.Text = $"Total Coins: {gm.TotalCoinsCollected:N0}";
        _lblVehiclesUnlocked.Text = $"Vehicles: {gm.UnlockedVehicles.Count} / {vehicleFactory?.GetAllVehicleTypes().Count ?? 6}";
        _lblTracksUnlocked.Text = $"Tracks: {gm.UnlockedTracks.Count} / {trackSelector?.GetTotalTracks() ?? 30}";

        _lblVersion.Text = "v1.0.0 - Hill Climb Racing Windows Clone";
    }
}