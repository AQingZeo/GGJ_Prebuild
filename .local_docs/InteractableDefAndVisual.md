# Interactable: Def vs Visual (sample setup)

## How they link to InteractableCore

- **Def (InteractableDefinition)** = **ScriptableObject** that defines *what* happens when the player interacts (rules, one-shot, visibility, use-item). You assign it once per interactable type; many objects can share the same Def.
- **Visual (InteractableVisualState)** = **MonoBehaviour** on the same GameObject (or a child) that controls *how* it looks per saved state (e.g. state 0 = full bottle, state 1 = empty). InteractableCore calls `visual.ApplyState(state)` on load and when actions change state.

**InteractableCore** needs:
- **Def** (required): the ScriptableObject with id, rules, oneShot, etc.
- **Id Override** (optional): use when several objects share one Def but each must have a unique save id (e.g. "poison_1", "poison_2").
- **Visual** (optional): only if the object changes sprite (or collider) based on saved state.

---

## Sample Def: “Poison pickup” (one-shot, add to inventory, then disappear)

1. In Project: **Right-click → Create → Interaction → Interactable Definition**.
2. Name the asset (e.g. `PoisonPickup_Def`).
3. Set in Inspector:

| Field | Value |
|-------|--------|
| **Id** | `poison_picked` |
| **Interaction Type** | Both |
| **One Shot** | ✓ true |

4. **Rules** (Normal interaction – one rule):
   - Add one **Interaction Rule**.
   - **When** (conditions): leave empty = “always match”.
   - **Do** (actions): add these (use the list’s “Add” and pick the type):
     - **Add Inventory Item Action** → `itemId` = `poison`
     - **Consume Interactable Action** → `targetId` = `self` (so this object is marked consumed and hidden on load)
   - **Stop After Match** = ✓ true

5. Optional: add **Start Dialogue Action** → `dialogueId` = `intro` if you want dialogue before it disappears.

Result: one click/trigger adds `poison` to inventory, marks this interactable consumed, and (if one-shot) hides it. After save/load it stays hidden because consumed is saved.

---

## Sample Def: “Door” (change state + sprite, no consumption)

1. Create another **Interactable Definition** (e.g. `Door_Def`).
2. Set:

| Field | Value |
|-------|--------|
| **Id** | `door_01` |
| **Interaction Type** | Both |
| **One Shot** | ☐ false |

3. **Rules** – one rule:
   - **When**: empty (always).
   - **Do**:
     - **Set Interactable State Action** → `targetId` = `self`, `newState` = `1`
   - **Stop After Match** = ✓ true

4. On the door object you also add **InteractableVisualState** (see below) and assign **Visual** on InteractableCore. State 0 = closed sprite, state 1 = open sprite.

---

## Sample Visual: state → sprite

1. On the **same GameObject** as **InteractableCore** (or a child), add component **Interactable Visual State**.
2. In Inspector:

| Field | Value |
|-------|--------|
| **Sprite Renderer** | Drag the SpriteRenderer that shows the object (or leave empty to use same object’s). |
| **Default Sprite** | Sprite for state 0 (e.g. “poison full” or “door closed”). |
| **State Sprites** | List of (state index, sprite): e.g. `0` → full sprite, `1` → empty/open sprite. |
| **Hide If State Missing** | ☐ unless you want to hide when state has no sprite. |
| **Optional Collider** | Optional Collider2D to enable/disable with state. |

3. On **InteractableCore**, set **Visual** to this InteractableVisualState (same object or child).

When is it used?

- **On load:** InteractableCore reads `Interactables.GetState(_id, 0)` and calls `visual.ApplyState(state)` so the sprite matches the saved state.
- **After an action:** If a rule runs **Set Interactable State Action**, InteractableCore calls `visual.ApplyState(newState)` so the sprite updates immediately.

---

## Quick link checklist

| What | Where | Assign to |
|------|--------|-----------|
| **Def** | InteractableCore (on the interactable in scene) | Your InteractableDefinition asset (e.g. PoisonPickup_Def, Door_Def). |
| **Id Override** | InteractableCore | Only if multiple objects share one Def and need unique save ids (e.g. `poison_scene2`). |
| **Visual** | InteractableCore | The InteractableVisualState on this object or a child (only needed if you use state → sprite). |

---

## Example hierarchy (door with visual)

```
Door (GameObject)
├── InteractableCore     → Def = Door_Def, Visual = (below)
├── InteractableVisualState  → Default Sprite = closed, State Sprites = [1 → open]
├── SpriteRenderer
└── Collider2D
```

Poison pickup (one-shot, no state change):

```
Poison (GameObject)
├── InteractableCore     → Def = PoisonPickup_Def, Visual = (none)
├── SpriteRenderer
└── Collider2D
```
