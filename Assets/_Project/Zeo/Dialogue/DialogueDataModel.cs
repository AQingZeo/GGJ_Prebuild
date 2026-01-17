using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueDataModel 
{
    public string dialogueID;
    public Dictionary<string, DialogueNode> nodes;
}

[System.Serializable]
public class DialogueNode
{
    // Speaker name 
    public string speaker;

    // Dialogue content to be displayed
    public string text;

    // Optional: if a dialogue node changes something in the game (sound, world)
    public List<DialogueCommand> commands;

    // Optional choices (if empty/null, auto-advance or end)
    public List<DialogueChoice> choices;

    // Optional next node if no choices
    public string nextNodeId;
}

[System.Serializable]
public class DialogueChoice
{
    // Text shown on the button
    public string text;

    // Node to jump to if selected
    public string nextNodeId;

    // Optional: if a dialogue choice changes something in the game (sound, world)
    public List<DialogueCommand> commands;
}

[System.Serializable]
public class DialogueCommand
{
    // Command keyword: flag, sfx, world, etc.
    public string command;

    // Arguments: ["set", "hasKey", "true"]
    public List<string> args;
}