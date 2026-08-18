using Godot;

namespace HillClimbRacing.Vehicles;

/// <summary>
/// Moon Buggy - Low gravity specialist, 4WD.
/// Designed for Moon, Mars, Asteroid tracks.
/// </summary>
[GlobalClass]
public partial class MoonBuggy : VehicleBase
{
    public MoonBuggy()
    {
        Name = "Moon Buggy";
    }

    public override void _Ready()
    {
        base._Ready();
        
        if (BaseStats != null)
        {
            Initialize(BaseStats.GetTunedStats(new VehicleTuning()), new VehicleTuning());
        }
    }

    protected override void InitializeVisuals()
    {
        base.InitializeVisuals();
        
        if (_bodySprite != null && _bodySprite.Texture == null)
        {
            _bodySprite.Modulate = new Color(0.7f, 0.7f, 0.8f); // Light gray/white
            _bodySprite.Texture = CreatePlaceholderTexture(new Vector2(100, 55), new Color(0.7f, 0.7f, 0.8f));
        }
        
        if (_frontWheelSprite != null && _frontWheelSprite.Texture == null)
        {
            _frontWheelSprite.Texture = CreateWheelTexture(0.4f, new Color(0.3f, 0.3f, 0.35f));
        }
        if (_rearWheelSprite != null && _rearWheelSprite.Texture == null)
        {
            _rearWheelSprite.Texture = CreateWheelTexture(0.4f, new Color(0.3f, 0.3f, 0.35f));
        }
    }

    private Texture2D CreatePlaceholderTexture(Vector2 size, Color color)
    {
        var image = Image.Create((int)size.X, (int)size.Y, false, Image.Format.Rgba8);
        image.Fill(color);
        return ImageTexture.CreateFromImage(image);
    }

    private Texture2D CreateWheelTexture(float radiusMeters, Color color)
    {
        int size = (int)(radiusMeters * 100 * 2);
        var image = Image.Create(size, size, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = new Vector2(x, y).DistanceTo(center);
                if (dist <= radius && dist >= radius - 8)
                {
                    image.SetPixel(x, y, color);
                }
                else if (dist < radius - 8)
                {
                    image.SetPixel(x, y, new Color(0.5f, 0.5f, 0.55f));
                }
            }
        }
        
        return ImageTexture.CreateFromImage(image);
    }
}