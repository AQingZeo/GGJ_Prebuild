# ItemInteraction change

## **0) Target architecture in one sentence**

Replace the monolithic ItemInteractable with a **data-driven interactable**:

- **InteractableDefinition (SO)** = ordered rules: *(conditions → actions)* + optional visibility rules
- **InteractableCore (Mono)** = runs rules + applies saved state (consumed / state int)
- **Save data** stores:
    - consumedInteractables: HashSet<string>
    - interactableStates: Dictionary<string,int> (for sprite/state changes)
- Keep inventory as (ItemDefinition, InventoryService, InventoryUIController)
- Masks “on/off” are a **player state** (equippedMaskId), not a pile of flags

---

## **1) Files to change / add / remove**

### **A) Keep as-is (no changes required)**

- ItemDefinition.cs (inventory metadata: icon/name/consumeOnUse/(remove stackable))
- InventoryService.cs
- InventoryUIController.cs
- IItemUseTarget.cs
- ItemUseController.cs (minor change; see below)

### **B) Deprecate / remove**

- **Deprecate** ItemInteractable.cs (replace usage)
- Remove IInteractable.cs

Reason: it mixes pickup + flags + dialogue + destroy + save check + coroutine-based dialogue finding. You’ll keep running into “field soup” and hard-to-branch logic.

---

## **2) Add these new scripts (minimal set)**

### **2.1**

### **InteractableDefinition.cs**

### **(ScriptableObject)**

Create a new SO that describes one placed object’s logic.

**Fields**

- string id (unique interactable ID, used by save)
- InteractionType interactionType (reuse your enum or a new one)
- bool oneShot (if true, mark consumed after successful interaction)
- List<VisibilityRule> visibilityRules
- List<InteractionRule> rules (ordered top→bottom)

Where:

- VisibilityRule: List<Condition> showWhen, List<Condition> hideWhen
- InteractionRule: List<Condition> when, List<Action> do, bool stopAfterMatch=true

Evaluation: first matching rule executes.

### **2.2 InteractableCore.cs (MonoBehaviour)**

Attach this to each placed interactable prefab.

This replaces ItemInteractable completely and also handles “use item then click target” interaction (no ItemUseController required).

---

### **Fields**

- **InteractableDefinition def**
    
    Defines interaction rules, use-item rules, visibility rules, and one-shot behavior.
    
- **string idOverride (optional)**
    
    Used when the same definition is reused by multiple placed objects that require unique persistence IDs.
    
- **InteractableVisualState visual (optional)**
    
    Handles sprite / visual changes based on persisted interactable state.
    

---

### **Responsibilities**

- **On Awake / Start**
    - Resolve a unique interactable ID (use idOverride or def.id)
    - If InteractableSaveService.IsConsumed(id) → disable GameObject immediately
    - Load persisted interactable state (int)
    - Apply state to visuals via InteractableVisualState
    - Evaluate visibility rules (if defined)
        - If not visible → disable GameObject or interaction
- **On Click / Trigger**
    - Ignore interaction if consumed or not visible
    - Route interaction by mode:
        - If an inventory item is currently selected → use-item mode
        - Otherwise → normal interaction mode
- **Use-item mode**
    - Evaluate use-item rules defined in InteractableDefinition
    - Match rules based on selected item ID and conditions
    - Execute matched rule actions
    - If the used item is consumeOnUse → remove from inventory
    - Clear selected item after successful use
    - Do not execute normal interaction rules in the same interaction
- **Normal interaction mode**
    - Evaluate interaction rules in order
    - Execute the first matching rule’s actions
    - If the definition is marked one-shot → mark consumed and disable object
- **Visual State changes**
    - Persist new state via InteractableSaveService
    - Update visuals through InteractableVisualState
- **Consumption**
    - Persist consumed state via InteractableSaveService
    - Disable object immediately after consumption
- **Persistence**
    - Do not store persistent state on the MonoBehaviour
    - All consumed and state data must be read from and written to the centralized save service

### **2.3**

### **InteractableVisualState.cs**

### **(MonoBehaviour)**

For “item state changes bond to different sprite”.

**Fields**

