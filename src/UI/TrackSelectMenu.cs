using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HillClimbRacing.UI;

/// <summary>
/// Track selection screen showing all 30 tracks grouped by difficulty.
/// </summary>
[GlobalClass]
public partial class TrackSelectMenu : Control
{
    private VBoxContainer _trackListContainer;
    private VBoxContainer _trackDetailsPanel;
    private Label _lblTrackName;
    private Label _lblTrackDescription;
    private Label _lblTrackInfo;
    private Button _btnPlay;
    private Button _btnBack;
    private Label _lblCoins;
    private TrackDifficulty _currentDifficultyFilter = TrackDifficulty.Easy;
    private TrackData _selectedTrack;
    private readonly List<Button> _difficultyTabs = new();
    private readonly Dictionary<string, Button> _trackButtons = new();
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _trackListContainer = GetNode<VBoxContainer>("%TrackListContainer");
        _trackDetailsPanel = GetNode<VBoxContainer>("%TrackDetailsPanel");
        _lblTrackName = GetNode<Label>("%LblTrackName");
        _lblTrackDescription = GetNode<Label>("%LblTrackDescription");
        _lblTrackInfo = GetNode<Label>("%LblTrackInfo");
        _btnPlay = GetNode<Button>("%BtnPlay");
        _btnBack = GetNode<Button>("%BtnBack");
        _lblCoins = GetNode<Label>("%LblCoins");
        _animPlayer = GetNode<AnimationPlayer>("%AnimPlayer");

        _btnPlay.Pressed += OnPlayPressed;
        _btnBack.Pressed += OnBackPressed;

        CreateDifficultyTabs();
        PopulateTrackList(_currentDifficultyFilter);
        
        // Select first unlocked track
        var gm = GameManager.Instance;
        var trackSelector = TrackSelector.Instance;
        if (gm?.SelectedTrack != null)
        {
            SelectTrack(gm.SelectedTrack);
        }
        else if (trackSelector != null)
        {
            var easyTracks = trackSelector.GetTracksByDifficulty(TrackDifficulty.Easy);
            if (easyTracks.Count > 0)
            {
                SelectTrack(easyTracks[0]);
            }
        }

