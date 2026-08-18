using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HillClimbRacing.UI;

/// <summary>
/// Vehicle selection screen with garage/tuning access.
/// </summary>
[GlobalClass]
public partial class VehicleSelectMenu : Control
{
    private HBoxContainer _vehicleListContainer;
    private VBoxContainer _vehicleDetailsPanel;
    private Label _lblVehicleName;
    private Label _lblVehicleDescription;
    private Label _lblVehicleStats;
    private Button _btnSelect;
    private Button _btnGarage;
    private Button _btnBack;
    private Label _lblCoins;
    private VehicleType _selectedVehicle = VehicleType.Jeep;
    private readonly Dictionary<VehicleType, Button> _vehicleButtons = new();
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _vehicleListContainer = GetNode<HBoxContainer>("%VehicleListContainer");
        _vehicleDetailsPanel = GetNode<VBoxContainer>("%VehicleDetailsPanel");
        _lblVehicleName = GetNode<Label>("%LblVehicleName");
        _lblVehicleDescription = GetNode<Label>("%LblVehicleDescription");
        _lblVehicleStats = GetNode<Label>("%LblVehicleStats");
        _btnSelect = GetNode<Button>("%BtnSelect");
        _btnGarage = GetNode<Button>("%BtnGarage");
        _btnBack = GetNode<Button>("%BtnBack");
        _lblCoins = GetNode<Label>("%LblCoins");
        _animPlayer = GetNode<AnimationPlayer>("%AnimPlayer");

        _btnSelect.Pressed += OnSelectPressed;
        _btnGarage.Pressed += OnGaragePressed;
        _btnBack.Pressed += OnBackPressed;

        PopulateVehicleList();
        SelectVehicle(GameManager.Instance?.SelectedVehicle ?? VehicleType.Jeep);
        
        _animPlayer?.Play("entrance");
        AudioManager.Instance?.PlayMusic("res://assets/audio/music_garage.ogg");
    }

    public override void _Process(double delta)
    {
        if (GameManager.Instance != null)
        {
            _lblCoins.Text = $"{GameManager.Instance.Coins:N0} 🪙";
        }
    }

    private void PopulateVehicleList()
    {
        var factory = VehicleFactory.Instance;
        if (factory == null) return;

        var vehicles = factory.GetAllVehicleTypes();
        foreach (var vehicleType in vehicles)
        {
            var stats = factory.GetBaseStats(vehicleType);
            if (stats == null) continue;

            bool unlocked = GameManager.Instance?.UnlockedVehicles.Contains(vehicleType) ?? stats.IsUnlockedByDefault;
            int unlockCost = stats.UnlockCoins;

            var btn = new Button
            {
                CustomMinimumSize = new Vector2(140, 180),
                Text = "",
                ToggleMode = true,
                ButtonGroup = "vehicle_select",
                ToolTip = stats.Description
            };

            // Vehicle preview
            var vbox = new VBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center
            };

            var preview = new TextureRect
            {
                CustomMinimumSize = new Vector2(120, 80),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Texture = stats.SpritePath != null ? ResourceLoader.Load<Texture2D>(stats.SpritePath) : null
            };
            if (preview.Texture == null)
            {
                // Create placeholder
                var img = Image.Create(120, 80, false, Image.Format.Rgba8);
                img.Fill(stats.BodyColor);
                preview.Texture = ImageTexture.CreateFromImage(img);
            }
            vbox.AddChild(preview);

            var nameLabel = new Label
            {
                Text = stats.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                ThemeOverrideFontSizes = { ["normal"] = 16 }
            };
            vbox.AddChild(nameLabel);

            var unlockLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                ThemeOverrideFontSizes = { ["normal"] = 14 }
            };
            if (!unlocked)
            {
                unlockLabel.Text = $"Locked - {unlockCost:N0} 🪙";
                unlockLabel.ThemeOverrideColors = { ["font_color"] = Colors.Gray };
                btn.Disabled = true;
            }
            else
            {
                unlockLabel.Text = "UNLOCKED";
                unlockLabel.ThemeOverrideColors = { ["font_color"] = new Color(0.3f, 1f, 0.3f) };
            }
            vbox.AddChild(unlockLabel);

            btn.AddChild(vbox);
            _vehicleListContainer.AddChild(btn);
            _vehicleButtons[vehicleType] = btn;

            int capturedType = (int)vehicleType;
            btn.Toggled += (pressed) =>
            {
                if (pressed)
                {
                    SelectVehicle((VehicleType)capturedType);
                }
            };
        }
    }

    private void SelectVehicle(VehicleType vehicle)
    {
        _selectedVehicle = vehicle;
        var factory = VehicleFactory.Instance;
        var stats = factory?.GetBaseStats(vehicle);
        var gm = GameManager.Instance;

        if (stats != null)
        {
            _lblVehicleName.Text = stats.Name;
            _lblVehicleDescription.Text = stats.Description;
            
            bool unlocked = gm?.UnlockedVehicles.Contains(vehicle) ?? stats.IsUnlockedByDefault;
            
            if (unlocked)
            {
                _btnSelect.Text = gm?.SelectedVehicle == vehicle ? "SELECTED" : "SELECT";
                _btnSelect.Disabled = gm?.SelectedVehicle == vehicle;
                _btnGarage.Visible = true;
            }
            else
            {
                _btnSelect.Text = $"UNLOCK ({stats.UnlockCoins:N0} 🪙)";
                _btnSelect.Disabled = gm?.Coins < stats.UnlockCoins;
                _btnGarage.Visible = false;
            }

            // Show base stats
            var tunedStats = stats.GetTunedStats(gm?.GetTuning(vehicle) ?? new VehicleTuning());
            _lblVehicleStats.Text = $@"
Mass: {tunedStats.Mass:F0} kg
Power: {tunedStats.EnginePower:F0} kW
Torque: {tunedStats.EngineTorque:F0} Nm
Max RPM: {tunedStats.MaxRpm:F0}
Suspension Travel: {tunedStats.SuspensionTravel:F2} m
Tire Grip: {tunedStats.TireGrip:F1}x
Drivetrain: {tunedStats.Drivetrain}
Weight Reduction: {gm?.GetTuning(vehicle).WeightLevel ?? 0}/10
";
        }

        // Update button toggle states
        foreach (var kvp in _vehicleButtons)
        {
            kvp.Value.ButtonPressed = kvp.Key == vehicle;
        }
    }

    private void OnSelectPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        var gm = GameManager.Instance;
        var factory = VehicleFactory.Instance;
        var stats = factory?.GetBaseStats(_selectedVehicle);

        if (gm == null || stats == null) return;

        bool unlocked = gm.UnlockedVehicles.Contains(_selectedVehicle) || stats.IsUnlockedByDefault;

        if (!unlocked)
        {
            // Try to unlock
            if (gm.Coins >= stats.UnlockCoins)
            {
                gm.SpendCoins(stats.UnlockCoins);
                gm.UnlockVehicle(_selectedVehicle);
                gm.SelectedVehicle = _selectedVehicle;
                SaveSystem.Instance.SaveGame();
                SelectVehicle(_selectedVehicle);
            }
        }
        else
        {
            gm.SelectedVehicle = _selectedVehicle;
            SelectVehicle(_selectedVehicle);
        }
    }

    private void OnGaragePressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().ChangeSceneToFile("res://scenes/Garage.tscn");
    }

    private void OnBackPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }
}