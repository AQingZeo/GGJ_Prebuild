using UnityEngine;

/// <summary>
/// ScriptableObject definition for an inventory item.
/// itemId, displayName, icon, stackable, consumeOnUse.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Definition", order = 0)]
public class ItemDefinition : ScriptableObject
{
    public string itemId = "";
    public string displayName = "";
    public Sprite icon;
    public bool stackable = false;
    public bool consumeOnUse = false;
}
