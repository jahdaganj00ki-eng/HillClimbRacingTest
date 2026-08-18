using System;
using Godot;

namespace HillClimbRacing.UI;

/// <summary>
/// In-game HUD showing speed, distance, RPM, checkpoints, coins.
/// </summary>
[GlobalClass]
public partial class HUD : CanvasLayer
{
    private Label _lblSpeed;
    private Label _lblDistance;
    private Label _lblRpm;
    private Label _lblCheckpoint;
    private Label _lblCoins;
    private ProgressBar _rpmBar;
    private ProgressBar _fuelBar; // Always full (no fuel)
    private Label _lblBestDistance;
    private TextureProgressBar _rpmGauge;
    private AnimationPlayer _animPlayer;
    private VehicleBase _playerVehicle;
    private CheckpointSystem _checkpointSystem;

    public override void _Ready()
    {
        _lblSpeed = GetNode<Label>("%LblSpeed");
        _lblDistance = GetNode<Label>("%LblDistance");
        _lblRpm = GetNode<Label>("%LblRpm");
        _lblCheckpoint = GetNode<Label>("%LblCheckpoint");
        _lblCoins = GetNode<Label>("%LblCoins");
        _rpmBar = GetNode<ProgressBar>("%RpmBar");
        _fuelBar = GetNode<ProgressBar>("%FuelBar");
        _lblBestDistance = GetNode<Label>("%LblBestDistance");
        _rpmGauge = GetNode<TextureProgressBar>("%RpmGauge");
        _animPlayer = GetNode<AnimationPlayer>("%AnimPlayer");

        _checkpointSystem = CheckpointSystem.Instance;

        // Find player vehicle
        _playerVehicle = GetTree().CurrentScene?.GetNode<VehicleBase>("PlayerVehicle");
        if (_playerVehicle == null)
        {
            // Try to find in root
            _playerVehicle = GetTree().Root.GetNodeOrNull<VehicleBase>("PlayerVehicle");
        }

        // Fuel bar always full (no fuel system)
        _fuelBar.Value = _fuelBar.MaxValue;
        
        _animPlayer?.Play("entrance");
    }

    public override void _Process(double delta)
    {
        if (_playerVehicle == null)
        {
            // Try to find player vehicle
            _playerVehicle = GetTree().CurrentScene?.GetNode<VehicleBase>("PlayerVehicle");
            if (_playerVehicle == null)
            {
                _playerVehicle = GetTree().Root.GetNodeOrNull<VehicleBase>("PlayerVehicle");
            }
        }

        if (_playerVehicle != null && !_playerVehicle.IsFlipped())
        {
            UpdateHUD();
        }
    }

    private void UpdateHUD()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Speed
        float speed = _playerVehicle.GetSpeedKmh();
        _lblSpeed.Text = $"{speed:F0} km/h";

        // Distance
        float distanceKm = gm.CurrentDistance / 1000f;
        _lblDistance.Text = $"{distanceKm:F2} km";

        // Best distance
        float bestKm = gm.BestDistance / 1000f;
        _lblBestDistance.Text = $"Best: {bestKm:F2} km";

        // RPM
        float rpm = _playerVehicle.GetRpm();
        float maxRpm = _playerVehicle._tunedStats?.MaxRpm ?? 6000f;
        _lblRpm.Text = $"{rpm:F0} / {maxRpm:F0} RPM";
        _rpmBar.Value = rpm;
        _rpmBar.MaxValue = maxRpm * 1.1f;
        if (_rpmGauge != null)
        {
            _rpmGauge.Value = rpm / maxRpm;
        }

        // Checkpoint
        int cp = gm.CurrentCheckpointIndex + 1;
        int totalCp = _checkpointSystem?.GetTotalCheckpoints() ?? 0;
        _lblCheckpoint.Text = $"Checkpoint: {cp} / {totalCp}";

        // Coins
        _lblCoins.Text = $"{gm.Coins:N0} 🪙";
    }

    public void ShowRespawnMessage(float countdown)
    {
        var msgLabel = GetNodeOrNull<Label>("RespawnMessage");
        if (msgLabel == null)
        {
            msgLabel = new Label
            {
                Name = "RespawnMessage",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"RESPAWNING IN {countdown:F1}s...",
                ThemeOverrideFontSizes = { ["normal"] = 48 },
                ThemeOverrideColors = { ["font_color"] = Colors.Red },
                AnchorsPreset = LayoutPreset.Center
            };
            AddChild(msgLabel);
        }
        msgLabel.Text = $"RESPAWNING IN {countdown:F1}s...";
        msgLabel.Visible = true;
    }

    public void HideRespawnMessage()
    {
        var msgLabel = GetNodeOrNull<Label>("RespawnMessage");
        if (msgLabel != null)
        {
            msgLabel.Visible = false;
        }
    }

    public void OnCheckpointReached(int index, float distance)
    {
        // Show checkpoint popup
        var popup = new Label
        {
            Text = $"CHECKPOINT {index + 1} REACHED!",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            ThemeOverrideFontSizes = { ["normal"] = 36 },
            ThemeOverrideColors = { ["font_color"] = new Color(0.3f, 1f, 0.3f) },
            AnchorsPreset = LayoutPreset.Center,
            Modulate = Colors.White
        };
        AddChild(popup);
        
        // Animate and remove
        var tween = CreateTween();
        tween.TweenProperty(popup, "modulate:a", 0f, 1.5f);
        tween.TweenCallback(Callable.From(() => popup.QueueFree()));
    }
}