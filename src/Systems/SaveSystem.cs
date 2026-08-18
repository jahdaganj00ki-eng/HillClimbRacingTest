using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace HillClimbRacing.Systems;

/// <summary>
/// Handles saving and loading game data to/from JSON file.
/// </summary>
[GlobalClass]
public partial class SaveSystem : Node
{
    public static SaveSystem Instance { get; private set; }

    private const string SaveFileName = "savegame.json";
    private string SavePath => Path.Combine(OS.GetUserDataDir(), SaveFileName);

    [Signal] public delegate void OnSaveCompletedEventHandler(bool success);
    [Signal] public delegate void OnLoadCompletedEventHandler(bool success);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Saves the current game state to disk.
    /// </summary>
    public void SaveGame()
    {
        try
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null) return;

            var saveData = new SaveData
            {
                Version = 1,
                Timestamp = DateTime.UtcNow.Ticks,
                SelectedVehicle = gameManager.SelectedVehicle,
                Coins = gameManager.Coins,
                TotalCoinsCollected = gameManager.TotalCoinsCollected,
                BestDistance = gameManager.BestDistance,
                UnlockedVehicles = new List<int>(),
                UnlockedTracks = new List<string>(),
                VehicleTunings = new Dictionary<string, TuningSaveData>()
            };

            foreach (var vehicle in gameManager.UnlockedVehicles)
            {
                saveData.UnlockedVehicles.Add((int)vehicle);
            }

            foreach (var track in gameManager.UnlockedTracks)
            {
                saveData.UnlockedTracks.Add(track.TrackId);
            }

            foreach (var kvp in gameManager.VehicleTunings)
            {
                var tuning = kvp.Value;
                saveData.VehicleTunings[kvp.Key.ToString()] = new TuningSaveData
                {
                    EngineLevel = tuning.EngineLevel,
                    SuspensionLevel = tuning.SuspensionLevel,
                    TiresLevel = tuning.TiresLevel,
                    WeightLevel = tuning.WeightLevel,
                    DrivetrainLevel = tuning.DrivetrainLevel
                };
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(saveData, options);
            File.WriteAllText(SavePath, json);

            GD.Print($"Game saved to: {SavePath}");
            EmitSignal(SignalName.OnSaveCompleted, true);
            gameManager.EmitSignal(GameManager.SignalName.OnDataSaved);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save game: {e.Message}");
            EmitSignal(SignalName.OnSaveCompleted, false);
        }
    }

    /// <summary>
    /// Loads the game state from disk.
    /// </summary>
    public void LoadGame()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                GD.Print("No save file found, starting fresh");
                EmitSignal(SignalName.OnLoadCompleted, false);
                return;
            }

            string json = File.ReadAllText(SavePath);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var saveData = JsonSerializer.Deserialize<SaveData>(json, options);
            if (saveData == null)
            {
                GD.PrintErr("Failed to deserialize save data");
                EmitSignal(SignalName.OnLoadCompleted, false);
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null) return;

            gameManager.SelectedVehicle = saveData.SelectedVehicle;
            gameManager.Coins = saveData.Coins;
            gameManager.TotalCoinsCollected = saveData.TotalCoinsCollected;
            gameManager.BestDistance = saveData.BestDistance;

            gameManager.UnlockedVehicles.Clear();
            foreach (int v in saveData.UnlockedVehicles)
            {
                gameManager.UnlockedVehicles.Add((VehicleType)v);
            }

            gameManager.UnlockedTracks.Clear();
            var trackSelector = TrackSelector.Instance;
            if (trackSelector != null)
            {
                foreach (string trackId in saveData.UnlockedTracks)
                {
                    var track = trackSelector.GetTrackById(trackId);
                    if (track != null)
                    {
                        gameManager.UnlockedTracks.Add(track);
                    }
                }
            }

            foreach (var kvp in saveData.VehicleTunings)
            {
                if (Enum.TryParse<VehicleType>(kvp.Key, out var vehicleType))
                {
                    var tuning = gameManager.GetTuning(vehicleType);
                    tuning.EngineLevel = kvp.Value.EngineLevel;
                    tuning.SuspensionLevel = kvp.Value.SuspensionLevel;
                    tuning.TiresLevel = kvp.Value.TiresLevel;
                    tuning.WeightLevel = kvp.Value.WeightLevel;
                    tuning.DrivetrainLevel = kvp.Value.DrivetrainLevel;
                }
            }

            GD.Print($"Game loaded from: {SavePath}");
            EmitSignal(SignalName.OnLoadCompleted, true);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load game: {e.Message}");
            EmitSignal(SignalName.OnLoadCompleted, false);
        }
    }

    /// <summary>
    /// Deletes the save file (for reset/new game).
    /// </summary>
    public void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                GD.Print("Save file deleted");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to delete save: {e.Message}");
        }
    }
}

/// <summary>
/// Serializable save data structure.
/// </summary>
public class SaveData
{
    public int Version { get; set; }
    public long Timestamp { get; set; }
    public VehicleType SelectedVehicle { get; set; }
    public int Coins { get; set; }
    public int TotalCoinsCollected { get; set; }
    public float BestDistance { get; set; }
    public List<int> UnlockedVehicles { get; set; } = new();
    public List<string> UnlockedTracks { get; set; } = new();
    public Dictionary<string, TuningSaveData> VehicleTunings { get; set; } = new();
}

public class TuningSaveData
{
    public int EngineLevel { get; set; }
    public int SuspensionLevel { get; set; }
    public int TiresLevel { get; set; }
    public int WeightLevel { get; set; }
    public int DrivetrainLevel { get; set; }
}