using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data model for player save/load. Only inventoryIds is serialized.
/// (Level, health, sanity, masks are not used; masks are flags e.g. mask_00_on.)
/// </summary>
[Serializable]
public class PlayerStateDataModel
{
    /// <summary>Serialized to save file. Items are pickup-once; runtime inventory rebuilt from this on load.</summary>
    public List<string> inventoryIds = new List<string>();
}
