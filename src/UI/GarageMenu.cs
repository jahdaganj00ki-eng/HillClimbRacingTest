using System;
using System.Collections.Generic;
using Godot;

namespace HillClimbRacing.UI;

/// <summary>
/// Garage/Tuning menu for upgrading vehicle stats.
/// 5 categories, 10 levels each.
/// </summary>
[GlobalClass]
public partial class GarageMenu : Control
{
    private VBoxContainer _tuningContainer;
    private Label _lblVehicleName;
    private Label _lblCoins;
    private Button _btnBack;
    private VehicleType _currentVehicle;
    private readonly Dictionary<TuningCategory, VBoxContainer> _categoryContainers = new();
    private readonly Dictionary<TuningCategory, Label> _levelLabels = new();
    private readonly Dictionary<TuningCategory, Button> _upgradeButtons = new();
    private readonly Dictionary<TuningCategory, ProgressBar> _progressBars = new();
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _tuningContainer = GetNode<VBoxContainer>("%TuningContainer");
        _lblVehicleName = GetNode<Label>("%LblVehicleName");
        _lblCoins = GetNode<Label>("%LblCoins");
        _btnBack = GetNode<Button>("%BtnBack");
        _animPlayer = GetNode<AnimationPlayer>("%AnimPlayer");

        _btnBack.Pressed += OnBackPressed;

        _currentVehicle = GameManager.Instance?.SelectedVehicle ?? VehicleType.Jeep;
        BuildTuningUI();
        UpdateAllDisplays();

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

