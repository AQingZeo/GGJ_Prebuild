using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define the data model for player's stats with Default/initialized value.
/// inventoryIds is serialized for save/load (items are pickup-once, no count).
/// </summary>
[Serializable]
public class PlayerStateDataModel
{
    public int level = 0;
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int currentSan = 100;
    public int minSan = 0;

    /// <summary>Serialized to save file. Items are pickup-once; runtime inventory rebuilt from this on load.</summary>
    public List<string> inventoryIds = new List<string>();
}