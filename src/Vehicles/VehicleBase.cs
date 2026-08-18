using System;
using Godot;

namespace HillClimbRacing.Vehicles;

/// <summary>
/// Abstract base class for all vehicles. Handles physics, input, and state.
/// </summary>
[GlobalClass]
public partial class VehicleBase : RigidBody2D
{
    // Configuration
    [Export] public VehicleStats BaseStats { get; set; }
    [Export] public float FlipThresholdDegrees = 120f; // Degrees before considered "flipped"
    [Export] public float CrashImpulseThreshold = 5000f; // Impulse magnitude for crash detection

    // Runtime state
    protected TunedVehicleStats _tunedStats;
    protected VehicleTuning _tuning;
    protected bool _isGrounded = false;
    protected int _groundedWheels = 0;
    protected float _currentRpm = 0f;
    protected float _throttleInput = 0f;
    protected float _brakeInput = 0f;
    protected bool _boostActive = false;
    protected bool _isFlipped = false;
    protected bool _hasCrashed = false;
    
    // Wheel references (set in derived classes)
    protected RigidBody2D _frontWheel;
    protected RigidBody2D _rearWheel;
    protected PinJoint2D _frontSuspension;
    protected PinJoint2D _rearSuspension;
    protected DampedSpringJoint2D _frontSpring;
    protected DampedSpringJoint2D _rearSpring;

    // Engine simulation
    protected float _engineTorque = 0f;
    protected GearState _gearState = GearState.Neutral;
    protected float _clutchEngagement = 1f;

    // Visual
    protected Sprite2D _bodySprite;
    protected Sprite2D _frontWheelSprite;
    protected Sprite2D _rearWheelSprite;
    protected AnimationPlayer _animationPlayer;
    protected ParticleSystem2D _dustParticles;
    protected ParticleSystem2D _exhaustParticles;

    // Flip detection
    private float _timeUpsideDown = 0f;
    private const float FLIP_TIME_THRESHOLD = 1.5f; // Seconds upside down before respawn

    public override void _Ready()
    {
        // Initialize physics properties
        Mass = _tunedStats?.Mass ?? BaseStats?.BaseMass ?? 1000f;
        CenterOfMassOffset = new Vector2(0, -_tunedStats?.CenterOfGravityHeight ?? BaseStats?.BaseCenterOfGravityHeight ?? 0.6f);
        GravityScale = 1f;
        AngularDamp = 0.1f;
        LinearDamp = 0.01f;

        // Setup collision layers
        CollisionLayer = 1; // Vehicle layer
        CollisionMask = 2 | 4; // Ground + Obstacles

        // Connect signals
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        // Initialize visual components
        InitializeVisuals();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hasCrashed || _isFlipped) return;

        float dt = (float)delta;
        
        // Update grounded state
        UpdateGroundedState();
        
        // Handle flip detection
        UpdateFlipDetection(dt);
        
        // Process input
        ProcessInput();
        
        // Simulate engine and drivetrain
        SimulateEngine(dt);
        
        // Apply forces to wheels
        ApplyWheelForces(dt);
        
        // Update suspension
        UpdateSuspension(dt);
        
        // Update visuals
        UpdateVisuals(dt);
        