    private void BuildTuningUI()
    {
        var gm = GameManager.Instance;
        var factory = VehicleFactory.Instance;
        if (gm == null || factory == null) return;

        var stats = factory.GetBaseStats(_currentVehicle);
        if (stats != null)
        {
            _lblVehicleName.Text = $"{stats.Name} - TUNING";
        }

        var categories = Enum.GetValues<TuningCategory>();
        foreach (var category in categories)
        {
            var categoryBox = new VBoxContainer { Separation = 10 };
            
            // Category header
            var header = new HBoxContainer { Separation = 15 };
            var catLabel = new Label
            {
                Text = GetCategoryName(category),
                ThemeOverrideFontSizes = { ["normal"] = 20 },
                ThemeOverrideColors = { ["font_color"] = Color(1f, 0.9f, 0.2f) },
                CustomMinimumSize = new Vector2(200, 0)
            };
            header.AddChild(catLabel);

            // Progress bar
            var progressBar = new ProgressBar
            {
                MinValue = 0,
                MaxValue = VehicleTuning.MaxLevel,
                Value = 0,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(200, 20)
            };
            header.AddChild(progressBar);
            _progressBars[category] = progressBar;

            // Level display
            var levelLabel = new Label
            {
                Text = "Level 0/10",
                ThemeOverrideFontSizes = { ["normal"] = 18 },
                ThemeOverrideColors = { ["font_color"] = Colors.White },
                CustomMinimumSize = new Vector2(120, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            header.AddChild(levelLabel);
            _levelLabels[category] = levelLabel;

            categoryBox.AddChild(header);

            // Description
            var descLabel = new Label
            {
                Text = GetCategoryDescription(category),
                ThemeOverrideFontSizes = { ["normal"] = 14 },
                ThemeOverrideColors = { ["font_color"] = new Color(0.7f, 0.8f, 0.9f) },
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            categoryBox.AddChild(descLabel);

            // Upgrade button
            var upgradeBtn = new Button
            {
                Text = "UPGRADE",
                CustomMinimumSize = new Vector2(150, 40),
                Disabled = true
            };
            upgradeBtn.ThemeOverrideTheme = CreateButtonTheme(new Color(0.2f, 0.6f, 0.2f));
            
            var capturedCategory = category;
            upgradeBtn.Pressed += () => OnUpgradePressed(capturedCategory);
            _upgradeButtons[category] = upgradeBtn;

            var btnContainer = new HBoxContainer();
            btnContainer.AddChild(new Control()); // Spacer
            btnContainer.AddChild(upgradeBtn);
            categoryBox.AddChild(btnContainer);

            // Separator
            categoryBox.AddChild(new HSeparator());

            _tuningContainer.AddChild(categoryBox);
            _categoryContainers[category] = categoryBox;
        }
    }

    private string GetCategoryName(TuningCategory cat)
    {
        return cat switch
        {
            TuningCategory.Engine => "⚡ ENGINE",
            TuningCategory.Suspension => "🔧 SUSPENSION",
            TuningCategory.Tires => "🛞 TIRES",
            TuningCategory.Weight => "⚖️ WEIGHT",
            TuningCategory.Drivetrain => "⚙️ DRIVETRAIN",
            _ => cat.ToString().ToUpper()
        };
    }

    private string GetCategoryDescription(TuningCategory cat)
    {
        return cat switch
        {
            TuningCategory.Engine => "Increases engine power (+15%/level) and torque (+12%/level). Higher RPM limit.",
            TuningCategory.Suspension => "Increases suspension travel (+10%/level) and damping (+8%/level). Better landing stability.",
            TuningCategory.Tires => "Increases tire grip (+10%/level) and width (+5%/level). Better traction and cornering.",
            TuningCategory.Weight => "Reduces vehicle mass (-8%/level) and lowers center of gravity (-5%/level). Less flipping.",
            TuningCategory.Drivetrain => "Adjusts 4WD torque split and differential lock strength. Better power delivery.",
            _ => ""
        };
    }

    private ButtonTheme CreateButtonTheme(Color baseColor)
    {
        return new ButtonTheme
        {
            Normal = CreateStyleBox(baseColor),
            Hover = CreateStyleBox(baseColor.Lightened(0.2f)),
            Pressed = CreateStyleBox(baseColor.Darkened(0.2f)),
            Disabled = CreateStyleBox(new Color(0.2f, 0.2f, 0.25f)),
            Focus = CreateStyleBox(baseColor.Lightened(0.2f))
        };
    }

    private StyleBoxFlat CreateStyleBox(Color color)
    {
        return new StyleBoxFlat
        {
            BgColor = color,
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderColor = color.Lightened(0.3f),
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8
        };
    }

    private void UpdateAllDisplays()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        var tuning = gm.GetTuning(_currentVehicle);
        var factory = VehicleFactory.Instance;
        var stats = factory?.GetBaseStats(_currentVehicle);

        foreach (TuningCategory category in Enum.GetValues<TuningCategory>())
        {
            int level = tuning.GetLevel(category);
            int cost = tuning.GetUpgradeCost(category);

            if (_levelLabels.TryGetValue(category, out var levelLabel))
            {
                levelLabel.Text = level >= VehicleTuning.MaxLevel ? "MAX LEVEL" : $"Level {level}/10";
            }

            if (_progressBars.TryGetValue(category, out var progressBar))
            {
                progressBar.Value = level;
            }

            if (_upgradeButtons.TryGetValue(category, out var btn))
            {
                if (level >= VehicleTuning.MaxLevel)
                {
                    btn.Text = "MAXED";
                    btn.Disabled = true;
                }
                else
                {
                    btn.Text = $"UPGRADE ({cost:N0} 🪙)";
                    btn.Disabled = gm.Coins < cost;
                }
            }
        }
    }

    private void OnUpgradePressed(TuningCategory category)
    {
        var gm = GameManager.Instance;
        var tuning = gm?.GetTuning(_currentVehicle);
        if (gm == null || tuning == null) return;

        int cost = tuning.GetUpgradeCost(category);
        if (cost < 0 || gm.Coins < cost) return;

        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_upgrade.ogg");
        
        gm.SpendCoins(cost);
        gm.UpgradeTuning(_currentVehicle, category);
        
        UpdateAllDisplays();
        _lblCoins.Text = $"{gm.Coins:N0} 🪙";
    }

    private void OnBackPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().ChangeSceneToFile("res://scenes/VehicleSelect.tscn");
    }
}