using System;
using Godot;

namespace HillClimbRacing.Core;

/// <summary>
/// Main gameplay scene - integrates track, vehicle, checkpoints, HUD, and handles game loop.
/// </summary>
[GlobalClass]
public partial class Gameplay : Node2D
{
    private TrackData _currentTrack;
    private VehicleBase _playerVehicle;
    private HUD _hud;
    private Camera2D _camera;
    private Node2D _trackContainer;
    private Timer _respawnCountdownTimer;
    private Label _respawnLabel;
    private bool _isRespawning = false;
    private float _respawnTimeRemaining = 0f;

    public override void _Ready()
    {
        // Get references
        _hud = GetNode<HUD>("HUD");
        _camera = GetNode<Camera2D>("Camera2D");
        _trackContainer = GetNode<Node2D>("TrackContainer");
        
        // Setup respawn timer
        _respawnCountdownTimer = new Timer
        {
            WaitTime = 0.1f,
            OneShot = false,
            Autostart = false
        };
        _respawnCountdownTimer.Timeout += OnRespawnCountdownTick;
        AddChild(_respawnCountdownTimer);

        // Respawn label
        _respawnLabel = new Label
        {
            Name = "RespawnLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = "",
            ThemeOverrideFontSizes = { ["normal"] = 64 },
            ThemeOverrideColors = { ["font_color"] = Colors.Red },
            AnchorsPreset = LayoutPreset.Center,
            Visible = false
        };
        AddChild(_respawnLabel);

        // Load selected track and vehicle
        var gm = GameManager.Instance;
        if (gm != null)
        {
            _currentTrack = gm.SelectedTrack;
            GenerateTrack();
            SpawnVehicle();
            InitializeCheckpoints();
        }

        // Connect checkpoint signals
        CheckpointSystem.Instance.OnCheckpointReached += OnCheckpointReached;
        CheckpointSystem.Instance.OnRespawnStarted += OnRespawnStarted;
        CheckpointSystem.Instance.OnRespawnCompleted += OnRespawnCompleted;

        // Camera follows player
        if (_playerVehicle != null && _camera != null)
        {
            _camera.TargetDescriptor = _playerVehicle.GetPath();
        }

        AudioManager.Instance?.PlayMusic(_currentTrack?.MusicTrack ?? "res://assets/audio/music_gameplay.ogg");
    }

    public override void _ExitTree()
    {
        CheckpointSystem.Instance.OnCheckpointReached -= OnCheckpointReached;
        CheckpointSystem.Instance.OnRespawnStarted -= OnRespawnStarted;
        CheckpointSystem.Instance.OnRespawnCompleted -= OnRespawnCompleted;
    }

    public override void _Process(double delta)
    {
        if (_isRespawning)
        {
            return;
        }

        // Handle pause
        if (Input.IsActionJustPressed("pause"))
        {
            TogglePause();
        }
    }

    private void GenerateTrack()
    {
        if (_currentTrack == null) return;

        var path = _currentTrack.GetTrackPath();
        if (path == null || path.Count < 2) return;

        // Create ground collision polygons from track path
        var groundBody = new StaticBody2D
        {
            Name = "Ground",
            CollisionLayer = 2,
            CollisionMask = 1
        };
        _trackContainer.AddChild(groundBody);

        // Create collision polygons along the track
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 p1 = path[i];
            Vector2 p2 = path[i + 1];
            
            float width = _currentTrack.GetTrackWidth(i * 10f);
            Vector2 dir = (p2 - p1).Normalized();
            Vector2 normal = new Vector2(-dir.Y, dir.X);
            
            var poly = new CollisionPolygon2D
            {
                Polygon = new Vector2[]
                {
                    p1 + normal * width / 2,
                    p1 - normal * width / 2,
                    p2 - normal * width / 2,
                    p2 + normal * width / 2
                },
                CollisionLayer = 2,
                CollisionMask = 1
            };
            groundBody.AddChild(poly);
        }

