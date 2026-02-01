using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

public class InteractableCore : MonoBehaviour
{
    [SerializeField] private InteractableDefinition def;
    [SerializeField] private string idOverride = "";
    [SerializeField] private InteractableVisualState visual;

    private string _id;
    private bool _consumed;
    private bool _visible = true;
    private bool _playerInTrigger; // For Both: true only while player is colliding
    private bool _waitingForDialogue;
    private Coroutine _actionCoroutine;
    private IInteractableContext Ctx => GameManager.Instance as IInteractableContext;

    private string ConsumedFlagKey => "consumed_" + _id;

    private void Awake()
    {
        // Use idOverride if set, else def.id if set, else fallback to gameObject.name
        _id = !string.IsNullOrEmpty(idOverride) ? idOverride :
              (def != null && !string.IsNullOrEmpty(def.id)) ? def.id : gameObject.name;

        // InteractableCore uses OnTriggerEnter2D: colliders must be Is Trigger for CollisionOnly/Both
        if (def != null && def.interactionType != InteractionType.ClickOnly)
        {
            var col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
                Debug.LogWarning($"InteractableCore '{_id}': Collider2D must have Is Trigger checked for {def.interactionType} to fire. Fix on: {gameObject.name}", this);
        }
    }

    private void Start()
    {
        // Use flags for consumed state (flags serialize correctly)
        if (Ctx?.Flags != null && Ctx.Flags.Get(ConsumedFlagKey, false))
        {
            _consumed = true;
            gameObject.SetActive(false);
            return;
        }

        // Apply saved visual state (Start ensures GameManager is ready)
        ApplyVisualState();

        EvaluateVisibility();
        if (!_visible)
            gameObject.SetActive(false);

        // Subscribe to state changes (for cross-interactable updates)
        EventBus.Subscribe<InteractableStateChanged>(OnStateChanged);
        EventBus.Subscribe<DialogueEnded>(OnDialogueEnded);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<InteractableStateChanged>(OnStateChanged);
        EventBus.Unsubscribe<DialogueEnded>(OnDialogueEnded);
    }

    private void OnStateChanged(InteractableStateChanged e)
    {
        if (e.Id == _id && visual != null)
            visual.ApplyState(e.NewState);
    }

    private void OnDialogueEnded(DialogueEnded e)
    {
        _waitingForDialogue = false;
    }

    private void ApplyVisualState()
    {
        if (visual != null && Ctx?.Interactables != null)
        {
            int state = Ctx.Interactables.GetState(_id, 0);
            visual.ApplyState(state);
        }
    }

