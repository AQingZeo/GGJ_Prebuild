using UnityEngine;
using GameContracts;

/// <summary>
/// Subclass of ItemInteractable: adds itemId pickup via InventoryService,
/// optional flag/dialogue via base. Respects destroyOnTrigger and InteractionType.
/// </summary>
public class CollectibleItemInteractable : ItemInteractable
{
    [Header("Collectible")]
    [SerializeField] private string itemId = "";
    [SerializeField] private ItemDefinition itemDefinition; // Optional, for display/stackable info

    private string EffectiveItemId => !string.IsNullOrEmpty(itemId) ? itemId : (itemDefinition != null ? itemDefinition.itemId : "");

    protected override void ExecuteTrigger()
    {
        string id = EffectiveItemId;
        if (!string.IsNullOrEmpty(id) && GameManager.Instance != null && GameManager.Instance.Inventory != null)
        {
            GameManager.Instance.Inventory.AddItem(id);
        }

        base.ExecuteTrigger();
    }
}
