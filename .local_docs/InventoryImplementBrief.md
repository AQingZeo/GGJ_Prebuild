## **Requirement brief: Item collectible + inventory (escape-room style)**

### **1) Player-facing behavior**

**Collecting**

- When the player acquires an item (by clicking a world object or via dialogue), the item is added to the player’s **Inventory**.
- If the item is already owned:
    - Either allow duplicates (stack) or ignore (unique). Default: **unique** unless item is explicitly stackable.

**Inventory display**

- A left-side UI list shows all owned items (icon + name).
- The list updates immediately when items are added/removed.

**Selecting and using**

- Clicking an inventory item puts the player into **“Use Item mode”** (selected highlight).
- While in Use Item mode:
    - The next click in the game world attempts to use that item on the clicked target.
    - If the target accepts it: play result (unlock/change state/dialogue trigger), optionally consume(item def) the item, and clear selection.
    - If the target rejects it: show a small feedback (optional), clear selection.

**State rules**

- Item usage is only available in **Explore**
- Inventory UI is only visible in Explore

---

### **2) Data requirements**

**Item definition**

- Add to item interactable — for itemInteractable add to is collectible:
    - itemId (unique key, stable for save/load)
    - displayName
    - icon
    - consumeOnUse
    

**Inventory state**

- Inventory stores:
    - List of owned itemIds (already implemented on player states)
    - Optional: currently selected itemId (usually *not* saved; selection resets on load) (either implemented in inventory states, or allow items in inventoryUI to act still as iteminteractable to let user select to enter user item mode)

---

### **3) Core functions (must-have)**

**Inventory**

- AddItem(itemId)
- RemoveItem(itemId)
- HasItem(itemId) -> bool
- GetAllItems() -> list
- SelectItem(itemId) / ClearSelection()
- Events/callbacks:
    - OnInventoryChanged
    - OnSelectedItemChanged

**Collectible acquisition sources**

- From world’s item interactable: Collect() calls AddItem
- From dialogue: GiveItem(itemId) calls AddItem

**Item usage**

- TryUseSelectedItemOn(target) -> result
    - target is “whatever player clicked”
    - result includes: success/failure + optional effect id

---

### **4) Interactions with existing systems/files**

### **A) Save/Load system**

**What to save**

- Inventory content:  [itemId]
- Do not save UI state (selected item)

**Required integration**

- On Save: inventory exports DTO data to your save file.
- On Load: inventory imports DTO data and raises OnInventoryChanged so UI refreshes.

### **B) Player state**

Inventory is part of “player progression state”.

- Inventory should be owned by a persistent manager/service (Bootstrap / GameRoot).
- PlayerController should not own inventory data directly; it can request it.

**Files involved**

- PlayerStateData / PlayerState (if you have)
- Inventory should be referenced as a dependency or via a central manager.

### **C) Item interactables in the world**

Collectibles is a subtype or it is a type of interactable:

- Use IInteractable as base:
    - CollectibleItemInteractable implements/extends it and calls AddItem(itemId) then destroys/disables itself.

### **D) Dialogue system (items granted by dialogue)**

Dialogue choices/events can grant items:

- Add a dialogue command/action like:
    - GiveItem("key_01")
    - RemoveItem("coin")
- This should call Inventory functions, not directly touch UI.

**Files involved**

- DialogueCommandExecutor
- Dialogue JSON schema: add an action entry (e.g., "giveItem": "key_01")

---

### **5) World item-usage targets (must-have interface)**

Any world object that can receive an item should implement a simple contract:

- Input: itemId
- Output: accepted?, plus an effect (unlock, swap sprite, trigger dialogue, set flag).
- Act as iteminteractible. Logic is the inventoryUI can be selected as iteminteractible and only consume when it is consumeOnUse + accepted

---

### **6) Non-functional requirements**

- Inventory operations must be deterministic and safe across scene loads.
- ItemIds must be stable and referenced consistently in:
    - Scriptable item definitions
    - Dialogue JSON
    - Save files
- UI must refresh from inventory state (event-driven or polling once per change), not rebuild every frame.