        _animPlayer?.Play("entrance");
        AudioManager.Instance?.PlayMusic("res://assets/audio/music_menu.ogg");
    }

    public override void _Process(double delta)
    {
        if (GameManager.Instance != null)
        {
            _lblCoins.Text = $"{GameManager.Instance.Coins:N0} 🪙";
        }
    }

    private void CreateDifficultyTabs()
    {
        var tabContainer = GetNode<HBoxContainer>("%DifficultyTabs");
        var difficulties = Enum.GetValues<TrackDifficulty>();

        foreach (var diff in difficulties)
        {
            var btn = new Button
            {
                Text = GetDifficultyName(diff),
                ToggleMode = true,
                ButtonGroup = "difficulty_tabs",
                CustomMinimumSize = new Vector2(140, 50),
                ThemeOverrideFontSizes = { ["normal"] = 18 }
            };

            var colors = GetDifficultyColors(diff);
            var theme = new ButtonTheme
            {
                Normal = CreateStyleBox(colors.normal),
                Hover = CreateStyleBox(colors.hover),
                Pressed = CreateStyleBox(colors.pressed),
                Focus = CreateStyleBox(colors.hover)
            };
            btn.ThemeOverrideTheme = theme;

            var capturedDiff = diff;
            btn.Toggled += (pressed) =>
            {
                if (pressed)
                {
                    _currentDifficultyFilter = capturedDiff;
                    PopulateTrackList(capturedDiff);
                }
            };

            tabContainer.AddChild(btn);
            _difficultyTabs.Add(btn);
        }

        // Select Easy by default
        if (_difficultyTabs.Count > 0)
        {
            _difficultyTabs[0].ButtonPressed = true;
        }
    }

    private string GetDifficultyName(TrackDifficulty diff)
    {
        return diff switch
        {
            TrackDifficulty.Easy => "🟢 EASY",
            TrackDifficulty.Medium => "🟡 MEDIUM",
            TrackDifficulty.Hard => "🔴 HARD",
            TrackDifficulty.Expert => "🟣 EXPERT",
            TrackDifficulty.Insane => "⚫ INSANE",
            _ => diff.ToString()
        };
    }

    private (Color normal, Color hover, Color pressed) GetDifficultyColors(TrackDifficulty diff)
    {
        return diff switch
        {
            TrackDifficulty.Easy => (new Color(0.2f, 0.6f, 0.2f), new Color(0.3f, 0.8f, 0.3f), new Color(0.15f, 0.5f, 0.15f)),
            TrackDifficulty.Medium => (new Color(0.8f, 0.6f, 0.1f), new Color(1f, 0.8f, 0.2f), new Color(0.6f, 0.5f, 0.1f)),
            TrackDifficulty.Hard => (new Color(0.8f, 0.2f, 0.2f), new Color(1f, 0.3f, 0.3f), new Color(0.6f, 0.15f, 0.15f)),
            TrackDifficulty.Expert => (new Color(0.5f, 0.2f, 0.8f), new Color(0.7f, 0.3f, 1f), new Color(0.4f, 0.15f, 0.6f)),
            TrackDifficulty.Insane => (new Color(0.3f, 0.1f, 0.3f), new Color(0.5f, 0.2f, 0.5f), new Color(0.2f, 0.05f, 0.2f)),
            _ => (Colors.Gray, Colors.DarkGray, Colors.DimGray)
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

    private void PopulateTrackList(TrackDifficulty difficulty)
    {
        // Clear existing track buttons
        foreach (var child in _trackListContainer.GetChildren())
        {
            if (child is Button btn && _trackButtons.ContainsValue(btn))
            {
                child.QueueFree();
            }
        }
        _trackButtons.Clear();

        var trackSelector = TrackSelector.Instance;
        var gm = GameManager.Instance;
        if (trackSelector == null || gm == null) return;

        var tracks = trackSelector.GetTracksByDifficulty(difficulty);
        var unlockedTrackIds = gm.UnlockedTracks.Select(t => t.TrackId).ToHashSet();

        foreach (var track in tracks)
        {
            bool unlocked = track.IsUnlockedByDefault || unlockedTrackIds.Contains(track.TrackId);
            bool completed = gm.BestDistance >= track.Length; // Simplified

            var btn = new Button
            {
                Text = "",
                ToggleMode = true,
                ButtonGroup = "track_select",
                CustomMinimumSize = new Vector2(0, 100)
            };

            var hbox = new HBoxContainer { Separation = 20 };

            // Track preview/icon
            var preview = new TextureRect
            {
                CustomMinimumSize = new Vector2(160, 90),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Texture = track.BackgroundPath != null ? ResourceLoader.Load<Texture2D>(track.BackgroundPath) : null
            };
            if (preview.Texture == null)
            {
                var img = Image.Create(160, 90, false, Image.Format.Rgba8);
                img.Fill(track.GroundColor);
                preview.Texture = ImageTexture.CreateFromImage(img);
            }
            hbox.AddChild(preview);

            // Track info
            var vbox = new VBoxContainer();

            var nameLabel = new Label
            {
                Text = track.Name,
                HorizontalAlignment = HorizontalAlignment.Left,
                ThemeOverrideFontSizes = { ["normal"] = 20 },
                ThemeOverrideColors = { ["font_color"] = Colors.White }
            };
            vbox.AddChild(nameLabel);

            var descLabel = new Label
            {
                Text = track.Description,
                HorizontalAlignment = HorizontalAlignment.Left,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                ThemeOverrideFontSizes = { ["normal"] = 14 },
                ThemeOverrideColors = { ["font_color"] = new Color(0.8f, 0.8f, 0.9f) }
            };
            vbox.AddChild(descLabel);

            // Info line
            var infoLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                ThemeOverrideFontSizes = { ["normal"] = 13 }
            };
            string infoText = $"Length: {track.Length / 1000f:F1} km  |  Checkpoints: {track.EstimatedCheckpoints}  |  Est. Time: {track.EstimatedTimeMinutes} min";
            if (!unlocked)
            {
                infoText += $"  |  🔒 {track.UnlockCoins:N0} 🪙";
                infoLabel.ThemeOverrideColors = { ["font_color"] = Colors.Gray };
            }
            else if (completed)
            {
                infoText += "  |  ✓ COMPLETED";
                infoLabel.ThemeOverrideColors = { ["font_color"] = new Color(0.3f, 1f, 0.3f) };
            }
            else
            {
                infoLabel.ThemeOverrideColors = { ["font_color"] = new Color(0.7f, 0.9f, 1f) };
            }
            infoLabel.Text = infoText;
            vbox.AddChild(infoLabel);

            hbox.AddChild(vbox);
            btn.AddChild(hbox);

            _trackListContainer.AddChild(btn);
            _trackButtons[track.TrackId] = btn;

            if (!unlocked)
            {
                btn.Disabled = true;
            }

            var capturedTrack = track;
            btn.Toggled += (pressed) =>
            {
                if (pressed)
                {
                    SelectTrack(capturedTrack);
                }
            };
        }
    }

    private void SelectTrack(TrackData track)
    {
        _selectedTrack = track;
        var gm = GameManager.Instance;
        
        if (track != null)
        {
            _lblTrackName.Text = track.Name;
            _lblTrackDescription.Text = track.Description;
            
            bool unlocked = track.IsUnlockedByDefault || 
                           (gm?.UnlockedTracks.Any(t => t.TrackId == track.TrackId) ?? false);
            bool completed = gm?.BestDistance >= track.Length ?? false;

            if (!unlocked)
            {
                _btnPlay.Text = $"UNLOCK ({track.UnlockCoins:N0} 🪙)";
                _btnPlay.Disabled = gm?.Coins < track.UnlockCoins;
            }
            else
            {
                _btnPlay.Text = completed ? "REPLAY" : "PLAY";
                _btnPlay.Disabled = false;
            }

            var gravityText = track.GravityMultiplier != 1f ? $"Gravity: {track.GravityMultiplier}x Earth" : "Gravity: Normal (1x Earth)";
            _lblTrackInfo.Text = $@"
Difficulty: {track.Difficulty}
Length: {track.Length / 1000f:F1} km
Checkpoints: {track.EstimatedCheckpoints}
Estimated Time: {track.EstimatedTimeMinutes} min
{gravityText}
Theme: {track.TerrainTheme}
";
        }

        // Update button states
        foreach (var kvp in _trackButtons)
        {
            kvp.Value.ButtonPressed = kvp.Key == track.TrackId;
        }
    }

    private void OnPlayPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        var gm = GameManager.Instance;
        if (gm == null || _selectedTrack == null) return;

        bool unlocked = _selectedTrack.IsUnlockedByDefault || 
                       gm.UnlockedTracks.Any(t => t.TrackId == _selectedTrack.TrackId);

        if (!unlocked)
        {
            if (gm.Coins >= _selectedTrack.UnlockCoins)
            {
                gm.SpendCoins(_selectedTrack.UnlockCoins);
                gm.UnlockTrack(_selectedTrack);
                SaveSystem.Instance.SaveGame();
                // Refresh track list
                PopulateTrackList(_currentDifficultyFilter);
                SelectTrack(_selectedTrack);
            }
            return;
        }

        gm.SetSelectedTrack(_selectedTrack);
        GetTree().ChangeSceneToFile("res://scenes/Gameplay.tscn");
    }

    private void OnBackPressed()
    {
        AudioManager.Instance?.PlaySfx("res://assets/audio/ui_click.ogg");
        GetTree().ChangeSceneToFile("res://scenes/VehicleSelect.tscn");
    }
}