using System;
using UnityEngine;
using GameContracts;

/// <summary>
/// Runtime context for conditions and actions. Implemented by GameManager.
/// </summary>
public interface IInteractableContext
{
    FlagManager Flags { get; }
    InventoryService Inventory { get; }
    InteractableSaveService Interactables { get; }
    PlayerState Player { get; }
    DialogueManager Dialogue { get; }
    GameStateMachine StateMachine { get; }
    RoomLoader RoomLoader { get; }
}

// ---- Serializable wrappers (show correctly in Inspector) ----

public enum ConditionType
{
    FlagBool,
    FlagInt,
    HasInventoryItem,
    InteractableState,
    EquippedMask
}

public enum ActionType
{
    StartDialogue,
    SetFlag,
    AddInventoryItem,
    RemoveInventoryItem,
    SetInteractableState,
    ConsumeInteractable,
    SetEquippedMask,
    LoadRoom
}

[Serializable]
public class SerializableCondition
{
    public ConditionType type = ConditionType.FlagBool;
    [Header("FlagBool / FlagInt")]
    public string key = "";
    public bool expected = true;
    [Header("FlagInt")]
    public FlagIntCompareCondition.Op intOp = FlagIntCompareCondition.Op.Equal;
    public int intValue = 0;
    [Header("HasInventoryItem")]
    public string itemId = "";
    public int count = 1;
    [Header("InteractableState")]
    public string interactableId = "self";
    public InteractableStateCompareCondition.Op stateOp = InteractableStateCompareCondition.Op.Equal;
    public int stateValue = 0;
    [Header("EquippedMask")]
    public string maskId = "";

    public static bool Eval(SerializableCondition c, IInteractableContext ctx, string selfId)
    {
        if (c == null || ctx == null) return false;
        switch (c.type)
        {
            case ConditionType.FlagBool:
                return ctx.Flags != null && ctx.Flags.Get(c.key, false) == c.expected;
            case ConditionType.FlagInt:
                if (ctx.Flags == null) return false;
                int actual = ctx.Flags.Get(c.key, 0);
                switch (c.intOp)
                {
                    case FlagIntCompareCondition.Op.Equal: return actual == c.intValue;
                    case FlagIntCompareCondition.Op.NotEqual: return actual != c.intValue;
                    case FlagIntCompareCondition.Op.Less: return actual < c.intValue;
                    case FlagIntCompareCondition.Op.LessOrEqual: return actual <= c.intValue;
                    case FlagIntCompareCondition.Op.Greater: return actual > c.intValue;
                    case FlagIntCompareCondition.Op.GreaterOrEqual: return actual >= c.intValue;
                    default: return false;
                }
            case ConditionType.HasInventoryItem:
                return ctx.Inventory != null && !string.IsNullOrEmpty(c.itemId) && ctx.Inventory.GetCount(c.itemId) >= c.count;
            case ConditionType.InteractableState:
                if (ctx.Interactables == null) return false;
                string id = (c.interactableId == "self" || string.IsNullOrEmpty(c.interactableId)) ? selfId : c.interactableId;
                int state = ctx.Interactables.GetState(id, 0);
                switch (c.stateOp)
                {
                    case InteractableStateCompareCondition.Op.Equal: return state == c.stateValue;
                    case InteractableStateCompareCondition.Op.NotEqual: return state != c.stateValue;
                    case InteractableStateCompareCondition.Op.Less: return state < c.stateValue;
                    case InteractableStateCompareCondition.Op.Greater: return state > c.stateValue;
                    default: return false;
                }
            case ConditionType.EquippedMask:
                return true;
            default:
                return false;
        }
    }
}

[Serializable]
public class SerializableAction
{
    public ActionType type = ActionType.SetFlag;
    [Header("StartDialogue")]
    public string dialogueId = "";
    [Header("SetFlag")]
    public string key = "";
    public bool boolValue = false;
    public int intValue = 0;
    public bool useInt = false;
    [Header("Add/Remove Inventory")]
    public string itemId = "";
    [Header("SetInteractableState / ConsumeInteractable")]
    public string targetId = "self";
    public int newState = 0;
    [Header("SetEquippedMask")]
    public string maskId = "";
    [Header("LoadRoom")]
    public string roomSceneName = "";
    public string spawnPoint = "";

