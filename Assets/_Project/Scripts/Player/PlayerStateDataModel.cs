using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define the data model for player's stats with Default/initialized value.
/// Keep Dictionary of Inventory.
/// </summary>
[System.Serializable]
public class PlayerStateDataModel
{
    public int level = 0;
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int currentSan = 100;
    public int minSan = 0;
    
    public Dictionary<string, object> inventory = new Dictionary<string, object>();
}