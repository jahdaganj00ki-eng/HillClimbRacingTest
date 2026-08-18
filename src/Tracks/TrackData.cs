using System;
using System.Collections.Generic;
using Godot;

namespace HillClimbRacing.Tracks;

/// <summary>
/// Track difficulty tiers
/// </summary>
public enum TrackDifficulty
{
    Easy,       // Green - Gentle, wide tracks
    Medium,     // Yellow - Moderate hills, jumps
    Hard,       // Red - Steep, technical terrain
    Expert,     // Purple - Low gravity, alien environments
    Insane      // Black - Physics-defying, extreme
}

/// <summary>
/// Track metadata and procedural generation parameters.
/// </summary>
[GlobalClass]
public partial class TrackData : Resource
{
    public string TrackId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TrackDifficulty Difficulty { get; set; }
    public int DifficultyOrder { get; set; } // For sorting within difficulty

    // Track dimensions
    public float Length { get; set; } // meters
    public int EstimatedCheckpoints { get; set; }
    public float EstimatedTimeMinutes { get; set; }

    // Procedural generation parameters
    public int Seed { get; set; }
    public float BaseFrequency { get; set; } = 0.01f;
    public float Amplitude { get; set; } = 50f;
    public int Octaves { get; set; } = 4;
    public float Persistence { get; set; } = 0.5f;
    public float Lacunarity { get; set; } = 2f;

    // Terrain theme
    public TerrainTheme Theme { get; set; } = TerrainTheme.Grass;
    public float GravityMultiplier { get; set; } = 1f;
    public Color SkyColor { get; set; } = new Color(0.5f, 0.7f, 1f);
    public Color GroundColor { get; set; } = new Color(0.3f, 0.5f, 0.2f);
    public Color AccentColor { get; set; } = new Color(0.8f, 0.6f, 0.2f);

    // Obstacles and features
    public float JumpFrequency { get; set; } = 0.1f;
    public float ObstacleDensity { get; set; } = 0.05f;
    public bool HasLoops { get; set; } = false;
    public bool HasMovingPlatforms { get; set; } = false;
    public bool HasBoostPads { get; set; } = false;

    // Unlock requirements
    public int UnlockCoins { get; set; } = 0;
    public TrackDifficulty RequiredDifficultyCompletion { get; set; } = TrackDifficulty.Easy;
    public bool IsUnlockedByDefault { get; set; } = false;

    // Visual assets
    public string BackgroundPath { get; set; }
    public string TilesetPath { get; set; }
    public string MusicTrack { get; set; }

    /// <summary>
    /// Generates the track path (centerline) using procedural generation.
    /// Returns list of Vector2 points representing the track centerline.
    /// </summary>
    public List<Vector2> GetTrackPath()
    {
        var path = new List<Vector2>();
        var noise = new FastNoiseLite
        {
            Seed = Seed,
            Frequency = BaseFrequency,
            FractalType = FastNoiseLite.FractalTypeEnum.Fbm,
            FractalOctaves = Octaves,
            FractalPersistence = Persistence,
            FractalLacunarity = Lacunarity
        };

        // Generate track as a series of points
        int numPoints = (int)(Length / 10f); // Point every 10 meters
        float x = 0f;
        float y = 0f;
        
        path.Add(new Vector2(x, y));

        for (int i = 1; i < numPoints; i++)
        {
            x += 10f; // Step forward 10 meters
            
            // Add terrain variation using noise
            float noiseValue = noise.GetNoise2D(x, 0);
            y += noiseValue * Amplitude * 0.1f; // Scale down for gradual changes
            
            // Add jumps at intervals
            if (JumpFrequency > 0 && i % (int)(1f / JumpFrequency) == 0)
            {
                y += 20f * (float)GD.RandRange(0.5, 1.5); // Jump height
            }
            
            path.Add(new Vector2(x, y));
        }

        return path;
    }

    /// <summary>
    /// Gets the terrain height at a given X position.
    /// </summary>
    public float GetTerrainHeight(float x)
    {
        var noise = new FastNoiseLite
        {
            Seed = Seed,
            Frequency = BaseFrequency,
            FractalType = FastNoiseLite.FractalTypeEnum.Fbm,
            FractalOctaves = Octaves,
            FractalPersistence = Persistence,
            FractalLacunarity = Lacunarity
        };
        
        return noise.GetNoise2D(x, 0) * Amplitude;
    }

    /// <summary>
    /// Gets the track width at a given position (for varying width tracks).
    /// </summary>
    public float GetTrackWidth(float distanceAlongTrack)
    {
        // Base width with some variation
        float baseWidth = 20f; // meters
        var noise = new FastNoiseLite { Seed = Seed + 1000, Frequency = 0.005f };
        float variation = noise.GetNoise2D(distanceAlongTrack, 0) * 5f;
        return Math.Max(10f, baseWidth + variation);
    }
}

public enum TerrainTheme
{
    Grass,      // Green countryside
    Sand,       // Desert
    Dirt,       // Forest/mountain
    Snow,       // Arctic
    Rock,       // Mountain/volcano
    Jungle,     // Tropical
    Ice,        // Glacier
    MoonDust,   // Moon
    MarsRed,    // Mars
    Asteroid,   // Asteroid
    Void,       // Hell/Void
    Neon        // Quantum/Paradox
}