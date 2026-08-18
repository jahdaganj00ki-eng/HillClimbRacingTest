using System;
using System.Collections.Generic;
using Godot;

namespace HillClimbRacing.Core;

/// <summary>
/// Global game state manager. Handles current vehicle, track, progress, and save/load coordination.
/// </summary>
[GlobalClass]
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    // Current session state
    public VehicleType SelectedVehicle { get; set; } = VehicleType.Jeep;
    public TrackData SelectedTrack { get; set; }
    public int CurrentCheckpointIndex { get; set; } = 0;
    public float CurrentDistance { get; set; } = 0f;
    public float BestDistance { get; set; } = 0f;
    public int Coins { get; set; } = 0;
    public int TotalCoinsCollected { get; set; } = 0;

    // Vehicle tuning levels (0-10)
    public Dictionary<VehicleType, VehicleTuning> VehicleTunings { get; } = new();

    // Unlocked content
    public HashSet<VehicleType> UnlockedVehicles { get; } = new() { VehicleType.Jeep };
    public HashSet<TrackData> UnlockedTracks { get; } = new();

    // Events
    [Signal] public delegate void OnVehicleChangedEventHandler(VehicleType vehicle);
    [Signal] public delegate void OnTrackChangedEventHandler(TrackData track);
    [Signal] public delegate void OnCheckpointReachedEventHandler(int checkpointIndex, float distance);
    [Signal] public delegate void OnVehicleFlippedEventHandler();
    [Signal] public delegate void OnCoinCollectedEventHandler(int amount);
    [Signal] public delegate void OnDataSavedEventHandler();

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        
        // Initialize default tunings for all vehicles
        foreach (VehicleType type in Enum.GetValues<VehicleType>())
        {
            VehicleTunings[type] = new VehicleTuning();
        }

        // Load saved data
        SaveSystem.Instance.LoadGame();
        
        // Unlock starter tracks
        UnlockStarterTracks();
    }

    private void UnlockStarterTracks()
    {
        var trackSelector = TrackSelector.Instance;
        if (trackSelector != null)
        {
            var easyTracks = trackSelector.GetTracksByDifficulty(TrackDifficulty.Easy);
            foreach (var track in easyTracks)
            {
                UnlockedTracks.Add(track);
            }
        }
    }

    public void SetSelectedVehicle(VehicleType vehicle)
    {
        if (UnlockedVehicles.Contains(vehicle))
        {
            SelectedVehicle = vehicle;
            EmitSignal(SignalName.OnVehicleChanged, (int)vehicle);
        }
    }

    public void SetSelectedTrack(TrackData track)
    {
        if (UnlockedTracks.Contains(track) || track.Difficulty == TrackDifficulty.Easy)
        {
            SelectedTrack = track;
            CurrentCheckpointIndex = 0;
            CurrentDistance = 0f;
            EmitSignal(SignalName.OnTrackChanged, track);
        }
    }

    public void OnCheckpointReached(int index, float distance)
    {
        CurrentCheckpointIndex = index;
        CurrentDistance = distance;
        if (distance > BestDistance)
        {
            BestDistance = distance;
        }
        EmitSignal(SignalName.OnCheckpointReached, index, distance);
    }

    public void OnVehicleFlipped()
    {
        EmitSignal(SignalName.OnVehicleFlipped);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        TotalCoinsCollected += amount;
        EmitSignal(SignalName.OnCoinCollected, amount);
    }

    public void SpendCoins(int amount)
    {
        Coins = Math.Max(0, Coins - amount);
    }

    public void UnlockVehicle(VehicleType vehicle)
    {
        UnlockedVehicles.Add(vehicle);
    }

    public void UnlockTrack(TrackData track)
    {
        UnlockedTracks.Add(track);
    }

    public VehicleTuning GetTuning(VehicleType vehicle)
    {
        return VehicleTunings.GetValueOrDefault(vehicle, new VehicleTuning());
    }

    public void UpgradeTuning(VehicleType vehicle, TuningCategory category)
    {
        var tuning = GetTuning(vehicle);
        tuning.Upgrade(category);
        SaveSystem.Instance.SaveGame();
    }

    public void ResetProgress()
    {
        CurrentCheckpointIndex = 0;
        CurrentDistance = 0f;
        BestDistance = 0f;
        Coins = 0;
        TotalCoinsCollected = 0;
        UnlockedVehicles.Clear();
        UnlockedVehicles.Add(VehicleType.Jeep);
        UnlockedTracks.Clear();
        UnlockStarterTracks();
        
        foreach (var tuning in VehicleTunings.Values)
        {
            tuning.Reset();
        }
        
        SaveSystem.Instance.SaveGame();
    }
}