    public static void Execute(SerializableAction a, IInteractableContext ctx, string selfId, string selectedItemId)
    {
        if (a == null || ctx == null) return;
        switch (a.type)
        {
            case ActionType.StartDialogue:
                if (string.IsNullOrEmpty(a.dialogueId)) return;
                if (GameManager.Instance != null) GameManager.Instance.PendingDialogueId = a.dialogueId;
                if (ctx.StateMachine != null) ctx.StateMachine.SetState(GameState.Dialogue);
                if (ctx.Dialogue != null) ctx.Dialogue.StartDialogue(a.dialogueId);
                break;
            case ActionType.SetFlag:
                if (ctx.Flags == null || string.IsNullOrEmpty(a.key)) return;
                if (a.useInt) ctx.Flags.Set(a.key, a.intValue); else ctx.Flags.Set(a.key, a.boolValue);
                break;
            case ActionType.AddInventoryItem:
                if (ctx.Inventory != null && !string.IsNullOrEmpty(a.itemId)) ctx.Inventory.AddItem(a.itemId);
                break;
            case ActionType.RemoveInventoryItem:
                if (ctx.Inventory != null && !string.IsNullOrEmpty(a.itemId)) ctx.Inventory.RemoveItem(a.itemId);
                break;
            case ActionType.SetInteractableState:
                if (ctx.Interactables == null) return;
                string setId = (a.targetId == "self" || string.IsNullOrEmpty(a.targetId)) ? selfId : a.targetId;
                ctx.Interactables.SetState(setId, a.newState);
                break;
            case ActionType.ConsumeInteractable:
                if (ctx.Interactables == null) return;
                string consumeId = (a.targetId == "self" || string.IsNullOrEmpty(a.targetId)) ? selfId : a.targetId;
                ctx.Interactables.Consume(consumeId);
                break;
            case ActionType.SetEquippedMask:
                break;
            case ActionType.LoadRoom:
                if (!string.IsNullOrEmpty(a.roomSceneName))
                    ctx.RoomLoader?.LoadRoom(a.roomSceneName, string.IsNullOrEmpty(a.spawnPoint) ? null : a.spawnPoint);
                break;
        }
    }
}

// ---- Conditions (plain serializable, kept for compatibility) ----

[Serializable]
public abstract class InteractableCondition
{
    public abstract bool Eval(IInteractableContext ctx, string selfId);
}

[Serializable]
public class FlagBoolCondition : InteractableCondition
{
    public string key = "";
    public bool expected = true;

    public override bool Eval(IInteractableContext ctx, string selfId)
    {
        return ctx?.Flags != null && ctx.Flags.Get(key, false) == expected;
    }
}

[Serializable]
public class FlagIntCompareCondition : InteractableCondition
{
    public string key = "";
    public enum Op { Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual }
    public Op op = Op.Equal;
    public int value = 0;

    public override bool Eval(IInteractableContext ctx, string selfId)
    {
        if (ctx?.Flags == null) return false;
        int actual = ctx.Flags.Get(key, 0);
        switch (op)
        {
            case Op.Equal: return actual == value;
            case Op.NotEqual: return actual != value;
            case Op.Less: return actual < value;
            case Op.LessOrEqual: return actual <= value;
            case Op.Greater: return actual > value;
            case Op.GreaterOrEqual: return actual >= value;
            default: return false;
        }
    }
}

[Serializable]
public class HasInventoryItemCondition : InteractableCondition
{
    public string itemId = "";
    public int count = 1;

    public override bool Eval(IInteractableContext ctx, string selfId)
    {
        if (ctx?.Inventory == null || string.IsNullOrEmpty(itemId)) return false;
        return ctx.Inventory.GetCount(itemId) >= count;
    }
}

[Serializable]
public class InteractableStateCompareCondition : InteractableCondition
{
    public string interactableId = "self";
    public enum Op { Equal, NotEqual, Less, Greater }
    public Op op = Op.Equal;
    public int value = 0;

