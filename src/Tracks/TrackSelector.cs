using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HillClimbRacing.Tracks;

/// <summary>
/// Manages all 30 tracks, handles selection, unlocking, and filtering by difficulty.
/// </summary>
[GlobalClass]
public partial class TrackSelector : Node
{
    public static TrackSelector Instance { get; private set; }

    private readonly Dictionary<string, TrackData> _tracks = new();
    private readonly List<TrackData> _trackList = new();

    [Signal] public delegate void OnTrackListUpdatedEventHandler();

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;

        InitializeAllTracks();
    }

    private void InitializeAllTracks()
    {
        _tracks.Clear();
        _trackList.Clear();

        // ===== EASY (Green) - 6 Tracks =====
        AddTrack(new TrackData
        {
            TrackId = "countryside",
            Name = "Countryside",
            Description = "Gentle rolling hills through peaceful farmland. Perfect for beginners.",
            Difficulty = TrackDifficulty.Easy,
            DifficultyOrder = 0,
            Length = 1800f,
            EstimatedCheckpoints = 14,
            EstimatedTimeMinutes = 3,
            Seed = 1001,
            Amplitude = 30f,
            Theme = TerrainTheme.Grass,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.5f, 0.7f, 1f),
            GroundColor = new Color(0.3f, 0.5f, 0.2f),
            JumpFrequency = 0.05f,
            ObstacleDensity = 0.02f,
            UnlockCoins = 0,
            IsUnlockedByDefault = true,
            BackgroundPath = "res://assets/tracks/bg_countryside.png",
            MusicTrack = "res://assets/audio/music_countryside.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "farmlands",
            Name = "Farmlands",
            Description = "Wide open fields with gentle slopes and occasional barn jumps.",
            Difficulty = TrackDifficulty.Easy,
            DifficultyOrder = 1,
            Length = 2000f,
            EstimatedCheckpoints = 15,
            EstimatedTimeMinutes = 4,
            Seed = 1002,
            Amplitude = 25f,
            Theme = TerrainTheme.Grass,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.6f, 0.8f, 1f),
            GroundColor = new Color(0.4f, 0.6f, 0.3f),
            JumpFrequency = 0.08f,
            ObstacleDensity = 0.03f,
            UnlockCoins = 500,
            RequiredDifficultyCompletion = TrackDifficulty.Easy,
            BackgroundPath = "res://assets/tracks/bg_farmlands.png",
            MusicTrack = "res://assets/audio/music_farmlands.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "suburbs",
            Name = "Suburbs",
            Description = "Quiet residential streets with driveways and speed bumps.",
            Difficulty = TrackDifficulty.Easy,
            DifficultyOrder = 2,
            Length = 1600f,
            EstimatedCheckpoints = 12,
            EstimatedTimeMinutes = 3,
            Seed = 1003,
            Amplitude = 20f,
            Theme = TerrainTheme.Dirt,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.7f, 0.85f, 1f),
            GroundColor = new Color(0.5f, 0.5f, 0.4f),
            JumpFrequency = 0.06f,
            ObstacleDensity = 0.04f,
            UnlockCoins = 1000,
            RequiredDifficultyCompletion = TrackDifficulty.Easy,
            BackgroundPath = "res://assets/tracks/bg_suburbs.png",
            MusicTrack = "res://assets/audio/music_suburbs.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "vineyard",
            Name = "Vineyard",
            Description = "Rolling hills covered in grape vines. Smooth curves and long straights.",
            Difficulty = TrackDifficulty.Easy,
            DifficultyOrder = 3,
            Length = 2200f,
            EstimatedCheckpoints = 16,
            EstimatedTimeMinutes = 4,
            Seed = 1004,
            Amplitude = 35f,
            Theme = TerrainTheme.Grass,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.4f, 0.6f, 0.9f),
            GroundColor = new Color(0.2f, 0.4f, 0.1f),
            JumpFrequency = 0.04f,
            ObstacleDensity = 0.02f,
            UnlockCoins = 2000,
            RequiredDifficultyCompletion = TrackDifficulty.Easy,
            BackgroundPath = "res://assets/tracks/bg_vineyard.png",
            MusicTrack = "res://assets/audio/music_vineyard.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "coastal",
            Name = "Coastal Highway",
            Description = "Scenic ocean-side road with gentle climbs and sea views.",
            Difficulty = TrackDifficulty.Easy,
            DifficultyOrder = 4,
            Length = 2500f,
            EstimatedCheckpoints = 18,
            EstimatedTimeMinutes = 5,
            Seed = 1005,
            Amplitude = 40f,
            Theme = TerrainTheme.Sand,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.4f, 0.7f, 1f),
            GroundColor = new Color(0.8f, 0.7f, 0.5f),
            JumpFrequency = 0.05f,
            ObstacleDensity = 0.03f,
            UnlockCoins = 3000,
            RequiredDifficultyCompletion = TrackDifficulty.Easy,
            BackgroundPath = "res://assets/tracks/bg_coastal.png",
            MusicTrack = "res://assets/audio/music_coastal.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "park",
            Name = "National Park",
            Description = "Winding paths through protected wilderness. Beautiful but tricky.",
            Difficulty = TrackDifficulty.Easy,
            DifficultyOrder = 5,
            Length = 1900f,
            EstimatedCheckpoints = 14,
            EstimatedTimeMinutes = 4,
            Seed = 1006,
            Amplitude = 30f,
            Theme = TerrainTheme.Dirt,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.5f, 0.8f, 0.9f),
            GroundColor = new Color(0.3f, 0.4f, 0.3f),
            JumpFrequency = 0.07f,
            ObstacleDensity = 0.05f,
            UnlockCoins = 5000,
            RequiredDifficultyCompletion = TrackDifficulty.Easy,
            BackgroundPath = "res://assets/tracks/bg_park.png",
            MusicTrack = "res://assets/audio/music_park.ogg"
        });

        // ===== MEDIUM (Yellow) - 6 Tracks =====
        AddTrack(new TrackData
        {
            TrackId = "desert",
            Name = "Desert",
            Description = "Hot sandy dunes with steep climbs and sudden drops. Watch your temperature!",
            Difficulty = TrackDifficulty.Medium,
            DifficultyOrder = 0,
            Length = 2500f,
            EstimatedCheckpoints = 16,
            EstimatedTimeMinutes = 5,
            Seed = 2001,
            Amplitude = 60f,
            Theme = TerrainTheme.Sand,
            GravityMultiplier = 1f,
            SkyColor = new Color(1f, 0.9f, 0.6f),
            GroundColor = new Color(0.9f, 0.8f, 0.5f),
            JumpFrequency = 0.15f,
            ObstacleDensity = 0.1f,
            UnlockCoins = 8000,
            RequiredDifficultyCompletion = TrackDifficulty.Easy,
            BackgroundPath = "res://assets/tracks/bg_desert.png",
            MusicTrack = "res://assets/audio/music_desert.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "forest",
            Name = "Deep Forest",
            Description = "Dense woods with tree roots, logs, and muddy sections. Low visibility.",
            Difficulty = TrackDifficulty.Medium,
            DifficultyOrder = 1,
            Length = 2800f,
            EstimatedCheckpoints = 18,
            EstimatedTimeMinutes = 6,
            Seed = 2002,
            Amplitude = 50f,
            Theme = TerrainTheme.Dirt,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.3f, 0.5f, 0.3f),
            GroundColor = new Color(0.2f, 0.3f, 0.1f),
            JumpFrequency = 0.12f,
            ObstacleDensity = 0.15f,
            UnlockCoins = 12000,
            RequiredDifficultyCompletion = TrackDifficulty.Easy,
            BackgroundPath = "res://assets/tracks/bg_forest.png",
            MusicTrack = "res://assets/audio/music_forest.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "highway",
            Name = "Abandoned Highway",
            Description = "Cracked asphalt with broken bridges and massive jumps. High speed required.",
            Difficulty = TrackDifficulty.Medium,
            DifficultyOrder = 2,
            Length = 3000f,
            EstimatedCheckpoints = 18,
            EstimatedTimeMinutes = 5,
            Seed = 2003,
            Amplitude = 45f,
            Theme = TerrainTheme.Rock,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.6f, 0.6f, 0.7f),
            GroundColor = new Color(0.3f, 0.3f, 0.35f),
            JumpFrequency = 0.2f,
            ObstacleDensity = 0.08f,
            HasBoostPads = true,
            UnlockCoins = 18000,
            RequiredDifficultyCompletion = TrackDifficulty.Medium,
            BackgroundPath = "res://assets/tracks/bg_highway.png",
            MusicTrack = "res://assets/audio/music_highway.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "canyon",
            Name = "Grand Canyon",
            Description = "Narrow canyon paths with sheer drops. One mistake and it's a long fall.",
            Difficulty = TrackDifficulty.Medium,
            DifficultyOrder = 3,
            Length = 2600f,
            EstimatedCheckpoints = 16,
            EstimatedTimeMinutes = 6,
            Seed = 2004,
            Amplitude = 80f,
            Theme = TerrainTheme.Rock,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.9f, 0.6f, 0.3f),
            GroundColor = new Color(0.6f, 0.4f, 0.2f),
            JumpFrequency = 0.1f,
            ObstacleDensity = 0.12f,
            UnlockCoins = 25000,
            RequiredDifficultyCompletion = TrackDifficulty.Medium,
            BackgroundPath = "res://assets/tracks/bg_canyon.png",
            MusicTrack = "res://assets/audio/music_canyon.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "swamp",
            Name = "Swamp",
            Description = "Murky water, floating platforms, and sinking mud. Keep moving or sink!",
            Difficulty = TrackDifficulty.Medium,
            DifficultyOrder = 4,
            Length = 2400f,
            EstimatedCheckpoints = 16,
            EstimatedTimeMinutes = 7,
            Seed = 2005,
            Amplitude = 30f,
            Theme = TerrainTheme.Jungle,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.2f, 0.4f, 0.3f),
            GroundColor = new Color(0.1f, 0.2f, 0.15f),
            JumpFrequency = 0.08f,
            ObstacleDensity = 0.2f,
            HasMovingPlatforms = true,
            UnlockCoins = 35000,
            RequiredDifficultyCompletion = TrackDifficulty.Medium,
            BackgroundPath = "res://assets/tracks/bg_swamp.png",
            MusicTrack = "res://assets/audio/music_swamp.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "dunes",
            Name = "Sand Dunes",
            Description = "Endless shifting dunes. Soft sand slows you down, but big air is easy.",
            Difficulty = TrackDifficulty.Medium,
            DifficultyOrder = 5,
            Length = 3200f,
            EstimatedCheckpoints = 20,
            EstimatedTimeMinutes = 7,
            Seed = 2006,
            Amplitude = 70f,
            Theme = TerrainTheme.Sand,
            GravityMultiplier = 1f,
            SkyColor = new Color(1f, 0.85f, 0.5f),
            GroundColor = new Color(0.95f, 0.85f, 0.6f),
            JumpFrequency = 0.18f,
            ObstacleDensity = 0.05f,
            UnlockCoins = 50000,
            RequiredDifficultyCompletion = TrackDifficulty.Medium,
            BackgroundPath = "res://assets/tracks/bg_dunes.png",
            MusicTrack = "res://assets/audio/music_dunes.ogg"
        });

        // ===== HARD (Red) - 6 Tracks =====
        AddTrack(new TrackData
        {
            TrackId = "arctic",
            Name = "Arctic",
            Description = "Frozen tundra with ice physics. Low friction, slippery slopes, cracking ice.",
            Difficulty = TrackDifficulty.Hard,
            DifficultyOrder = 0,
            Length = 3000f,
            EstimatedCheckpoints = 18,
            EstimatedTimeMinutes = 7,
            Seed = 3001,
            Amplitude = 60f,
            Theme = TerrainTheme.Ice,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.7f, 0.9f, 1f),
            GroundColor = new Color(0.8f, 0.9f, 1f),
            JumpFrequency = 0.1f,
            ObstacleDensity = 0.1f,
            UnlockCoins = 75000,
            RequiredDifficultyCompletion = TrackDifficulty.Medium,
            BackgroundPath = "res://assets/tracks/bg_arctic.png",
            MusicTrack = "res://assets/audio/music_arctic.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "mountain",
            Name = "High Mountain",
            Description = "Extreme elevation changes, hairpin turns, and falling rocks. Thin air reduces engine power.",
            Difficulty = TrackDifficulty.Hard,
            DifficultyOrder = 1,
            Length = 3500f,
            EstimatedCheckpoints = 22,
            EstimatedTimeMinutes = 8,
            Seed = 3002,
            Amplitude = 100f,
            Theme = TerrainTheme.Rock,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.5f, 0.7f, 0.9f),
            GroundColor = new Color(0.4f, 0.4f, 0.45f),
            JumpFrequency = 0.15f,
            ObstacleDensity = 0.15f,
            UnlockCoins = 100000,
            RequiredDifficultyCompletion = TrackDifficulty.Medium,
            BackgroundPath = "res://assets/tracks/bg_mountain.png",
            MusicTrack = "res://assets/audio/music_mountain.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "volcano",
            Name = "Active Volcano",
            Description = "Lava flows, volcanic rock, and sudden eruptions. Heat damages vehicle over time.",
            Difficulty = TrackDifficulty.Hard,
            DifficultyOrder = 2,
            Length = 3200f,
            EstimatedCheckpoints = 20,
            EstimatedTimeMinutes = 8,
            Seed = 3003,
            Amplitude = 80f,
            Theme = TerrainTheme.Rock,
            GravityMultiplier = 1f,
            SkyColor = new Color(1f, 0.3f, 0.1f),
            GroundColor = new Color(0.3f, 0.1f, 0.05f),
            JumpFrequency = 0.12f,
            ObstacleDensity = 0.18f,
            UnlockCoins = 150000,
            RequiredDifficultyCompletion = TrackDifficulty.Hard,
            BackgroundPath = "res://assets/tracks/bg_volcano.png",
            MusicTrack = "res://assets/audio/music_volcano.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "jungle",
            Name = "Ancient Jungle",
            Description = "Overgrown ruins with vine swings, crumbling temples, and hidden shortcuts.",
            Difficulty = TrackDifficulty.Hard,
            DifficultyOrder = 3,
            Length = 3800f,
            EstimatedCheckpoints = 24,
            EstimatedTimeMinutes = 9,
            Seed = 3004,
            Amplitude = 70f,
            Theme = TerrainTheme.Jungle,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.2f, 0.5f, 0.3f),
            GroundColor = new Color(0.15f, 0.25f, 0.15f),
            JumpFrequency = 0.1f,
            ObstacleDensity = 0.2f,
            HasMovingPlatforms = true,
            UnlockCoins = 200000,
            RequiredDifficultyCompletion = TrackDifficulty.Hard,
            BackgroundPath = "res://assets/tracks/bg_jungle.png",
            MusicTrack = "res://assets/audio/music_jungle.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "ruins",
            Name = "Forgotten Ruins",
            Description = "Ancient civilization remnants. Precision driving through collapsing architecture.",
            Difficulty = TrackDifficulty.Hard,
            DifficultyOrder = 4,
            Length = 3600f,
            EstimatedCheckpoints = 22,
            EstimatedTimeMinutes = 9,
            Seed = 3005,
            Amplitude = 65f,
            Theme = TerrainTheme.Rock,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.6f, 0.5f, 0.4f),
            GroundColor = new Color(0.4f, 0.35f, 0.3f),
            JumpFrequency = 0.12f,
            ObstacleDensity = 0.15f,
            UnlockCoins = 300000,
            RequiredDifficultyCompletion = TrackDifficulty.Hard,
            BackgroundPath = "res://assets/tracks/bg_ruins.png",
            MusicTrack = "res://assets/audio/music_ruins.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "glacier",
            Name = "Glacier",
            Description = "Massive ice sheets with deep crevasses. Ice physics + moving ice floes.",
            Difficulty = TrackDifficulty.Hard,
            DifficultyOrder = 5,
            Length = 4000f,
            EstimatedCheckpoints = 25,
            EstimatedTimeMinutes = 10,
            Seed = 3006,
            Amplitude = 90f,
            Theme = TerrainTheme.Ice,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.6f, 0.8f, 1f),
            GroundColor = new Color(0.7f, 0.85f, 1f),
            JumpFrequency = 0.08f,
            ObstacleDensity = 0.12f,
            HasMovingPlatforms = true,
            UnlockCoins = 500000,
            RequiredDifficultyCompletion = TrackDifficulty.Hard,
            BackgroundPath = "res://assets/tracks/bg_glacier.png",
            MusicTrack = "res://assets/audio/music_glacier.ogg"
        });

        // ===== EXPERT (Purple) - 6 Tracks =====
        AddTrack(new TrackData
        {
            TrackId = "moon",
            Name = "Moon Base",
            Description = "Low gravity (1/6th Earth). Huge jumps, slow falls. Requires delicate throttle control.",
            Difficulty = TrackDifficulty.Expert,
            DifficultyOrder = 0,
            Length = 3500f,
            EstimatedCheckpoints = 22,
            EstimatedTimeMinutes = 8,
            Seed = 4001,
            Amplitude = 120f,
            Theme = TerrainTheme.MoonDust,
            GravityMultiplier = 0.166f,
            SkyColor = new Color(0.1f, 0.1f, 0.15f),
            GroundColor = new Color(0.3f, 0.3f, 0.35f),
            JumpFrequency = 0.25f,
            ObstacleDensity = 0.05f,
            UnlockCoins = 750000,
            RequiredDifficultyCompletion = TrackDifficulty.Hard,
            BackgroundPath = "res://assets/tracks/bg_moon.png",
            MusicTrack = "res://assets/audio/music_moon.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "mars",
            Name = "Mars Colony",
            Description = "Red planet with 1/3rd gravity. Dust storms reduce visibility. Rocky terrain.",
            Difficulty = TrackDifficulty.Expert,
            DifficultyOrder = 1,
            Length = 4000f,
            EstimatedCheckpoints = 25,
            EstimatedTimeMinutes = 9,
            Seed = 4002,
            Amplitude = 100f,
            Theme = TerrainTheme.MarsRed,
            GravityMultiplier = 0.38f,
            SkyColor = new Color(0.8f, 0.4f, 0.2f),
            GroundColor = new Color(0.6f, 0.3f, 0.15f),
            JumpFrequency = 0.2f,
            ObstacleDensity = 0.1f,
            UnlockCoins = 1000000,
            RequiredDifficultyCompletion = TrackDifficulty.Hard,
            BackgroundPath = "res://assets/tracks/bg_mars.png",
            MusicTrack = "res://assets/audio/music_mars.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "asteroid",
            Name = "Asteroid Belt",
            Description = "Micro-gravity. Jump between asteroids. One wrong move = lost in space.",
            Difficulty = TrackDifficulty.Expert,
            DifficultyOrder = 2,
            Length = 3000f,
            EstimatedCheckpoints = 20,
            EstimatedTimeMinutes = 10,
            Seed = 4003,
            Amplitude = 200f,
            Theme = TerrainTheme.Asteroid,
            GravityMultiplier = 0.02f,
            SkyColor = new Color(0.05f, 0.05f, 0.1f),
            GroundColor = new Color(0.2f, 0.2f, 0.25f),
            JumpFrequency = 0.3f,
            ObstacleDensity = 0.15f,
            UnlockCoins = 1500000,
            RequiredDifficultyCompletion = TrackDifficulty.Expert,
            BackgroundPath = "res://assets/tracks/bg_asteroid.png",
            MusicTrack = "res://assets/audio/music_asteroid.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "europa",
            Name = "Europa Ice",
            Description = "Jupiter's moon. Subsurface ocean geysers launch you. Extreme low gravity.",
            Difficulty = TrackDifficulty.Expert,
            DifficultyOrder = 3,
            Length = 4200f,
            EstimatedCheckpoints = 28,
            EstimatedTimeMinutes = 11,
            Seed = 4004,
            Amplitude = 150f,
            Theme = TerrainTheme.Ice,
            GravityMultiplier = 0.13f,
            SkyColor = new Color(0.2f, 0.3f, 0.5f),
            GroundColor = new Color(0.8f, 0.9f, 1f),
            JumpFrequency = 0.2f,
            ObstacleDensity = 0.1f,
            HasBoostPads = true,
            UnlockCoins = 2000000,
            RequiredDifficultyCompletion = TrackDifficulty.Expert,
            BackgroundPath = "res://assets/tracks/bg_europa.png",
            MusicTrack = "res://assets/audio/music_europa.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "titan",
            Name = "Titan Lakes",
            Description = "Saturn's moon. Methane lakes, thick atmosphere. Buoyancy affects handling.",
            Difficulty = TrackDifficulty.Expert,
            DifficultyOrder = 4,
            Length = 4500f,
            EstimatedCheckpoints = 30,
            EstimatedTimeMinutes = 12,
            Seed = 4005,
            Amplitude = 80f,
            Theme = TerrainTheme.MoonDust,
            GravityMultiplier = 0.14f,
            SkyColor = new Color(0.4f, 0.3f, 0.2f),
            GroundColor = new Color(0.2f, 0.2f, 0.15f),
            JumpFrequency = 0.15f,
            ObstacleDensity = 0.12f,
            HasMovingPlatforms = true,
            UnlockCoins = 3000000,
            RequiredDifficultyCompletion = TrackDifficulty.Expert,
            BackgroundPath = "res://assets/tracks/bg_titan.png",
            MusicTrack = "res://assets/audio/music_titan.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "venus",
            Name = "Venus Surface",
            Description = "Crushing pressure, acid rain, 900°F heat. Heavy gravity. Vehicle takes constant damage.",
            Difficulty = TrackDifficulty.Expert,
            DifficultyOrder = 5,
            Length = 3800f,
            EstimatedCheckpoints = 26,
            EstimatedTimeMinutes = 10,
            Seed = 4006,
            Amplitude = 90f,
            Theme = TerrainTheme.Rock,
            GravityMultiplier = 0.9f,
            SkyColor = new Color(0.9f, 0.5f, 0.1f),
            GroundColor = new Color(0.4f, 0.2f, 0.1f),
            JumpFrequency = 0.1f,
            ObstacleDensity = 0.2f,
            UnlockCoins = 5000000,
            RequiredDifficultyCompletion = TrackDifficulty.Expert,
            BackgroundPath = "res://assets/tracks/bg_venus.png",
            MusicTrack = "res://assets/audio/music_venus.ogg"
        });

        // ===== INSANE (Black) - 6 Tracks =====
        AddTrack(new TrackData
        {
            TrackId = "hell",
            Name = "Hell",
            Description = "The underworld. Fire, brimstone, inverted gravity zones. Physics breaks down.",
            Difficulty = TrackDifficulty.Insane,
            DifficultyOrder = 0,
            Length = 4000f,
            EstimatedCheckpoints = 28,
            EstimatedTimeMinutes = 12,
            Seed = 5001,
            Amplitude = 200f,
            Theme = TerrainTheme.Void,
            GravityMultiplier = 1f,
            SkyColor = new Color(0.5f, 0.05f, 0.05f),
            GroundColor = new Color(0.3f, 0.05f, 0.05f),
            JumpFrequency = 0.3f,
            ObstacleDensity = 0.25f,
            HasLoops = true,
            UnlockCoins = 10000000,
            RequiredDifficultyCompletion = TrackDifficulty.Expert,
            BackgroundPath = "res://assets/tracks/bg_hell.png",
            MusicTrack = "res://assets/audio/music_hell.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "void",
            Name = "The Void",
            Description = "Empty space with floating platforms. No ground reference. Pure precision.",
            Difficulty = TrackDifficulty.Insane,
            DifficultyOrder = 1,
            Length = 5000f,
            EstimatedCheckpoints = 30,
            EstimatedTimeMinutes = 15,
            Seed = 5002,
            Amplitude = 300f,
            Theme = TerrainTheme.Void,
            GravityMultiplier = 0.5f,
            SkyColor = new Color(0.02f, 0.02f, 0.05f),
            GroundColor = new Color(0.1f, 0.1f, 0.15f),
            JumpFrequency = 0.4f,
            ObstacleDensity = 0.3f,
            UnlockCoins = 20000000,
            RequiredDifficultyCompletion = TrackDifficulty.Insane,
            BackgroundPath = "res://assets/tracks/bg_void.png",
            MusicTrack = "res://assets/audio/music_void.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "neutron_star",
            Name = "Neutron Star",
            Description = "Extreme gravity (1000x Earth). Vehicle crushed if not perfectly balanced. Time dilation.",
            Difficulty = TrackDifficulty.Insane,
            DifficultyOrder = 2,
            Length = 2000f,
            EstimatedCheckpoints = 20,
            EstimatedTimeMinutes = 10,
            Seed = 5003,
            Amplitude = 50f,
            Theme = TerrainTheme.Void,
            GravityMultiplier = 100f,
            SkyColor = new Color(0.8f, 0.8f, 1f),
            GroundColor = new Color(0.2f, 0.2f, 0.3f),
            JumpFrequency = 0.01f,
            ObstacleDensity = 0.05f,
            UnlockCoins = 50000000,
            RequiredDifficultyCompletion = TrackDifficulty.Insane,
            BackgroundPath = "res://assets/tracks/bg_neutron.png",
            MusicTrack = "res://assets/audio/music_neutron.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "event_horizon",
            Name = "Event Horizon",
            Description = "Black hole edge. Gravity increases as you progress. Light bends. Space warps.",
            Difficulty = TrackDifficulty.Insane,
            DifficultyOrder = 3,
            Length = 5000f,
            EstimatedCheckpoints = 32,
            EstimatedTimeMinutes = 20,
            Seed = 5004,
            Amplitude = 400f,
            Theme = TerrainTheme.Void,
            GravityMultiplier = 10f, // Increases dynamically
            SkyColor = new Color(0f, 0f, 0f),
            GroundColor = new Color(0.05f, 0.02f, 0.1f),
            JumpFrequency = 0.2f,
            ObstacleDensity = 0.2f,
            HasLoops = true,
            UnlockCoins = 100000000,
            RequiredDifficultyCompletion = TrackDifficulty.Insane,
            BackgroundPath = "res://assets/tracks/bg_event_horizon.png",
            MusicTrack = "res://assets/audio/music_event_horizon.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "quantum",
            Name = "Quantum Realm",
            Description = "Probability-based terrain. Track exists in superposition. Observe to collapse.",
            Difficulty = TrackDifficulty.Insane,
            DifficultyOrder = 4,
            Length = 4500f,
            EstimatedCheckpoints = 30,
            EstimatedTimeMinutes = 18,
            Seed = 5005,
            Amplitude = 250f,
            Theme = TerrainTheme.Neon,
            GravityMultiplier = 1f, // Fluctuates
            SkyColor = new Color(0.3f, 0.1f, 0.5f),
            GroundColor = new Color(0.1f, 0.05f, 0.2f),
            JumpFrequency = 0.25f,
            ObstacleDensity = 0.3f,
            UnlockCoins = 200000000,
            RequiredDifficultyCompletion = TrackDifficulty.Insane,
            BackgroundPath = "res://assets/tracks/bg_quantum.png",
            MusicTrack = "res://assets/audio/music_quantum.ogg"
        });

        AddTrack(new TrackData
        {
            TrackId = "paradox",
            Name = "Paradox",
            Description = "The final track. Non-Euclidean geometry. Past and future overlap. Can you finish?",
            Difficulty = TrackDifficulty.Insane,
            DifficultyOrder = 5,
            Length = 6000f,
            EstimatedCheckpoints = 35,
            EstimatedTimeMinutes = 25,
            Seed = 5006,
            Amplitude = 500f,
            Theme = TerrainTheme.Neon,
            GravityMultiplier = 1f, // Varies by section
            SkyColor = new Color(0.2f, 0.2f, 0.4f),
            GroundColor = new Color(0.05f, 0.05f, 0.15f),
            JumpFrequency = 0.3f,
            ObstacleDensity = 0.4f,
            HasLoops = true,
            HasMovingPlatforms = true,
            HasBoostPads = true,
            UnlockCoins = 500000000,
            RequiredDifficultyCompletion = TrackDifficulty.Insane,
            BackgroundPath = "res://assets/tracks/bg_paradox.png",
            MusicTrack = "res://assets/audio/music_paradox.ogg"
        });

        GD.Print($"Initialized {_trackList.Count} tracks across 5 difficulties");
    }

    private void AddTrack(TrackData track)
    {
        _tracks[track.TrackId] = track;
        _trackList.Add(track);
    }

    public TrackData GetTrackById(string trackId)
    {
        _tracks.TryGetValue(trackId, out var track);
        return track;
    }

    public List<TrackData> GetAllTracks() => new(_trackList);

    public List<TrackData> GetTracksByDifficulty(TrackDifficulty difficulty)
    {
        return _trackList.Where(t => t.Difficulty == difficulty)
                         .OrderBy(t => t.DifficultyOrder)
                         .ToList();
    }

    public List<TrackData> GetUnlockedTracks(HashSet<string> unlockedTrackIds)
    {
        return _trackList.Where(t => t.IsUnlockedByDefault || unlockedTrackIds.Contains(t.TrackId))
                         .OrderBy(t => (int)t.Difficulty)
                         .ThenBy(t => t.DifficultyOrder)
                         .ToList();
    }

    public TrackData GetNextLockedTrack(TrackDifficulty completedDifficulty)
    {
        return _trackList.FirstOrDefault(t => 
            t.RequiredDifficultyCompletion == completedDifficulty && 
            !t.IsUnlockedByDefault);
    }

    public int GetTotalTracks() => _trackList.Count;
    public int GetTrackCountByDifficulty(TrackDifficulty difficulty) => 
        _trackList.Count(t => t.Difficulty == difficulty);
}