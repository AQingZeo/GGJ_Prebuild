# Scene linking: Inspector references

Assign these in the Unity Inspector so scripts find their dependencies (no FindObjectOfType at runtime).

---

## Bootstrap scene

| GameObject / Component | Field | Assign to |
|------------------------|--------|------------|
| **SceneController** | `Pause UIController` | The PauseUIController in this scene (e.g. Pause canvas root). |
| *(GameManager, GameStateMachine, InputRouter, etc. are on separate objects; no refs needed.)* | | |

---

## Menu scene

| GameObject / Component | Field | Assign to |
|------------------------|--------|------------|
| **MenuController** | *(if it has refs to GameManager / StateMachine, assign as needed)* | Usually uses `GameManager.Instance`; no scene refs if so. |

---

## Explore scene

| GameObject / Component | Field | Assign to |
|------------------------|--------|------------|
| **InventoryUIController** | `List Container` | RectTransform that holds the slot list (e.g. Content of a ScrollView). |
| | `Item Slot Prefab` | Your inventory slot prefab. |
| | `Panel Root` | GameObject to show/hide (Explore only). Assign a **child** of the controller so hiding it doesn’t disable the controller. |
| | `Display Definitions` | Optional list of ItemDefinition assets for display names/icons. |
| **InteractableCore** (on each interactable) | `Def` | InteractableDefinition asset for this object. |
| | `Id Override` | Optional; use when one definition is reused and each instance needs a unique save id. |
| | `Visual` | Optional InteractableVisualState on this object (for state→sprite). |

---

## Dialogue scene

| GameObject / Component | Field | Assign to |
|------------------------|--------|------------|
| **DialogueManager** | `Dialogue UI Controller` | DialogueUIController in this scene. |
| | `Choice UI Controller` | ChoiceUIController in this scene. |
| **DialogueUIController** | `Canvas Ref` | Optional; Canvas for layout. If empty, uses `GetComponentInParent<Canvas>()`. |
| | `Dialogue Panel`, `Speaker Name Text`, `Dialogue Text`, etc. | Per your existing setup. |

---

## Inventory (Zeo/Inventory)

- **ItemDefinition** assets: create under e.g. `Resources/Items/` or any folder; assign in **InventoryUIController > Display Definitions** and/or on **ItemInteractable** (if still used).
- **InteractableDefinition** assets: Create → Interaction → Interactable Definition; assign to **InteractableCore > Def** on placed objects.

---

## Quick checklist

1. **Bootstrap:** SceneController → Pause UIController.
2. **Explore:** InventoryUIController → List Container, Item Slot Prefab, Panel Root.
3. **Explore:** Each interactable → InteractableCore → Def (and optional Id Override, Visual).
4. **Dialogue:** DialogueManager → Dialogue UI Controller, Choice UI Controller.