- List<StateSprite> mapping int state -> Sprite
- default sprite (state 0)
- optional: bool hideIfStateMissing

**Methods**

- ApplyState(int state) sets sprite (and optionally collider active/inactive)
- Called by InteractableCore after load, and by actions that change state

### **2.4**

### **Conditions and Actions**

### **(small library)**

These can be **plain serializable classes** (not MonoBehaviours) to keep runtime fast and prefab clean.

**Conditions** 

- FlagBoolCondition(key, expected)
- FlagIntCompareCondition(key, op, value)
- HasInventoryItemCondition(itemId, count)
- EquippedMaskCondition(maskId) (player equipment state)
- InteractableStateCompareCondition(id/self, op, value) (for state machine objects)

**Actions** 

- StartDialogueAction(dialogueId)
- SetFlagAction(key, bool/int)
- AddInventoryItemAction(itemId)
- RemoveInventoryItemAction(itemId)
- SetEquippedMaskAction(maskId)
- SetInteractableStateAction(targetId/self, int newState)
- ConsumeInteractableAction(targetId/self) (marks consumed + disables)
- ShowChoiceAction(choices[]) (optional but you likely need it from the sheet)

### **2.5**

### **InteractableSaveService.cs (plain C# service) + DTO changes**

This is in-memory storage + snapshot methods, same pattern as other save systems.

Data:

- HashSet<string> _consumed
- Dictionary<string,int> _states

Methods:

- IsConsumed(id)
- Consume(id)
- GetState(id, default=0)
- SetState(id, state)
- Snapshot() -> InteractablesSnapshot
- LoadFromSnapshot(InteractablesSnapshot snapshot)

This service does **not** write files.

**UpdateSaveManager to include this snapshot**

Change signatures:

- Save(FlagManager flagManager, PlayerState playerState, InteractableSaveService interactables)
- Load(FlagManager flagManager, PlayerState playerState, InteractableSaveService interactables)

And inside:

**Save**

- saveData.interactables = interactables.Snapshot();

**Load**

- interactables.LoadFromSnapshot(saveData.interactables);

Keeps “save in one place”.

**Where this connects to InteractableCore and InteractableVisualState**

- InteractableCore needs access to InteractableSaveService (usually via GameManager.Services.Interactables or a static locator).
- On Awake/Start:
    - If IsConsumed(id) → disable immediately
    - Else GetState(id) → visual.ApplyState(state)

When actions change state:

- SetState(id, newState) and visual.ApplyState(newState)

When consumed:

- Consume(id) and disable/destroy object

You need a central place to store:

- HashSet<string> consumedInteractables
- Dictionary<string,int> interactableStates

You already have PlayerState and services. Add:

- InteractableSaveService owned by GameManager (like Inventory)

---

## **3) Change to existing scripts (small but important)**

### **3.1 Replace dialogue-start logic (remove coroutine scene scanning)**

Right now ItemInteractable does a coroutine and searches DialogueScene root objects to find DialogueManager. That is slow and fragile.

Do this instead:

- DialogueManager registers itself on load:
    - GameManager.Instance.Dialogue = this; (or a DialogueService)
- Your StartDialogueAction simply calls:
    - GameManager.Instance.Dialogue.StartDialogue(dialogueId);
    - and sets state to Dialogue via GameStateMachine

Net effect: faster, cleaner, no “maxAttempts” loops.

---

## **4) Visibility vs one-shot disappearanc**

Use both, but for different purposes:

### **One-shot disappearance (pickup consumed, smashed, taken, etc.)**

Do **NOT** implement as a visibility rule. Implement as:

- On interaction: ConsumeInteractableAction() →
    1. consumedInteractables.Add(id)
    2. disable/destroy object immediately
- On scene load: InteractableCore.Awake() checks IsConsumed(id) and disables before it’s visible.

This guarantees no duplication after save/load.

### **Visibility rules (show/hide based on progress)**

Use visibility for:

- “only appears after flag X”
- “only appears before flag Y”
- “only visible when state == 2”
- “only visible when mask is on” (if needed, though usually interaction changes not visibility)

So:

- **Consumed** = persistence
- **Visibility** = gating

---

##