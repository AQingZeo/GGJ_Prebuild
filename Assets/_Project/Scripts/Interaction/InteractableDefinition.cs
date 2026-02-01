using System;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionType
{
    ClickOnly,
    CollisionOnly,
    Both
}

[Serializable]
public class VisibilityRule
{
    public List<SerializableCondition> showWhen = new List<SerializableCondition>();
    public List<SerializableCondition> hideWhen = new List<SerializableCondition>();
}

[Serializable]
public class InteractionRule
{
    public List<SerializableCondition> when = new List<SerializableCondition>();
    public List<SerializableAction> doActions = new List<SerializableAction>();
    public bool stopAfterMatch = true;
}

[Serializable]
public class UseItemRule
{
    public string itemId = "";
    public List<SerializableCondition> when = new List<SerializableCondition>();
    public List<SerializableAction> doActions = new List<SerializableAction>();
}

[CreateAssetMenu(fileName = "NewInteractable", menuName = "Interaction/Interactable Definition", order = 0)]
public class InteractableDefinition : ScriptableObject
{
    public string id = "";
    public InteractionType interactionType = InteractionType.Both;
    public bool oneShot = false;

    public List<VisibilityRule> visibilityRules = new List<VisibilityRule>();
    public List<InteractionRule> rules = new List<InteractionRule>();
    public List<UseItemRule> useItemRules = new List<UseItemRule>();
}
