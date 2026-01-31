Place ItemDefinition ScriptableObject assets here.
Create via: Right-click > Create > Inventory > Item Definition.
Name the asset file to match itemId (e.g. Key.asset) if you load by id.

--- ItemDefinition structure (ScriptableObject) ---

  itemId          (string)   Unique id used in code/dialogue (e.g. "key", "health_potion").
  displayName     (string)   Shown in inventory UI (e.g. "Rusty Key", "Health Potion").
  icon            (Sprite)   Optional. Sprite shown in inventory list.
  stackable       (bool)     If true, multiple of same item stack (count); if false, one per slot.
  consumeOnUse    (bool)     If true, item is removed when successfully used on an IItemUseTarget.

Example (Inspector values):

  itemId:         "key"
  displayName:    "Rusty Key"
  icon:           (assign a Sprite)
  stackable:      false
  consumeOnUse:   false

  itemId:         "health_potion"
  displayName:    "Health Potion"
  icon:           (assign a Sprite)
  stackable:      true
  consumeOnUse:   true