    private void EvaluateVisibility()
    {
        if (def?.visibilityRules == null || def.visibilityRules.Count == 0) return;
        foreach (var vr in def.visibilityRules)
        {
            if (vr.hideWhen != null)
            {
                foreach (var c in vr.hideWhen)
                {
                    if (c != null && SerializableCondition.Eval(c, Ctx, _id))
                    {
                        _visible = false;
                        return;
                    }
                }
            }
            if (vr.showWhen != null && vr.showWhen.Count > 0)
            {
                bool allShow = true;
                foreach (var c in vr.showWhen)
                {
                    if (c == null || !SerializableCondition.Eval(c, Ctx, _id)) { allShow = false; break; }
                }
                if (!allShow) { _visible = false; return; }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (def == null) return;
        if (def.interactionType == InteractionType.ClickOnly) return;
        if (def.interactionType == InteractionType.Both)
        {
            _playerInTrigger = true;
            return;
        }
        // CollisionOnly: interact on enter
        TryInteract(Ctx?.Inventory?.SelectedItemId ?? "");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (def == null || def.interactionType != InteractionType.Both) return;
        _playerInTrigger = false;
    }

    private void OnMouseDown()
    {
        if (def == null) return;
        if (def.interactionType == InteractionType.CollisionOnly) return;
        if (def.interactionType == InteractionType.Both && !_playerInTrigger) return; // Both: only interact when collide AND click
        if (Ctx?.StateMachine == null || Ctx.StateMachine.CurrentState != GameState.Explore) return;
        TryInteract(Ctx?.Inventory?.SelectedItemId ?? "");
    }

    private void TryInteract(string selectedItemId)
    {
        if (_consumed || !_visible || Ctx == null) return;
        if (_actionCoroutine != null) return; // Already executing actions

        bool useItemMode = !string.IsNullOrEmpty(selectedItemId);
        if (useItemMode)
        {
            TryExecuteUseItemRules(selectedItemId);
            return;
        }

        if (def.rules == null) return;
        foreach (var rule in def.rules)
        {
            if (rule.when == null) continue;
            bool allMatch = true;
            foreach (var c in rule.when)
            {
                if (c == null || !SerializableCondition.Eval(c, Ctx, _id)) { allMatch = false; break; }
            }
            if (!allMatch) continue;

            // Start coroutine to execute actions sequentially
            if (rule.doActions != null && rule.doActions.Count > 0)
            {
                _actionCoroutine = StartCoroutine(ExecuteActionsSequentially(rule.doActions, null, rule.stopAfterMatch));
            }
            else
            {
                FinishInteraction();
            }
            return; // Exit after first matching rule starts
        }
    }

    private void TryExecuteUseItemRules(string selectedItemId)
    {
        if (def.useItemRules == null) return;
        foreach (var rule in def.useItemRules)
        {
            if (!string.IsNullOrEmpty(rule.itemId) && rule.itemId != selectedItemId) continue;
            if (rule.when != null)
            {
                bool allMatch = true;
                foreach (var c in rule.when)
                {
                    if (c == null || !SerializableCondition.Eval(c, Ctx, _id)) { allMatch = false; break; }
                }
                if (!allMatch) continue;
            }

            // Start coroutine to execute actions sequentially
            if (rule.doActions != null && rule.doActions.Count > 0)
            {
                _actionCoroutine = StartCoroutine(ExecuteActionsSequentially(rule.doActions, selectedItemId, true));
            }
            else
            {
                // Handle inventory cleanup even if no actions
                if (Ctx.Inventory != null && Ctx.Inventory.IsConsumeOnUse(selectedItemId))
                    Ctx.Inventory.RemoveItem(selectedItemId);
                Ctx.Inventory?.ClearSelection();
            }
            return;
        }
    }

    private IEnumerator ExecuteActionsSequentially(List<SerializableAction> actions, string selectedItemId, bool stopAfterMatch)
    {
        foreach (var a in actions)
        {
            if (a == null) continue;

            // Check if this is a StartDialogue action
            bool isDialogueAction = (a.type == ActionType.StartDialogue && !string.IsNullOrEmpty(a.dialogueId));

            if (isDialogueAction)
                _waitingForDialogue = true;

            SerializableAction.Execute(a, Ctx, _id, selectedItemId);

            // Wait for dialogue to finish before continuing to next action
            if (isDialogueAction)
            {
                while (_waitingForDialogue)
                    yield return null;
            }
        }

        // Handle inventory cleanup for use-item rules
        if (!string.IsNullOrEmpty(selectedItemId))
        {
            if (Ctx.Inventory != null && Ctx.Inventory.IsConsumeOnUse(selectedItemId))
                Ctx.Inventory.RemoveItem(selectedItemId);
            Ctx.Inventory?.ClearSelection();
        }

        FinishInteraction();
        _actionCoroutine = null;
    }

    private void FinishInteraction()
    {
        if (visual != null)
            visual.ApplyState(Ctx.Interactables.GetState(_id, 0));

        if (def.oneShot)
        {
            Ctx.Flags?.Set(ConsumedFlagKey, true);
            _consumed = true;
            gameObject.SetActive(false);
        }
    }
}
