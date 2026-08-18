using System;
using System.Collections.Generic;
using Godot;

namespace HillClimbRacing.Systems;

/// <summary>
/// Manages all audio: music, sound effects, engine sounds.
/// </summary>
[GlobalClass]
public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    [Export] public float MasterVolume { get; set; } = 1f;
    [Export] public float MusicVolume { get; set; } = 0.7f;
    [Export] public float SfxVolume { get; set; } = 1f;
    [Export] public float EngineVolume { get; set; } = 0.8f;

    private AudioStreamPlayer _musicPlayer;
    private AudioStreamPlayer _enginePlayer;
    private readonly Dictionary<string, AudioStreamPlayer> _sfxPlayers = new();
    private string _currentMusic = "";

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;

        _musicPlayer = new AudioStreamPlayer { Bus = "Music" };
        _enginePlayer = new AudioStreamPlayer { Bus = "Engine", StreamPaused = true };
        AddChild(_musicPlayer);
        AddChild(_enginePlayer);

        // Create SFX bus players
        for (int i = 0; i < 8; i++)
        {
            var player = new AudioStreamPlayer { Bus = "SFX" };
            _sfxPlayers[$"sfx_{i}"] = player;
            AddChild(player);
        }
    }

    public void PlayMusic(string resourcePath, bool loop = true, float fadeTime = 1f)
    {
        if (_currentMusic == resourcePath && _musicPlayer.Playing) return;

        var stream = ResourceLoader.Load<AudioStream>(resourcePath);
        if (stream == null)
        {
            GD.PrintErr($"Music not found: {resourcePath}");
            return;
        }

        _musicPlayer.Stream = stream;
        _musicPlayer.VolumeDb = Mathf.LinearToDb(MusicVolume * MasterVolume);
        _musicPlayer.Play();
        _currentMusic = resourcePath;
    }

    public void StopMusic(float fadeTime = 1f)
    {
        _musicPlayer.Stop();
        _currentMusic = "";
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp(volume, 0f, 1f);
        _musicPlayer.VolumeDb = Mathf.LinearToDb(MusicVolume * MasterVolume);
    }

    public void PlayEngineSound(string resourcePath, float rpm, float maxRpm)
    {
        var stream = ResourceLoader.Load<AudioStream>(resourcePath);
        if (stream == null) return;

        _enginePlayer.Stream = stream;
        _enginePlayer.VolumeDb = Mathf.LinearToDb(EngineVolume * MasterVolume * 0.5f);
        _enginePlayer.PitchScale = 0.5f + (rpm / maxRpm) * 1.5f;
        
        if (!_enginePlayer.Playing)
        {
            _enginePlayer.Play();
        }
    }

    public void StopEngineSound()
    {
        _enginePlayer.Stop();
    }

    public void SetEngineVolume(float volume)
    {
        EngineVolume = Mathf.Clamp(volume, 0f, 1f);
        _enginePlayer.VolumeDb = Mathf.LinearToDb(EngineVolume * MasterVolume * 0.5f);
    }

    public void PlaySfx(string resourcePath, float volume = 1f, float pitchVariation = 0.1f)
    {
        var stream = ResourceLoader.Load<AudioStream>(resourcePath);
        if (stream == null) return;

        // Find available player
        foreach (var player in _sfxPlayers.Values)
        {
            if (!player.Playing)
            {
                player.Stream = stream;
                player.VolumeDb = Mathf.LinearToDb(SfxVolume * MasterVolume * volume);
                player.PitchScale = 1f + (float)GD.RandRange(-pitchVariation, pitchVariation);
                player.Play();
                return;
            }
        }

        // All busy, use first one
        var firstPlayer = _sfxPlayers.Values.First();
        firstPlayer.Stream = stream;
        firstPlayer.VolumeDb = Mathf.LinearToDb(SfxVolume * MasterVolume * volume);
        firstPlayer.PitchScale = 1f + (float)GD.RandRange(-pitchVariation, pitchVariation);
        firstPlayer.Play();
    }

    public void PlaySfxAtPosition(string resourcePath, Vector2 position, float volume = 1f)
    {
        // For 2D positional audio, we'd use AudioStreamPlayer2D
        // Simplified: just play with volume based on distance to camera
        PlaySfx(resourcePath, volume);
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp(volume, 0f, 1f);
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp(volume, 0f, 1f);
        _musicPlayer.VolumeDb = Mathf.LinearToDb(MusicVolume * MasterVolume);
        _enginePlayer.VolumeDb = Mathf.LinearToDb(EngineVolume * MasterVolume * 0.5f);
    }
}