using Godot;

namespace HillClimbRacing.Vehicles;

/// <summary>
/// Jeep - Starter vehicle. Balanced stats, RWD.
/// Good all-rounder for learning the game.
/// </summary>
[GlobalClass]
public partial class Jeep : VehicleBase
{
    public Jeep()
    {
        Name = "Jeep";
    }

    public override void _Ready()
    {
        base._Ready();
        
        // Jeep-specific setup
        if (BaseStats != null)
        {
            Initialize(BaseStats.GetTunedStats(new VehicleTuning()), new VehicleTuning());
        }
    }

    protected override void InitializeVisuals()
    {
        base.InitializeVisuals();
        
        // Jeep body - simple colored rectangle for now
        if (_bodySprite != null && _bodySprite.Texture == null)
        {
            _bodySprite.Modulate = new Color(0.2f, 0.5f, 0.2f); // Green
            // Create a simple procedural texture or use placeholder
            _bodySprite.Texture = CreatePlaceholderTexture(new Vector2(120, 60), new Color(0.2f, 0.5f, 0.2f));
        }
        
        // Wheel sprites
        if (_frontWheelSprite != null && _frontWheelSprite.Texture == null)
        {
            _frontWheelSprite.Texture = CreateWheelTexture(0.35f, new Color(0.1f, 0.1f, 0.1f));
        }
        if (_rearWheelSprite != null && _rearWheelSprite.Texture == null)
        {
            _rearWheelSprite.Texture = CreateWheelTexture(0.35f, new Color(0.1f, 0.1f, 0.1f));
        }
    }

    private Texture2D CreatePlaceholderTexture(Vector2 size, Color color)
    {
        var image = Image.Create((int)size.X, (int)size.Y, false, Image.Format.Rgba8);
        image.Fill(color);
        var texture = ImageTexture.CreateFromImage(image);
        return texture;
    }

    private Texture2D CreateWheelTexture(float radiusMeters, Color color)
    {
        int size = (int)(radiusMeters * 100 * 2); // Convert to pixels (1m = 100px)
        var image = Image.Create(size, size, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);
        
        // Draw circle
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = new Vector2(x, y).DistanceTo(center);
                if (dist <= radius && dist >= radius - 8) // Tire thickness
                {
                    image.SetPixel(x, y, color);
                }
                else if (dist < radius - 8) // Rim
                {
                    image.SetPixel(x, y, new Color(0.3f, 0.3f, 0.35f));
                }
            }
        }
        
        return ImageTexture.CreateFromImage(image);
    }
}