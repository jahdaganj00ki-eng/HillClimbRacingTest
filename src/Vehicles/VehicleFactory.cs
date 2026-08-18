using System;
using System.Collections.Generic;
using Godot;

namespace HillClimbRacing.Vehicles;

/// <summary>
/// Factory for creating and configuring vehicle instances with base stats and tuning.
/// </summary>
[GlobalClass]
public partial class VehicleFactory : Node
{
    public static VehicleFactory Instance { get; private set; }

    private readonly Dictionary<VehicleType, VehicleStats> _baseStats = new();
    private readonly Dictionary<VehicleType, PackedScene> _vehicleScenes = new();

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;

        InitializeBaseStats();
        LoadVehicleScenes();
    }

    private void InitializeBaseStats()
    {
        // ===== JEEP (Starter) =====
        _baseStats[VehicleType.Jeep] = new VehicleStats
        {
            Type = VehicleType.Jeep,
            Name = "Jeep",
            Description = "The classic all-rounder. Balanced performance on any terrain.",
            BaseMass = 1200f,
            BaseEnginePower = 120f,
            BaseEngineTorque = 280f,
            BaseMaxRpm = 6000f,
            BaseRedlineRpm = 6500f,
            BaseSuspensionTravel = 0.25f,
            BaseSpringStiffness = 30000f,
            BaseDampingCompression = 2500f,
            BaseDampingRebound = 1800f,
            BaseAntiRollBar = 8000f,
            BaseFrontWheelRadius = 0.35f,
            BaseRearWheelRadius = 0.35f,
            BaseWheelWidth = 0.25f,
            BaseTireGrip = 1.1f,
            BaseWheelbase = 2.4f,
            BaseTrackWidth = 1.5f,
            BaseCenterOfGravityHeight = 0.65f,
            Drivetrain = DrivetrainType.RWD,
            BaseFrontTorqueSplit = 0f,
            BaseDiffLockStrength = 0f,
            UnlockCoins = 0,
            IsUnlockedByDefault = true,
            SpritePath = "res://assets/vehicles/jeep.png",
            BodyColor = new Color(0.2f, 0.5f, 0.2f)
        };

        // ===== PICKUP TRUCK =====
        _baseStats[VehicleType.PickupTruck] = new VehicleStats
        {
            Type = VehicleType.PickupTruck,
            Name = "Pickup Truck",
            Description = "Heavy hauler with 4WD. Incredible torque for climbing, but slow.",
            BaseMass = 2200f,
            BaseEnginePower = 150f,
            BaseEngineTorque = 500f,
            BaseMaxRpm = 5000f,
            BaseRedlineRpm = 5500f,
            BaseSuspensionTravel = 0.3f,
            BaseSpringStiffness = 45000f,
            BaseDampingCompression = 4000f,
            BaseDampingRebound = 3000f,
            BaseAntiRollBar = 12000f,
            BaseFrontWheelRadius = 0.4f,
            BaseRearWheelRadius = 0.4f,
            BaseWheelWidth = 0.3f,
            BaseTireGrip = 1.3f,
            BaseWheelbase = 3.0f,
            BaseTrackWidth = 1.7f,
            BaseCenterOfGravityHeight = 0.8f,
            Drivetrain = DrivetrainType.FourWD,
            BaseFrontTorqueSplit = 0.4f,
            BaseDiffLockStrength = 0.3f,
            UnlockCoins = 25000,
            IsUnlockedByDefault = false,
            SpritePath = "res://assets/vehicles/pickup.png",
            BodyColor = new Color(0.4f, 0.3f, 0.2f)
        };

        // ===== SPORTS CAR =====
        _baseStats[VehicleType.SportsCar] = new VehicleStats
        {
            Type = VehicleType.SportsCar,
            Name = "Sports Car",
            Description = "Lightweight speed demon. Low suspension, RWD. Dominates flat tracks.",
            BaseMass = 900f,
            BaseEnginePower = 250f,
            BaseEngineTorque = 350f,
            BaseMaxRpm = 8000f,
            BaseRedlineRpm = 8500f,
            BaseSuspensionTravel = 0.12f,
            BaseSpringStiffness = 60000f,
            BaseDampingCompression = 5000f,
            BaseDampingRebound = 4000f,
            BaseAntiRollBar = 20000f,
            BaseFrontWheelRadius = 0.3f,
            BaseRearWheelRadius = 0.3f,
            BaseWheelWidth = 0.28f,
            BaseTireGrip = 1.5f,
            BaseWheelbase = 2.3f,
            BaseTrackWidth = 1.6f,
            BaseCenterOfGravityHeight = 0.4f,
            Drivetrain = DrivetrainType.RWD,
            BaseFrontTorqueSplit = 0f,
            BaseDiffLockStrength = 0f,
            UnlockCoins = 50000,
            IsUnlockedByDefault = false,
            SpritePath = "res://assets/vehicles/sports_car.png",
            BodyColor = new Color(0.8f, 0.1f, 0.1f)
        };

        // ===== MONSTER TRUCK =====
        _baseStats[VehicleType.MonsterTruck] = new VehicleStats
        {
            Type = VehicleType.MonsterTruck,
            Name = "Monster Truck",
            Description = "Massive wheels eat any terrain. 4WD, huge suspension travel. Slow but unstoppable.",
            BaseMass = 3500f,
            BaseEnginePower = 300f,
            BaseEngineTorque = 800f,
            BaseMaxRpm = 5500f,
            BaseRedlineRpm = 6000f,
            BaseSuspensionTravel = 0.6f,
            BaseSpringStiffness = 25000f,
            BaseDampingCompression = 3000f,
            BaseDampingRebound = 2500f,
            BaseAntiRollBar = 5000f,
            BaseFrontWheelRadius = 0.65f,
            BaseRearWheelRadius = 0.65f,
            BaseWheelWidth = 0.45f,
            BaseTireGrip = 1.4f,
            BaseWheelbase = 2.8f,
            BaseTrackWidth = 2.2f,
            BaseCenterOfGravityHeight = 1.0f,
            Drivetrain = DrivetrainType.FourWD,
            BaseFrontTorqueSplit = 0.5f,
            BaseDiffLockStrength = 0.5f,
            UnlockCoins = 100000,
            IsUnlockedByDefault = false,
            SpritePath = "res://assets/vehicles/monster_truck.png",
            BodyColor = new Color(0.1f, 0.1f, 0.6f)
        };

        // ===== MOON BUGGY =====
        _baseStats[VehicleType.MoonBuggy] = new VehicleStats
        {
            Type = VehicleType.MoonBuggy,
            Name = "Moon Buggy",
            Description = "Built for low gravity. Wire wheels, 4WD independent motors. Floats over craters.",
            BaseMass = 400f,
            BaseEnginePower = 80f, // Electric motors
            BaseEngineTorque = 400f, // Instant torque
            BaseMaxRpm = 10000f,
            BaseRedlineRpm = 12000f,
            BaseSuspensionTravel = 0.5f,
            BaseSpringStiffness = 15000f,
            BaseDampingCompression = 1000f,
            BaseDampingRebound = 800f,
            BaseAntiRollBar = 3000f,
            BaseFrontWheelRadius = 0.4f,
            BaseRearWheelRadius = 0.4f,
            BaseWheelWidth = 0.2f,
            BaseTireGrip = 0.9f, // Low grip for sliding control
            BaseWheelbase = 2.0f,
            BaseTrackWidth = 1.8f,
            BaseCenterOfGravityHeight = 0.5f,
            Drivetrain = DrivetrainType.AWD, // Independent electric motors
            BaseFrontTorqueSplit = 0.5f,
            BaseDiffLockStrength = 0f,
            UnlockCoins = 200000,
            IsUnlockedByDefault = false,
            SpritePath = "res://assets/vehicles/moon_buggy.png",
            BodyColor = new Color(0.7f, 0.7f, 0.8f)
        };

        // ===== TANK (Unlockable) =====
        _baseStats[VehicleType.Tank] = new VehicleStats
        {
            Type = VehicleType.Tank,
            Name = "Tank",
            Description = "Tracks instead of wheels. Near-indestructible. Crushing power. The ultimate vehicle.",
            BaseMass = 8000f,
            BaseEnginePower = 500f,
            BaseEngineTorque = 2000f,
            BaseMaxRpm = 3000f,
            BaseRedlineRpm = 3500f,
            BaseSuspensionTravel = 0.4f, // Track flex
            BaseSpringStiffness = 100000f,
            BaseDampingCompression = 10000f,
            BaseDampingRebound = 8000f,
            BaseAntiRollBar = 50000f,
            BaseFrontWheelRadius = 0.5f, // Not used - tracks
            BaseRearWheelRadius = 0.5f,
            BaseWheelWidth = 0.6f, // Track width
            BaseTireGrip = 2.0f, // Tracks grip everything
            BaseWheelbase = 3.5f,
            BaseTrackWidth = 2.5f,
            BaseCenterOfGravityHeight = 0.7f,
            Drivetrain = DrivetrainType.FourWD,
            BaseFrontTorqueSplit = 0.5f,
            BaseDiffLockStrength = 1f, // Fully locked
            UnlockCoins = 1000000,
            IsUnlockedByDefault = false,
            SpritePath = "res://assets/vehicles/tank.png",
            BodyColor = new Color(0.2f, 0.3f, 0.15f)
        };
    }

    private void LoadVehicleScenes()
    {
        // In a full implementation, these would be loaded from .tscn files
        // For now, we create vehicles programmatically
        _vehicleScenes[VehicleType.Jeep] = null; // Will instantiate Jeep class directly
        _vehicleScenes[VehicleType.PickupTruck] = null;
        _vehicleScenes[VehicleType.SportsCar] = null;
        _vehicleScenes[VehicleType.MonsterTruck] = null;
        _vehicleScenes[VehicleType.MoonBuggy] = null;
        _vehicleScenes[VehicleType.Tank] = null;
    }

    /// <summary>
    /// Creates a vehicle instance with the given type and tuning.
    /// </summary>
    public VehicleBase CreateVehicle(VehicleType type, VehicleTuning tuning = null)
    {
        if (!_baseStats.TryGetValue(type, out var stats))
        {
            GD.PrintErr($"No base stats for vehicle type: {type}");
            return null;
        }

        tuning ??= new VehicleTuning();
        var tunedStats = stats.GetTunedStats(tuning);

        VehicleBase vehicle = type switch
        {
            VehicleType.Jeep => new Jeep(),
            VehicleType.PickupTruck => new PickupTruck(),
            VehicleType.SportsCar => new SportsCar(),
            VehicleType.MonsterTruck => new MonsterTruck(),
            VehicleType.MoonBuggy => new MoonBuggy(),
            VehicleType.Tank => new Tank(),
            _ => null
        };

        if (vehicle != null)
        {
            vehicle.BaseStats = stats;
            vehicle.Initialize(tunedStats, tuning);
            vehicle.Name = stats.Name;
        }

        return vehicle;
    }

    /// <summary>
    /// Creates a vehicle and adds it to the scene tree at the given position.
    /// </summary>
    public VehicleBase SpawnVehicle(VehicleType type, Vector2 position, VehicleTuning tuning = null)
    {
        var vehicle = CreateVehicle(type, tuning);
        if (vehicle != null)
        {
            vehicle.GlobalPosition = position;
            GetTree().CurrentScene.AddChild(vehicle);
        }
        return vehicle;
    }

    public VehicleStats GetBaseStats(VehicleType type)
    {
        return _baseStats.GetValueOrDefault(type);
    }

    public List<VehicleType> GetAllVehicleTypes()
    {
        return new List<VehicleType>(_baseStats.Keys);
    }

    public List<VehicleType> GetUnlockedVehicles(HashSet<VehicleType> unlockedSet)
    {
        return _baseStats.Keys
            .Where(t => _baseStats[t].IsUnlockedByDefault || unlockedSet.Contains(t))
            .ToList();
    }

    public int GetUnlockCost(VehicleType type)
    {
        return _baseStats.TryGetValue(type, out var stats) ? stats.UnlockCoins : -1;
    }

    public bool IsUnlockedByDefault(VehicleType type)
    {
        return _baseStats.TryGetValue(type, out var stats) && stats.IsUnlockedByDefault;
    }
}