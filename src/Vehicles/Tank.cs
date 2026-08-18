using Godot;

namespace HillClimbRacing.Vehicles;

/// <summary>
/// Tank - Tracks instead of wheels, nearly indestructible.
/// Unlockable ultimate vehicle.
/// </summary>
[GlobalClass]
public partial class Tank : VehicleBase
{
    private Sprite2D _trackLeftSprite;
    private Sprite2D _trackRightSprite;
    private float _trackOffset = 0f;

    public Tank()
    {
        Name = "Tank";
    }

    public override void _Ready()
    {
        base._Ready();
        
        // Tank has tracks, not wheels - override physics setup
        if (BaseStats != null)
        {
            Initialize(BaseStats.GetTunedStats(new VehicleTuning()), new VehicleTuning());
        }
        
        // Tank-specific: higher mass, lower CoG
        Mass *= 2.5f;
        CenterOfMassOffset = new Vector2(0, -CenterOfMassOffset.Y * 1.5f);
        AngularDamp = 0.5f; // More stable
    }

    protected override void InitializeVisuals()
    {
        base.InitializeVisuals();
        
        // Hide wheel sprites - we use tracks
        if (_frontWheelSprite != null) _frontWheelSprite.Visible = false;
        if (_rearWheelSprite != null) _rearWheelSprite.Visible = false;
        
        if (_bodySprite != null && _bodySprite.Texture == null)
        {
            _bodySprite.Modulate = new Color(0.2f, 0.3f, 0.15f); // Olive drab
            _bodySprite.Texture = CreatePlaceholderTexture(new Vector2(140, 65), new Color(0.2f, 0.3f, 0.15f));
        }

        // Track visuals (animated)
        _trackLeftSprite = new Sprite2D
        {
            Texture = CreateTrackTexture(),
            Position = new Vector2(0, 30),
            ZIndex = 2
        };
        AddChild(_trackLeftSprite);

        _trackRightSprite = new Sprite2D
        {
            Texture = CreateTrackTexture(),
            Position = new Vector2(0, -30),
            ZIndex = 2
        };
        AddChild(_trackRightSprite);
    }

    protected override void ApplyWheelForces(float dt)
    {
        if (_tunedStats == null || !_isGrounded) return;

        // Tank uses track drive - apply force directly to body
        float throttleFactor = Mathf.Abs(_throttleInput);
        float targetForce = _tunedStats.EngineTorque * throttleFactor * 10f; // Tracks have huge torque
        
        if (_boostActive)
        {
            targetForce *= 2f;
        }

        // Apply force in forward direction
        Vector2 forward = Transform.X;
        ApplyCentralForce(forward * targetForce * _throttleInput * dt * 60f);

        // Braking
        if (_brakeInput > 0.1f)
        {
            Vector2 brakeForce = -LinearVelocity * _brakeInput * 500f * dt;
            ApplyCentralForce(brakeForce);
        }

        // Turning - tanks turn by differential track speed
        // Simplified: apply torque for turning
        float turnTorque = _throttleInput * _tunedStats.EngineTorque * 5f;
        ApplyTorqueImpulse(turnTorque * dt);

        // Update track animation offset
        _trackOffset += LinearVelocity.Length() * dt * 0.5f;
        if (_trackLeftSprite != null)
        {
            _trackLeftSprite.Texture = CreateTrackTexture(_trackOffset);
            _trackRightSprite.Texture = CreateTrackTexture(-_trackOffset);
        }
    }

    protected override void UpdateGroundedState()
    {
        // Tank is almost always grounded due to tracks
        _isGrounded = true;
        _groundedWheels = 2;
    }

    protected override void UpdateFlipDetection(float dt)
    {
        // Tank is very stable - higher flip threshold
        float angleDeg = Mathf.RadToDeg(Mathf.Abs(Rotation % (Mathf.Pi * 2)));
        if (angleDeg > 180f) angleDeg = 360f - angleDeg;

        if (angleDeg > FlipThresholdDegrees * 1.5f) // 50% more stable
        {
            _timeUpsideDown += dt;
            if (_timeUpsideDown >= FLIP_TIME_THRESHOLD * 2f && !_isFlipped)
            {
                OnVehicleFlipped();
            }
        }
        else
        {
            _timeUpsideDown = 0f;
        }
    }

    protected override void UpdateVisuals(float dt)
    {
        // No wheel sprites to update
        if (_exhaustParticles != null)
        {
            _exhaustParticles.GlobalPosition = GlobalPosition + Transform.X * -60f + Vector2.Up * 20f;
        }
    }

    private Texture2D CreatePlaceholderTexture(Vector2 size, Color color)
    {
        var image = Image.Create((int)size.X, (int)size.Y, false, Image.Format.Rgba8);
        image.Fill(color);
        return ImageTexture.CreateFromImage(image);
    }

    private Texture2D CreateTrackTexture(float offset = 0f)
    {
        int width = 140;
        int height = 30;
        var image = Image.Create(width, height, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);
        
        Color trackColor = new Color(0.1f, 0.1f, 0.1f);
        Color treadColor = new Color(0.05f, 0.05f, 0.05f);
        
        int segmentWidth = 20;
        int numSegments = width / segmentWidth + 2;
        
        for (int s = -1; s < numSegments; s++)
        {
            float segmentX = s * segmentWidth + (offset % segmentWidth);
            if (segmentX < -segmentWidth || segmentX > width) continue;
            
            int ix = (int)segmentX;
            for (int x = ix; x < ix + segmentWidth - 2 && x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Tread pattern
                    if ((x + y) % 4 < 2)
                        image.SetPixel(x, y, treadColor);
                    else
                        image.SetPixel(x, y, trackColor);
                }
            }
        }
        
        return ImageTexture.CreateFromImage(image);
    }
}