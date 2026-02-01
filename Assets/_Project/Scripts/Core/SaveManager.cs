using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GameContracts;

[Serializable]
public class FlagEntry
{
    public string key;
    public FlagValueDto value;
}

[Serializable]
public class SaveData
{
    public List<FlagEntry> flags;
    public PlayerStateDataModel playerState;
    public InteractablesSnapshot interactables;
}

public class SaveManager
{
    private const string SAVE_FILE_NAME = "savegame.json";
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    public void Save(FlagManager flagManager, PlayerState playerState, InteractableSaveService interactables)
    {
        var flagsSnapshot = flagManager.Snapshot();
        var flagList = flagsSnapshot.Select(kvp => new FlagEntry { key = kvp.Key, value = kvp.Value }).ToList();

        var playerStateSnapshot = playerState.Snapshot();
        var interactablesSnapshot = interactables?.Snapshot();

        var saveData = new SaveData
        {
            flags = flagList,
            playerState = playerStateSnapshot,
            interactables = interactablesSnapshot
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SaveFilePath, json);
    }

    public void Load(FlagManager flagManager, PlayerState playerState, InteractableSaveService interactables)
    {
        if (!HasSave())
            return;

        string json = File.ReadAllText(SaveFilePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        if (saveData == null)
            return;

        if (saveData.flags != null)
        {
            var flagsDict = saveData.flags.ToDictionary(entry => entry.key, entry => entry.value);
            flagManager.LoadFromSnapshot(flagsDict);
        }

        if (saveData.playerState != null && playerState != null)
            playerState.LoadFromSnapshot(saveData.playerState);

        if (saveData.interactables != null && interactables != null)
            interactables.LoadFromSnapshot(saveData.interactables);
    }

    public bool HasSave()
    {
        return File.Exists(SaveFilePath);
    }
}