/// <summary>
/// Vehicle types available in the game
/// </summary>
public enum VehicleType
{
    Jeep,           // Starter - Balanced, RWD
    PickupTruck,    // Heavy, 4WD, high torque
    SportsCar,      // Fast, RWD, low suspension
    MonsterTruck,   // Huge wheels, 4WD, high suspension
    MoonBuggy,      // Low gravity specialist, 4WD
    Tank            // Tracks instead of wheels, unlockable
}

/// <summary>
/// Tuning categories for vehicle upgrades
/// </summary>
public enum TuningCategory
{
    Engine,         // Power, torque, max RPM
    Suspension,     // Travel, damping, anti-roll
    Tires,          // Grip, width, pressure
    Weight,         // Mass reduction, CoG lowering
    Drivetrain      // 4WD split, diff lock
}

/// <summary>
/// Vehicle tuning data (5 categories, 10 levels each)
/// </summary>
[GlobalClass]
public partial class VehicleTuning : Resource
{
    public int EngineLevel { get; private set; } = 0;
    public int SuspensionLevel { get; private set; } = 0;
    public int TiresLevel { get; private set; } = 0;
    public int WeightLevel { get; private set; } = 0;
    public int DrivetrainLevel { get; private set; } = 0;

    public const int MaxLevel = 10;
    public static readonly int[] UpgradeCosts = { 100, 250, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000 };

    public void Upgrade(TuningCategory category)
    {
        int currentLevel = GetLevel(category);
        if (currentLevel < MaxLevel)
        {
            SetLevel(category, currentLevel + 1);
        }
    }

    public int GetLevel(TuningCategory category)
    {
        return category switch
        {
            TuningCategory.Engine => EngineLevel,
            TuningCategory.Suspension => SuspensionLevel,
            TuningCategory.Tires => TiresLevel,
            TuningCategory.Weight => WeightLevel,
            TuningCategory.Drivetrain => DrivetrainLevel,
            _ => 0
        };
    }

    public void SetLevel(TuningCategory category, int level)
    {
        level = Math.Clamp(level, 0, MaxLevel);
        switch (category)
        {
            case TuningCategory.Engine: EngineLevel = level; break;
            case TuningCategory.Suspension: SuspensionLevel = level; break;
            case TuningCategory.Tires: TiresLevel = level; break;
            case TuningCategory.Weight: WeightLevel = level; break;
            case TuningCategory.Drivetrain: DrivetrainLevel = level; break;
        }
    }

    public int GetUpgradeCost(TuningCategory category)
    {
        int level = GetLevel(category);
        if (level >= MaxLevel) return -1;
        return UpgradeCosts[level];
    }

    public float GetEnginePowerMultiplier() => 1f + EngineLevel * 0.15f;
    public float GetEngineTorqueMultiplier() => 1f + EngineLevel * 0.12f;
    public float GetSuspensionTravelMultiplier() => 1f + SuspensionLevel * 0.10f;
    public float GetSuspensionDampingMultiplier() => 1f + SuspensionLevel * 0.08f;
    public float GetTireGripMultiplier() => 1f + TiresLevel * 0.10f;
    public float GetTireWidthMultiplier() => 1f + TiresLevel * 0.05f;
    public float GetWeightMassMultiplier() => 1f - WeightLevel * 0.08f;
    public float GetWeightCoGMultiplier() => 1f - WeightLevel * 0.05f;
    public float GetDrivetrainFrontSplit() => DrivetrainLevel * 0.1f; // 0% to 100% front
    public float GetDrivetrainDiffLock() => DrivetrainLevel * 0.1f;   // 0% to 100% lock

    public void Reset()
    {
        EngineLevel = 0;
        SuspensionLevel = 0;
        TiresLevel = 0;
        WeightLevel = 0;
        DrivetrainLevel = 0;
    }
}