        // Update audio
        UpdateAudio(dt);
    }

    public virtual void Initialize(TunedVehicleStats stats, VehicleTuning tuning)
    {
        _tunedStats = stats;
        _tuning = tuning;
        Mass = stats.Mass;
        CenterOfMassOffset = new Vector2(0, -stats.CenterOfGravityHeight);
    }

    protected virtual void InitializeVisuals()
    {
        // Body sprite
        _bodySprite = new Sprite2D
        {
            Texture = BaseStats?.SpritePath != null ? ResourceLoader.Load<Texture2D>(BaseStats.SpritePath) : null,
            Scale = BaseStats?.SpriteScale ?? Vector2.One,
            Position = BaseStats?.BodyOffset ?? Vector2.Zero,
            ZIndex = 1
        };
        AddChild(_bodySprite);

        // Wheel sprites (will be positioned in _PhysicsProcess)
        _frontWheelSprite = new Sprite2D { ZIndex = 2 };
        _rearWheelSprite = new Sprite2D { ZIndex = 2 };
        AddChild(_frontWheelSprite);
        AddChild(_rearWheelSprite);

        // Dust particles
        _dustParticles = new GpuParticles2D
        {
            Emitting = false,
            Amount = 20,
            Lifetime = 0.5f,
            OneShot = false,
            Explosiveness = 0.5f,
            SpeedScale = 50f,
            Scale = 0.5f,
            Color = new Color(0.6f, 0.5f, 0.4f, 0.6f),
            ZIndex = 0
        };
        AddChild(_dustParticles);

        // Exhaust particles
        _exhaustParticles = new GpuParticles2D
        {
            Emitting = false,
            Amount = 30,
            Lifetime = 0.3f,
            OneShot = false,
            Explosiveness = 0.3f,
            SpeedScale = 100f,
            Scale = 0.3f,
            Color = new Color(0.3f, 0.3f, 0.3f, 0.5f),
            ZIndex = -1
        };
        AddChild(_exhaustParticles);
    }

    protected virtual void ProcessInput()
    {
        _throttleInput = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        _brakeInput = Input.GetActionStrength("brake");
        _boostActive = Input.IsActionPressed("boost");
    }

    protected virtual void SimulateEngine(float dt)
    {
        if (_tunedStats == null) return;

        // Simple engine RPM simulation
        float targetRpm = 800f; // Idle RPM
        
        if (Mathf.Abs(_throttleInput) > 0.1f && _isGrounded)
        {
            // Throttle applied - rev engine
            float throttleFactor = Mathf.Abs(_throttleInput);
            targetRpm = Mathf.Lerp(1000f, _tunedStats.MaxRpm, throttleFactor);
            
            if (_boostActive)
            {
                targetRpm = _tunedStats.RedlineRpm;
            }
        }
        else if (!_isGrounded)
        {
            // In air - engine revs freely
            targetRpm = Mathf.Lerp(_currentRpm, _tunedStats.MaxRpm * 0.8f, dt * 2f);
        }

        // Smooth RPM changes
        float rpmRate = _throttleInput > 0 ? 3000f : 1500f; // Accel vs decel rate
        _currentRpm = Mathf.MoveToward(_currentRpm, targetRpm, rpmRate * dt);
        _currentRpm = Mathf.Clamp(_currentRpm, 500f, _tunedStats.RedlineRpm * 1.1f);

        // Calculate engine torque based on RPM curve
        float rpmRatio = _currentRpm / _tunedStats.MaxRpm;
        float torqueCurve = Mathf.Sin(rpmRatio * Mathf.Pi * 0.8f); // Peak torque at ~80% RPM
        _engineTorque = _tunedStats.EngineTorque * torqueCurve * Mathf.Abs(_throttleInput);
        
        if (_boostActive)
        {
            _engineTorque *= 1.5f; // Boost multiplier
        }
    }

    protected virtual void ApplyWheelForces(float dt)
    {
        if (_tunedStats == null || !_isGrounded) return;

        // Distribute torque based on drivetrain
        float frontTorque = 0f;
        float rearTorque = 0f;

        switch (_tunedStats.Drivetrain)
        {
            case DrivetrainType.FWD:
                frontTorque = _engineTorque;
                break;
            case DrivetrainType.RWD:
                rearTorque = _engineTorque;
                break;
            case DrivetrainType.AWD:
                frontTorque = _engineTorque * _tunedStats.FrontTorqueSplit;
                rearTorque = _engineTorque * (1f - _tunedStats.FrontTorqueSplit);
                break;
            case DrivetrainType.FourWD:
                // With diff lock
                float totalTorque = _engineTorque;
                float lockFactor = _tunedStats.DiffLockStrength;
                frontTorque = totalTorque * _tunedStats.FrontTorqueSplit;
                rearTorque = totalTorque * (1f - _tunedStats.FrontTorqueSplit);
                
                // Diff lock transfers torque to wheel with more grip
                if (lockFactor > 0.1f && _frontWheel != null && _rearWheel != null)
                {
                    // Simplified: if one wheel slips, transfer torque
                    // This would need actual slip detection in a full implementation
                }
                break;
        }

        // Apply braking torque
        float brakeTorque = _brakeInput * 5000f * Mass / 1000f;
        frontTorque -= brakeTorque * 0.7f; // Front brake bias
        rearTorque -= brakeTorque * 0.3f;

        // Apply torque to wheels via angular impulse
        if (_frontWheel != null && frontTorque != 0)
        {
            _frontWheel.ApplyTorqueImpulse(frontTorque * dt / _tunedStats.FrontWheelRadius);
        }
        if (_rearWheel != null && rearTorque != 0)
        {
            _rearWheel.ApplyTorqueImpulse(rearTorque * dt / _tunedStats.RearWheelRadius);
        }

        // Apply lateral friction (cornering forces)
        ApplyLateralForces();
    }

    protected virtual void ApplyLateralForces()
    {
        // Simplified lateral force to prevent infinite sliding
        if (_frontWheel != null)
        {
            Vector2 vel = _frontWheel.LinearVelocity;
            Vector2 forward = _frontWheel.Transform.X;
            float lateralSpeed = vel.Dot(forward.Rotated(Mathf.Pi / 2));
            
            if (Mathf.Abs(lateralSpeed) > 10f)
            {
                float frictionForce = lateralSpeed * _tunedStats.TireGrip * 100f;
                _frontWheel.ApplyCentralForce(-forward.Rotated(Mathf.Pi / 2) * frictionForce);
            }
        }
        
        if (_rearWheel != null)
        {
            Vector2 vel = _rearWheel.LinearVelocity;
            Vector2 forward = _rearWheel.Transform.X;
            float lateralSpeed = vel.Dot(forward.Rotated(Mathf.Pi / 2));
            
            if (Mathf.Abs(lateralSpeed) > 10f)
            {
                float frictionForce = lateralSpeed * _tunedStats.TireGrip * 100f;
                _rearWheel.ApplyCentralForce(-forward.Rotated(Mathf.Pi / 2) * frictionForce);
            }
        }
    }

    protected virtual void UpdateSuspension(float dt)
    {
        // Suspension handled by DampedSpringJoint2D in Godot
        // This is for visual wheel positioning and effects
    }

    protected virtual void UpdateGroundedState()
    {
        // Count grounded wheels
        int grounded = 0;
        if (_frontWheel != null && IsWheelGrounded(_frontWheel)) grounded++;
        if (_rearWheel != null && IsWheelGrounded(_rearWheel)) grounded++;
        
        _groundedWheels = grounded;
        _isGrounded = grounded > 0;
    }

    protected virtual bool IsWheelGrounded(RigidBody2D wheel)
    {
        // Raycast down from wheel center
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            wheel.GlobalPosition,
            wheel.GlobalPosition + Vector2.Down * (_tunedStats?.FrontWheelRadius ?? 0.35f + 0.1f)
        );
        query.CollisionMask = 2; // Ground layer
        query.Exclude = new Godot.Collections.Array<Rid> { wheel.GetRid() };
        
        var result = spaceState.IntersectRay(query);
        return result.Count > 0;
    }

    protected virtual void UpdateFlipDetection(float dt)
    {
        float angleDeg = Mathf.RadToDeg(Mathf.Abs(Rotation % (Mathf.Pi * 2)));
        // Normalize to 0-180
        if (angleDeg > 180f) angleDeg = 360f - angleDeg;

        if (angleDeg > FlipThresholdDegrees)
        {
            _timeUpsideDown += dt;
            if (_timeUpsideDown >= FLIP_TIME_THRESHOLD && !_isFlipped)
            {
                OnVehicleFlipped();
            }
        }
        else
        {
            _timeUpsideDown = 0f;
        }
    }

    protected virtual void OnVehicleFlipped()
    {
        _isFlipped = true;
        _hasCrashed = true;
        
        // Emit signal for checkpoint system
        CheckpointSystem.Instance?.RequestRespawn(GlobalPosition, Rotation);
        
        // Visual feedback
        if (_animationPlayer != null)
        {
            _animationPlayer.Play("crash");
        }
        
        // Particles
        _dustParticles.Emitting = true;
        _dustParticles.Explosiveness = 1f;
        _dustParticles.SpeedScale = 200f;
    }

    protected virtual void UpdateVisuals(float dt)
    {
        // Update wheel sprite positions and rotations
        if (_frontWheel != null && _frontWheelSprite != null)
        {
            _frontWheelSprite.GlobalPosition = _frontWheel.GlobalPosition;
            _frontWheelSprite.Rotation = _frontWheel.Rotation;
        }
        if (_rearWheel != null && _rearWheelSprite != null)
        {
            _rearWheelSprite.GlobalPosition = _rearWheel.GlobalPosition;
            _rearWheelSprite.Rotation = _rearWheel.Rotation;
        }

        // Position particles
        if (_rearWheel != null)
        {
            _exhaustParticles.GlobalPosition = _rearWheel.GlobalPosition + Vector2.Left * 0.5f;
        }

        // Dust when grounded and moving
        if (_isGrounded && Mathf.Abs(LinearVelocity.X) > 50f)
        {
            _dustParticles.GlobalPosition = GlobalPosition + Vector2.Down * 0.5f;
            _dustParticles.Emitting = true;
        }
        else
        {
            _dustParticles.Emitting = false;
        }
    }

    protected virtual void UpdateAudio(float dt)
    {
        if (AudioManager.Instance != null && _tunedStats != null)
        {
            AudioManager.Instance.PlayEngineSound(
                "res://assets/audio/engine_loop.ogg",
                _currentRpm,
                _tunedStats.MaxRpm
            );
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is RigidBody2D rb)
        {
            // Check for hard impact
            float relativeVelocity = (LinearVelocity - rb.LinearVelocity).Length();
            float impulse = relativeVelocity * Mathf.Min(Mass, rb.Mass);
            
            if (impulse > CrashImpulseThreshold && !_hasCrashed)
            {
                OnVehicleFlipped();
            }
        }
    }

    private void OnBodyExited(Node2D body) { }

    /// <summary>
    /// Called by CheckpointSystem to respawn vehicle at checkpoint.
    /// </summary>
    public void RespawnAt(Vector2 position, float rotation)
    {
        GlobalPosition = position;
        Rotation = rotation;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0f;
        
        if (_frontWheel != null)
        {
            _frontWheel.GlobalPosition = position + Transform.X * (_tunedStats?.Wheelbase ?? 2.5f) / 2f;
            _frontWheel.LinearVelocity = Vector2.Zero;
            _frontWheel.AngularVelocity = 0f;
        }
        if (_rearWheel != null)
        {
            _rearWheel.GlobalPosition = position - Transform.X * (_tunedStats?.Wheelbase ?? 2.5f) / 2f;
            _rearWheel.LinearVelocity = Vector2.Zero;
            _rearWheel.AngularVelocity = 0f;
        }

        _isFlipped = false;
        _hasCrashed = false;
        _timeUpsideDown = 0f;
        _currentRpm = 800f;
    }

    public float GetSpeedKmh() => LinearVelocity.Length() * 3.6f;
    public float GetRpm() => _currentRpm;
    public bool IsFlipped() => _isFlipped;
    public bool HasCrashed() => _hasCrashed;
    public int GetGroundedWheels() => _groundedWheels;
}

public enum GearState
{
    Neutral,
    Drive,
    Reverse
}