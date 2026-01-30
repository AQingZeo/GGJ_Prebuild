# Inventory Appendix (Item Collectible + Inventory)

Additional hierarchy and responsibilities to layer the inventory feature onto the existing Scripts layout. Mirrors the format of `ScriptsHierarchy.md`.

```
Scripts/
├─ Core/
│  ├─ GameManager.cs          # holds InventoryService singleton reference
│  └─ SaveManager.cs          # already saves PlayerState snapshot (includes inventory dict)
├─ Player/
│  ├─ PlayerState.cs          # owns inventory dictionary + Add/Remove/Has
│  └─ PlayerStateDataModel.cs # serializable data with inventory Dictionary<string, object>
├─ Interaction/
│  ├─ ItemInteractable.cs     # base Mono interactable (flags/dialogue hooks)
│  └─ CollectibleItemInteractable.cs # subclass: adds itemId pickup + destroy handling
├─ Inventory/
│  ├─ InventoryService.cs     # Plain C#, wraps PlayerState inventory, raises events
│  ├─ ItemDefinition.cs       # ScriptableObject (itemId, displayName, icon, stackable, consumeOnUse)
│  ├─ InventoryUIController.cs# Mono UI list (icon+name), selection, visibility in Explore
│  ├─ ItemUseController.cs    # Mono handles “Use Item mode” + world click routing
│  └─ IItemUseTarget.cs       # Plain C# contract for world targets that accept items
├─ Dialogue/
│  └─ DialogueCommandExecutor.cs # add GiveItem/RemoveItem commands -> InventoryService
└─ UI/
   └─ InventoryPanel.prefab/.cs # view prefab hooked to InventoryUIController
```

## Scripts responsibilities and restrictions
├─ Core/
│ GameManager: Mono  
│ - Bootstrap `InventoryService` (plain C#) and expose read-only `Inventory` property alongside Flags/Player/Save.  
│ - InventoryService internally references `GameManager.Player` (PlayerState) to mutate the inventory dictionary defined there.  
│ SaveManager: Plain C#  
│ - Already saves `PlayerStateDataModel`; inventory content is persisted via PlayerState snapshot.  
│ - On Load, after `playerState.LoadFromSnapshot(...)`, call `InventoryService.RaiseInventoryChanged()` to refresh UI.
├─ Player/
│ PlayerState: Plain C#  
│ - Owns `Dictionary<string, object> inventory` (in `PlayerStateDataModel`).  
│ - Provide `AddToInventory(string itemId, object item)`, `RemoveFromInventory(string itemId)`, `HasInInventory(string itemId)`, `GetInventory()`.  
│ - No UI logic; persistence only.  
│ PlayerStateDataModel: Plain C# (Serializable)  
│ - Contains initialized `inventory` dictionary; included in save file.
├─ Interaction/
│ ItemInteractable: Mono  
│ - Base interactable (flag set, optional dialogue trigger).  
│ - No direct inventory writes; serves as parent for collectible subtype.  
│ CollectibleItemInteractable: Mono (subtype of ItemInteractable)  
│ - Configurable `itemId`, optional `ItemDefinition` reference.  
│ - On trigger/click, call `InventoryService.AddItem(itemId)`; optionally set flag/dialogue via base; respect `destroyOnTrigger`.  
│ - Use base InteractionType (Click/Collision/Both) filtering.  
├─ Inventory/
│ InventoryService: Plain C#  
│ - Wraps PlayerState inventory dict; enforces uniqueness unless ItemDefinition.stackable.  
│ - API: `AddItem(string id)`, `RemoveItem(string id)`, `HasItem(string id)`, `GetAllItems()`, `SelectItem(string id)`, `ClearSelection()`.  
│ - Events: `OnInventoryChanged`, `OnSelectedItemChanged`.  
│ - Selection is runtime only (do not write to PlayerState/save).  
│ ItemDefinition: ScriptableObject  
│ - Fields: `string itemId`, `string displayName`, `Sprite icon`, `bool stackable`, `bool consumeOnUse`.  
│ InventoryUIController: Mono  
│ - Subscribed to InventoryService events.  
│ - Renders left-side list (icon+name), refreshes on change, highlights selected item.  
│ - Visible only in `GameState.Explore`; hides otherwise.  
│ ItemUseController: Mono  
│ - Listens for item selection and world clicks (via InputRouter or Unity events).  
│ - Calls `IItemUseTarget.TryUseItem(itemId)` on clicked target; handles success/failure feedback; consumes item when `consumeOnUse` and accepted.  
│ IItemUseTarget: Plain C#  
│ - Interface `bool TryUseItem(string itemId)` (optionally return effect/result struct).  
│ - Implement on world objects that react to inventory items.  
├─ Dialogue/
│ DialogueCommandExecutor: Plain C#  
│ - Add commands: `giveitem <id>` → InventoryService.AddItem(id); `removeitem <id>` → InventoryService.RemoveItem(id).  
│ - No UI responsibilities; works in Dialogue state.  
└─ UI/
│ InventoryPanel: Mono view (hooked to InventoryUIController)  
│ - Contains scroll/list, selection highlight, optional feedback text.  
│ - No business logic; purely view.

## Notes
- Inventory data lives in `PlayerStateDataModel.inventory`; InventoryService is a thin facade over that dictionary so save/load stays unchanged.  
- Items can be acquired through `CollectibleItemInteractable` (world pickups) or dialogue commands; both paths must call InventoryService (not UI).  
- Item usage is only available in `GameState.Explore`; Inventory UI should hide or disable selection in other states.  
- Avoid scene-level singletons for inventory UI; bind via serialized references or scene lookup, but data source remains InventoryService.