    public override bool Eval(IInteractableContext ctx, string selfId)
    {
        if (ctx?.Interactables == null) return false;
        string id = (interactableId == "self" || string.IsNullOrEmpty(interactableId)) ? selfId : interactableId;
        int actual = ctx.Interactables.GetState(id, 0);
        switch (op)
        {
            case Op.Equal: return actual == value;
            case Op.NotEqual: return actual != value;
            case Op.Less: return actual < value;
            case Op.Greater: return actual > value;
            default: return false;
        }
    }
}

[Serializable]
public class EquippedMaskCondition : InteractableCondition
{
    public string maskId = "";

    public override bool Eval(IInteractableContext ctx, string selfId)
    {
        return true;
    }
}

// ---- Actions (plain serializable) ----

[Serializable]
public abstract class InteractableAction
{
    public abstract void Execute(IInteractableContext ctx, string selfId, string selectedItemId);
}

[Serializable]
public class StartDialogueAction : InteractableAction
{
    public string dialogueId = "";

    public override void Execute(IInteractableContext ctx, string selfId, string selectedItemId)
    {
        if (string.IsNullOrEmpty(dialogueId)) return;
        if (GameManager.Instance != null)
            GameManager.Instance.PendingDialogueId = dialogueId;
        if (ctx?.StateMachine != null)
            ctx.StateMachine.SetState(GameState.Dialogue);
        if (ctx?.Dialogue != null)
            ctx.Dialogue.StartDialogue(dialogueId);
    }
}

[Serializable]
public class SetFlagAction : InteractableAction
{
    public string key = "";
    public bool boolValue = false;
    public int intValue = 0;
    public bool useInt = false;

    public override void Execute(IInteractableContext ctx, string selfId, string selectedItemId)
    {
        if (ctx?.Flags == null || string.IsNullOrEmpty(key)) return;
        if (useInt)
            ctx.Flags.Set(key, intValue);
        else
            ctx.Flags.Set(key, boolValue);
    }
}

[Serializable]
public class AddInventoryItemAction : InteractableAction
{
    public string itemId = "";

    public override void Execute(IInteractableContext ctx, string selfId, string selectedItemId)
    {
        if (ctx?.Inventory == null || string.IsNullOrEmpty(itemId)) return;
        ctx.Inventory.AddItem(itemId);
    }
}

[Serializable]
public class RemoveInventoryItemAction : InteractableAction
{
    public string itemId = "";

    public override void Execute(IInteractableContext ctx, string selfId, string selectedItemId)
    {
        if (ctx?.Inventory == null || string.IsNullOrEmpty(itemId)) return;
        ctx.Inventory.RemoveItem(itemId);
    }
}

[Serializable]
public class SetInteractableStateAction : InteractableAction
{
    public string targetId = "self";
    public int newState = 0;

    public override void Execute(IInteractableContext ctx, string selfId, string selectedItemId)
    {
        if (ctx?.Interactables == null) return;
        string id = (targetId == "self" || string.IsNullOrEmpty(targetId)) ? selfId : targetId;
        ctx.Interactables.SetState(id, newState);
    }
}

[Serializable]
public class ConsumeInteractableAction : InteractableAction
{
    public string targetId = "self";

    public override void Execute(IInteractableContext ctx, string selfId, string selectedItemId)
    {
        if (ctx?.Interactables == null) return;
        string id = (targetId == "self" || string.IsNullOrEmpty(targetId)) ? selfId : targetId;
        ctx.Interactables.Consume(id);
    }
}

[Serializable]
public class SetEquippedMaskAction : InteractableAction
{
    public string maskId = "";

    public override void Execute(IInteractableContext ctx, string selfId, string selectedItemId)
    {
    }
}

[Serializable]
public class LoadRoomAction : InteractableAction
{
    [Tooltip("Scene name to load additively (e.g. Room_Hallway).")]
    public string roomSceneName = "";
    [Tooltip("Optional: name of a GameObject in the room to move the player to (e.g. Spawn_FromStartRoom).")]
    public string spawnPoint = "";

    public override void Execute(IInteractableContext ctx, string selfId, string selectedItemId)
    {
        if (string.IsNullOrEmpty(roomSceneName)) return;
        ctx?.RoomLoader?.LoadRoom(roomSceneName, string.IsNullOrEmpty(spawnPoint) ? null : spawnPoint);
    }
}
