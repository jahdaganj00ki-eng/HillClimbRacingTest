using System;
using System.Collections.Generic;
using Godot;

namespace HillClimbRacing.Core;

/// <summary>
/// Manages checkpoints along the track and handles vehicle respawning.
/// When vehicle flips/crashes, respawns at last reached checkpoint after 2-second delay.
/// </summary>
[GlobalClass]
public partial class CheckpointSystem : Node
{
    public static CheckpointSystem Instance { get; private set; }

    [Export] public float RespawnDelay = 2.0f;
    [Export] public float CheckpointSpacing = 150f; // Distance between checkpoints in meters

    private List<Checkpoint> _checkpoints = new();
    private int _lastReachedCheckpoint = -1;
    private Timer _respawnTimer;
    private bool _isRespawning = false;
    private Vector2 _respawnPosition;
    private float _respawnRotation;

    [Signal] public delegate void OnCheckpointReachedEventHandler(int index, Vector2 position, float distance);
    [Signal] public delegate void OnRespawnStartedEventHandler();
    [Signal] public delegate void OnRespawnCompletedEventHandler(Vector2 position, float rotation);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;

        _respawnTimer = new Timer
        {
            WaitTime = RespawnDelay,
            OneShot = true,
            Autostart = false
        };
        _respawnTimer.Timeout += OnRespawnTimerTimeout;
        AddChild(_respawnTimer);
    }

    /// <summary>
    /// Initializes checkpoints for a track. Called when track loads.
    /// </summary>
    public void InitializeCheckpoints(TrackData trackData)
    {
        _checkpoints.Clear();
        _lastReachedCheckpoint = -1;

        // Generate checkpoints along the track path
        var trackPath = trackData.GetTrackPath();
        if (trackPath == null || trackPath.Count < 2)
        {
            GD.PrintErr("Invalid track path for checkpoint generation");
            return;
        }

        float accumulatedDistance = 0f;
        for (int i = 0; i < trackPath.Count - 1; i++)
        {
            Vector2 start = trackPath[i];
            Vector2 end = trackPath[i + 1];
            float segmentLength = start.DistanceTo(end);

            while (accumulatedDistance + segmentLength >= CheckpointSpacing)
            {
                float t = (CheckpointSpacing - accumulatedDistance) / segmentLength;
                t = Math.Clamp(t, 0f, 1f);
                Vector2 cpPosition = start.Lerp(end, t);
                
                // Calculate rotation from track direction
                Vector2 direction = (end - start).Normalized();
                float rotation = Mathf.Atan2(direction.Y, direction.X);
                
                _checkpoints.Add(new Checkpoint
                {
                    Index = _checkpoints.Count,
                    Position = cpPosition,
                    Rotation = rotation,
                    Distance = _checkpoints.Count * CheckpointSpacing
                });

                accumulatedDistance = 0f;
                start = cpPosition;
                segmentLength = start.DistanceTo(end);
            }

            accumulatedDistance += segmentLength;
        }

        // Add final checkpoint at track end
        Vector2 lastPoint = trackPath[^1];
        Vector2 secondLast = trackPath[^2];
        Vector2 finalDirection = (lastPoint - secondLast).Normalized();
        _checkpoints.Add(new Checkpoint
        {
            Index = _checkpoints.Count,
            Position = lastPoint,
            Rotation = Mathf.Atan2(finalDirection.Y, finalDirection.X),
            Distance = trackData.Length
        });

        GD.Print($"Initialized {_checkpoints.Count} checkpoints for track: {trackData.Name}");
    }

    /// <summary>
    /// Called when vehicle passes a checkpoint. Updates last reached checkpoint.
    /// </summary>
    public void OnVehiclePassedCheckpoint(Vector2 vehiclePosition, float vehicleRotation, float distanceTraveled)
    {
        if (_checkpoints.Count == 0) return;

        // Find the next checkpoint ahead of vehicle
        for (int i = _lastReachedCheckpoint + 1; i < _checkpoints.Count; i++)
        {
            var cp = _checkpoints[i];
            float distToCp = vehiclePosition.DistanceTo(cp.Position);
            
            // If vehicle is close to checkpoint and past it in terms of distance
            if (distToCp < 50f && distanceTraveled >= cp.Distance - 10f)
            {
                _lastReachedCheckpoint = i;
                _respawnPosition = cp.Position;
                _respawnRotation = cp.Rotation;
                
                EmitSignal(SignalName.OnCheckpointReached, i, cp.Position, cp.Distance);
                
                var gameManager = GameManager.Instance;
                if (gameManager != null)
                {
                    gameManager.OnCheckpointReached(i, cp.Distance);
                }
                break;
            }
        }
    }

    /// <summary>
    /// Triggers respawn sequence when vehicle flips or crashes.
    /// </summary>
    public void RequestRespawn(Vector2 currentPosition, float currentRotation)
    {
        if (_isRespawning) return;
        
        _isRespawning = true;
        
        // Use last reached checkpoint, or start if none reached
        if (_lastReachedCheckpoint >= 0 && _lastReachedCheckpoint < _checkpoints.Count)
        {
            var cp = _checkpoints[_lastReachedCheckpoint];
            _respawnPosition = cp.Position;
            _respawnRotation = cp.Rotation;
        }
        else if (_checkpoints.Count > 0)
        {
            // Respawn at start
            var cp = _checkpoints[0];
            _respawnPosition = cp.Position;
            _respawnRotation = cp.Rotation;
        }

        EmitSignal(SignalName.OnRespawnStarted);
        _respawnTimer.Start();
    }

    private void OnRespawnTimerTimeout()
    {
        _isRespawning = false;
        EmitSignal(SignalName.OnRespawnCompleted, _respawnPosition, _respawnRotation);
    }

    /// <summary>
    /// Gets the respawn transform for the vehicle.
    /// </summary>
    public (Vector2 Position, float Rotation) GetRespawnTransform()
    {
        return (_respawnPosition, _respawnRotation);
    }

    /// <summary>
    /// Resets checkpoint progress for new run.
    /// </summary>
    public void Reset()
    {
        _lastReachedCheckpoint = -1;
        _isRespawning = false;
        _respawnTimer.Stop();
        
        if (_checkpoints.Count > 0)
        {
            _respawnPosition = _checkpoints[0].Position;
            _respawnRotation = _checkpoints[0].Rotation;
        }
    }

    public int GetLastCheckpointIndex() => _lastReachedCheckpoint;
    public int GetTotalCheckpoints() => _checkpoints.Count;
    public float GetCheckpointDistance(int index) => index >= 0 && index < _checkpoints.Count ? _checkpoints[index].Distance : 0f;
}

public class Checkpoint
{
    public int Index { get; set; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public float Distance { get; set; }
}