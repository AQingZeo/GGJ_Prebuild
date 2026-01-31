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
    private IInteractableContext Ctx => GameManager.Instance as IInteractableContext;

    private void Awake()
    {
        _id = !string.IsNullOrEmpty(idOverride) ? idOverride : (def != null ? def.id : gameObject.name);

        if (Ctx?.Interactables != null && Ctx.Interactables.IsConsumed(_id))
        {
            _consumed = true;
            gameObject.SetActive(false);
            return;
        }

        if (Ctx?.Interactables != null && visual != null)
        {
            int state = Ctx.Interactables.GetState(_id, 0);
            visual.ApplyState(state);
        }

        EvaluateVisibility();
        if (!_visible)
            gameObject.SetActive(false);
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
        if (def == null || def.interactionType == InteractionType.ClickOnly) return;
        TryInteract(Ctx?.Inventory?.SelectedItemId ?? "");
    }

    private void OnMouseDown()
    {
        if (def == null || def.interactionType == InteractionType.CollisionOnly) return;
        if (Ctx?.StateMachine == null || Ctx.StateMachine.CurrentState != GameState.Explore) return;
        TryInteract(Ctx?.Inventory?.SelectedItemId ?? "");
    }

    private void TryInteract(string selectedItemId)
    {
        if (_consumed || !_visible || Ctx == null) return;

        bool useItemMode = !string.IsNullOrEmpty(selectedItemId);
        if (useItemMode)
        {
            if (TryExecuteUseItemRules(selectedItemId))
                return;
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

            if (rule.doActions != null)
            {
                foreach (var a in rule.doActions)
                {
                    if (a != null)
                        SerializableAction.Execute(a, Ctx, _id, null);
                }
            }

            if (visual != null)
                visual.ApplyState(Ctx.Interactables.GetState(_id, 0));

            if (def.oneShot)
            {
                Ctx.Interactables.Consume(_id);
                _consumed = true;
                gameObject.SetActive(false);
            }
            if (rule.stopAfterMatch) return;
        }
    }

    private bool TryExecuteUseItemRules(string selectedItemId)
    {
        if (def.useItemRules == null) return false;
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

            if (rule.doActions != null)
            {
                foreach (var a in rule.doActions)
                {
                    if (a != null)
                        SerializableAction.Execute(a, Ctx, _id, selectedItemId);
                }
            }

            if (Ctx.Inventory != null && Ctx.Inventory.IsConsumeOnUse(selectedItemId))
                Ctx.Inventory.RemoveItem(selectedItemId);
            Ctx.Inventory?.ClearSelection();

            if (visual != null)
                visual.ApplyState(Ctx.Interactables.GetState(_id, 0));

            return true;
        }
        return false;
    }
}