        // Add visual track (simple colored polygons for now)
        CreateTrackVisuals(path);
    }

    private void CreateTrackVisuals(System.Collections.Generic.List<Vector2> path)
    {
        var visual = new Polygon2D
        {
            Name = "TrackVisual",
            Color = _currentTrack?.GroundColor ?? new Color(0.3f, 0.5f, 0.2f),
            Polygon = new Vector2[0]
        };

        // Build polygon from path
        var polygonPoints = new System.Collections.Generic.List<Vector2>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 p1 = path[i];
            Vector2 p2 = path[i + 1];
            float width = _currentTrack.GetTrackWidth(i * 10f) / 2f;
            Vector2 dir = (p2 - p1).Normalized();
            Vector2 normal = new Vector2(-dir.Y, dir.X);
            
            polygonPoints.Add(p1 + normal * width);
            polygonPoints.Add(p1 - normal * width);
        }
        
        // Close the polygon
        if (polygonPoints.Count > 2)
        {
            Vector2 last = path[^1];
            Vector2 secondLast = path[^2];
            float width = _currentTrack.GetTrackWidth((path.Count - 1) * 10f) / 2f;
            Vector2 dir = (last - secondLast).Normalized();
            Vector2 normal = new Vector2(-dir.Y, dir.X);
            
            polygonPoints.Add(last - normal * width);
            polygonPoints.Add(last + normal * width);
        }

        visual.Polygon = polygonPoints.ToArray();
        _trackContainer.AddChild(visual);
    }

    private void SpawnVehicle()
    {
        var gm = GameManager.Instance;
        var factory = VehicleFactory.Instance;
        if (gm == null || factory == null || _currentTrack == null) return;

        var tuning = gm.GetTuning(gm.SelectedVehicle);
        var startPos = _currentTrack.GetTrackPath()?[0] ?? Vector2.Zero;
        startPos.Y -= 2f; // Start slightly above ground

        _playerVehicle = factory.SpawnVehicle(gm.SelectedVehicle, startPos, tuning);
        if (_playerVehicle != null)
        {
            _playerVehicle.Name = "PlayerVehicle";
            _trackContainer.AddChild(_playerVehicle);
            
            // Update camera target
            if (_camera != null)
            {
                _camera.TargetDescriptor = _playerVehicle.GetPath();
            }
            
            // Update HUD reference
            if (_hud != null)
            {
                // HUD will find vehicle in _Process
            }
        }
    }

    private void InitializeCheckpoints()
    {
        if (_currentTrack != null)
        {
            CheckpointSystem.Instance.InitializeCheckpoints(_currentTrack);
        }
    }

    private void OnCheckpointReached(int index, Vector2 position, float distance)
    {
        _hud?.OnCheckpointReached(index, distance);
    }

    private void OnRespawnStarted()
    {
        _isRespawning = true;
        _respawnTimeRemaining = CheckpointSystem.Instance.RespawnDelay;
        _respawnLabel.Text = $"RESPAWNING IN {_respawnTimeRemaining:F1}s...";
        _respawnLabel.Visible = true;
        _respawnCountdownTimer.Start();
        
        if (_playerVehicle != null)
        {
            _playerVehicle.Visible = false;
        }
    }

    private void OnRespawnCountdownTick()
    {
        _respawnTimeRemaining -= 0.1f;
        if (_respawnTimeRemaining > 0)
        {
            _respawnLabel.Text = $"RESPAWNING IN {_respawnTimeRemaining:F1}s...";
        }
    }

    private void OnRespawnCompleted(Vector2 position, float rotation)
    {
        _respawnCountdownTimer.Stop();
        _respawnLabel.Visible = false;
        _isRespawning = false;

        if (_playerVehicle != null)
        {
            _playerVehicle.RespawnAt(position, rotation);
            _playerVehicle.Visible = true;
        }
    }

    private void TogglePause()
    {
        GetTree().Paused = !GetTree().Paused;
        if (GetTree().Paused)
        {
            GetTree().ChangeSceneToFile("res://scenes/PauseMenu.tscn");
        }
    }
}