using System;
using Godot;

namespace HillClimbRacing.Core;

/// <summary>
/// Base vehicle statistics and physics parameters.
/// Each vehicle type has unique base stats modified by tuning.
/// </summary>
[GlobalClass]
public partial class VehicleStats : Resource
{
    public VehicleType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Base physics parameters (before tuning)
    public float BaseMass { get; set; } = 1000f;           // kg
    public float BaseEnginePower { get; set; } = 150f;     // kW
    public float BaseEngineTorque { get; set; } = 300f;    // Nm
    public float BaseMaxRpm { get; set; } = 6000f;
    public float BaseRedlineRpm { get; set; } = 6500f;

    // Suspension
    public float BaseSuspensionTravel { get; set; } = 0.3f;    // meters
    public float BaseSpringStiffness { get; set; } = 35000f;   // N/m
    public float BaseDampingCompression { get; set; } = 3000f; // Ns/m
    public float BaseDampingRebound { get; set; } = 2000f;     // Ns/m
    public float BaseAntiRollBar { get; set; } = 10000f;       // Nm/rad

    // Wheels/Tires
    public float BaseFrontWheelRadius { get; set; } = 0.35f;
    public float BaseRearWheelRadius { get; set; } = 0.35f;
    public float BaseWheelWidth { get; set; } = 0.25f;
    public float BaseTireGrip { get; set; } = 1.2f;
    public float BaseTirePressure { get; set; } = 2.2f; // bar

    // Dimensions
    public float BaseWheelbase { get; set; } = 2.5f;      // meters
    public float BaseTrackWidth { get; set; } = 1.6f;     // meters
    public float BaseCenterOfGravityHeight { get; set; } = 0.6f; // meters

    // Drivetrain
    public DrivetrainType Drivetrain { get; set; } = DrivetrainType.RWD;
    public float BaseFrontTorqueSplit { get; set; } = 0f;  // 0 = RWD, 1 = FWD, 0.5 = 50/50
    public float BaseDiffLockStrength { get; set; } = 0f;  // 0 = open, 1 = locked

    // Visual
    public string SpritePath { get; set; }
    public Vector2 SpriteScale { get; set; } = Vector2.One;
    public Vector2 BodyOffset { get; set; } = Vector2.Zero;
    public Color BodyColor { get; set; } = Colors.White;

    // Unlock requirement
    public int UnlockCoins { get; set; } = 0;
    public bool IsUnlockedByDefault { get; set; } = false;

    /// <summary>
    /// Creates final stats by applying tuning multipliers.
    /// </summary>
    public TunedVehicleStats GetTunedStats(VehicleTuning tuning)
    {
        return new TunedVehicleStats
        {
            Type = Type,
            Mass = BaseMass * tuning.GetWeightMassMultiplier(),
            EnginePower = BaseEnginePower * tuning.GetEnginePowerMultiplier(),
            EngineTorque = BaseEngineTorque * tuning.GetEngineTorqueMultiplier(),
            MaxRpm = BaseMaxRpm,
            RedlineRpm = BaseRedlineRpm,
            SuspensionTravel = BaseSuspensionTravel * tuning.GetSuspensionTravelMultiplier(),
            SpringStiffness = BaseSpringStiffness * tuning.GetSuspensionDampingMultiplier(),
            DampingCompression = BaseDampingCompression * tuning.GetSuspensionDampingMultiplier(),
            DampingRebound = BaseDampingRebound * tuning.GetSuspensionDampingMultiplier(),
            AntiRollBar = BaseAntiRollBar * tuning.GetSuspensionDampingMultiplier(),
            FrontWheelRadius = BaseFrontWheelRadius,
            RearWheelRadius = BaseRearWheelRadius,
            WheelWidth = BaseWheelWidth * tuning.GetTireWidthMultiplier(),
            TireGrip = BaseTireGrip * tuning.GetTireGripMultiplier(),
            TirePressure = BaseTirePressure,
            Wheelbase = BaseWheelbase,
            TrackWidth = BaseTrackWidth,
            CenterOfGravityHeight = BaseCenterOfGravityHeight * tuning.GetWeightCoGMultiplier(),
            Drivetrain = Drivetrain,
            FrontTorqueSplit = BaseFrontTorqueSplit + tuning.GetDrivetrainFrontSplit(),
            DiffLockStrength = BaseDiffLockStrength + tuning.GetDrivetrainDiffLock()
        };
    }
}

/// <summary>
/// Final computed vehicle stats after tuning applied.
/// </summary>
public class TunedVehicleStats
{
    public VehicleType Type { get; set; }
    public float Mass { get; set; }
    public float EnginePower { get; set; }
    public float EngineTorque { get; set; }
    public float MaxRpm { get; set; }
    public float RedlineRpm { get; set; }
    public float SuspensionTravel { get; set; }
    public float SpringStiffness { get; set; }
    public float DampingCompression { get; set; }
    public float DampingRebound { get; set; }
    public float AntiRollBar { get; set; }
    public float FrontWheelRadius { get; set; }
    public float RearWheelRadius { get; set; }
    public float WheelWidth { get; set; }
    public float TireGrip { get; set; }
    public float TirePressure { get; set; }
    public float Wheelbase { get; set; }
    public float TrackWidth { get; set; }
    public float CenterOfGravityHeight { get; set; }
    public DrivetrainType Drivetrain { get; set; }
    public float FrontTorqueSplit { get; set; }
    public float DiffLockStrength { get; set; }
}

public enum DrivetrainType
{
    FWD,    // Front Wheel Drive
    RWD,    // Rear Wheel Drive
    AWD,    // All Wheel Drive (fixed split)
    FourWD  // 4WD (adjustable split with diff lock)
}