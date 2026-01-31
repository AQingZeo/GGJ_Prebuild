using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueUIController dialogueUIController;
    [SerializeField] private ChoiceUIController choiceUIController;

    private DialogueDataLoader dataLoader;
    private DialogueCommandExecutor commandExecutor;
    private DialogueDataModel currentDialogue;
    private string currentNodeId;
    private string currentDialogueId;
    private bool isDialogueActive = false;
    private GameState previousState; // Store state before dialogue

    private void Awake()
    {
        dataLoader = new DialogueDataLoader();
        commandExecutor = new DialogueCommandExecutor();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Dialogue = this;
            if (!string.IsNullOrEmpty(GameManager.Instance.PendingDialogueId))
            {
                string id = GameManager.Instance.PendingDialogueId;
                GameManager.Instance.PendingDialogueId = null;
                StartDialogue(id);
            }
        }
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.Dialogue == this)
            GameManager.Instance.Dialogue = null;
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        EventBus.Subscribe<InputIntentEvent>(OnInputIntent);
    }

    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        EventBus.Unsubscribe<InputIntentEvent>(OnInputIntent);
    }

    // Called by EventBus when GameState changes
    private void OnGameStateChanged(GameStateChanged stateChange)
    {
        // Edge case: if dialogue is active but state changed away from Dialogue, clean up
        if (isDialogueActive && stateChange.NewState != GameState.Dialogue)
        {
            // End() will be called explicitly in normal flow, but handle edge cases
            End();
        }
    }

    // Called by EventBus when InputRouter publishes input intents
    // Only process input when dialogue is active and in Dialogue state
    private void OnInputIntent(InputIntentEvent inputIntent)
    {
        if (!isDialogueActive) return;
        if (GameStateMachine.Instance == null || GameStateMachine.Instance.CurrentState != GameState.Dialogue) return;

        // If choices are visible, ignore input (choices are selected via UI buttons only)
        if (HasChoices()) return;

        // Any button (submit or click) to advance dialogue
        if (inputIntent.SubmitDown || inputIntent.ClickDown)
        {
            SkipTypewriter();
        }
    }

    /// <summary>
    /// Start a dialogue by ID. Loads the dialogue data and displays the first node.
    /// </summary>
    public void StartDialogue(string dialogueId)
    {
        if (isDialogueActive)
        {
            Debug.LogWarning($"Dialogue already active. Ending current dialogue before starting {dialogueId}");
            End();
        }
        
        // Validate required components
        if (dialogueUIController == null)
        {
            Debug.LogError($"DialogueManager.StartDialogue: DialogueUIController is null! Cannot start dialogue {dialogueId}. " +
                          $"Please assign DialogueUIController in Inspector on DialogueManager GameObject.");
            return;
        }
        
        // Load dialogue data
        currentDialogue = dataLoader.Load(dialogueId);
        if (currentDialogue == null)
        {
            Debug.LogError($"DialogueManager.StartDialogue: Failed to load dialogue: {dialogueId}");
            return;
        }

        currentDialogueId = dialogueId;
        isDialogueActive = true;
        dialogueUIController.ClearHistory();

        // Save previous state: if already Dialogue (set by ItemInteractable), use Explore
        if (GameStateMachine.Instance != null)
        {
            previousState = GameStateMachine.Instance.CurrentState == GameState.Dialogue 
                ? GameState.Explore 
                : GameStateMachine.Instance.CurrentState;
            GameStateMachine.Instance.SetState(GameState.Dialogue);
        }
        else
        {
            previousState = GameState.Explore;
        }

        // Start from "start" node (or first node if "start" doesn't exist)
        if (currentDialogue.nodes != null && currentDialogue.nodes.Count > 0)
        {
            currentNodeId = currentDialogue.nodes.ContainsKey("start") ? "start" : 
                           new List<string>(currentDialogue.nodes.Keys)[0];
            ProcessCurrentNode();
        }
        else
        {
            Debug.LogError($"DialogueManager.StartDialogue: Dialogue {dialogueId} has no nodes");
            End();
        }
    }

    /// <summary>
    /// Advance to the next dialogue node.
    /// Always accepts input when in Dialogue state (pause is handled elsewhere).
    /// </summary>
    public void Advance()
    {
        if (!isDialogueActive)
        {
            Debug.LogWarning("Cannot advance: No active dialogue");
            return;
        }

        if (string.IsNullOrEmpty(currentNodeId))
        {
            End();
            return;
        }

        DialogueNode currentNode = GetCurrentNode();
        if (currentNode == null)
        {
            End();
            return;
        }

        // If there are choices, don't auto-advance
        if (HasChoices())
        {
            return;
        }

        // Move to next node
        if (!string.IsNullOrEmpty(currentNode.nextNodeId))
        {
            // Clear history before moving to next node to prevent text accumulation
            dialogueUIController?.ClearHistory();
            currentNodeId = currentNode.nextNodeId;
            ProcessCurrentNode();
        }
        else
        {
            End();
        }
    }

    /// <summary>
    /// Select a choice by index. Executes choice commands and moves to the next node.
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (!isDialogueActive)
        {
            Debug.LogWarning("Cannot select choice: No active dialogue");
            return;
        }

        DialogueNode currentNode = GetCurrentNode();
        if (currentNode == null || currentNode.choices == null)
        {
            Debug.LogWarning("No choices available on current node");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Count)
        {
            Debug.LogError($"Invalid choice index: {choiceIndex}");
            return;
        }

        DialogueChoice selectedChoice = currentNode.choices[choiceIndex];

        // Clear choices immediately
        choiceUIController?.ClearChoices();

        // Clear dialogue history for next node
        dialogueUIController?.ClearHistory();

        // Execute choice commands
        if (selectedChoice.commands != null && selectedChoice.commands.Count > 0)
        {
            commandExecutor.ExecuteCommands(selectedChoice.commands);
        }

        // Move to next node
        if (!string.IsNullOrEmpty(selectedChoice.nextNodeId))
        {
            currentNodeId = selectedChoice.nextNodeId;
            ProcessCurrentNode();
        }
        else
        {
            End();
        }
    }

    /// <summary>
    /// End the current dialogue and return to Explore state.
    /// </summary>
    public void End()
    {
        if (!isDialogueActive)
        {
            return;
        }

        string endedDialogueId = currentDialogueId;

        isDialogueActive = false;
        currentDialogue = null;
        currentNodeId = null;
        currentDialogueId = null;

        dialogueUIController?.Hide();
        choiceUIController?.ClearChoices();

        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.SetState(previousState);
        }

        // Notify listeners that dialogue ended (for sequential action execution)
        EventBus.Publish(new DialogueEnded(endedDialogueId));
    }

    /// <summary>
    /// Process the current node: display text, execute commands, show choices.
    /// Choices are shown AFTER typewriter completes or is skipped.
    /// </summary>
    private void ProcessCurrentNode()
    {
        DialogueNode node = GetCurrentNode();
        if (node == null)
        {
            End();
            return;
        }

        // Check if this is the "end" node - end dialogue immediately
        if (currentNodeId == "end")
        {
            if (node.commands != null && node.commands.Count > 0)
            {
                commandExecutor.ExecuteCommands(node.commands);
            }
            End();
            return;
        }

        // Execute node commands
        if (node.commands != null && node.commands.Count > 0)
        {
            commandExecutor.ExecuteCommands(node.commands);
        }

        // Clear choices first (they'll be shown after typing completes)
        choiceUIController?.ClearChoices();

        // Display dialogue text
        if (HasChoices())
        {
            dialogueUIController.ShowLine(node.speaker, node.text, () => ShowChoicesForCurrentNode());
        }
        else
        {
            dialogueUIController.ShowLine(node.speaker, node.text);
        }
    }

    /// <summary>
    /// Show choices for the current dialogue node.
    /// Called after typewriter completes. Delay is handled by ChoiceUIController.
    /// </summary>
    private void ShowChoicesForCurrentNode()
    {
        DialogueNode node = GetCurrentNode();
        if (node == null || !HasChoices()) return;

        // Extract choice texts and pass to ChoiceUIController
        List<string> choiceTexts = new List<string>();
        foreach (var choice in node.choices)
        {
            choiceTexts.Add(choice.text);
        }
        
        choiceUIController?.ShowChoices(choiceTexts);
    }

    /// <summary>
    /// Get the current dialogue node.
    /// </summary>
    private DialogueNode GetCurrentNode()
    {
        if (currentDialogue == null || currentDialogue.nodes == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(currentNodeId))
        {
            return null;
        }

        if (currentDialogue.nodes.TryGetValue(currentNodeId, out DialogueNode node))
        {
            return node;
        }

        Debug.LogError($"Node not found: {currentNodeId}");
        return null;
    }

    /// <summary>
    /// Check if dialogue is currently active.
    /// </summary>
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    /// <summary>
    /// Get the current dialogue ID.
    /// </summary>
    public string GetCurrentDialogueId()
    {
        return currentDialogue?.dialogueID;
    }

    /// <summary>
    /// Check if the current node has choices available.
    /// </summary>
    public bool HasChoices()
    {
        if (!isDialogueActive) return false;

        DialogueNode currentNode = GetCurrentNode();
        return currentNode?.choices?.Count > 0;
    }

    /// <summary>
    /// Skip the typewriter effect and show full text immediately.
    /// If typewriter is not active and there are no choices, advances to the next node.
    /// </summary>
    public void SkipTypewriter()
    {
        if (!isDialogueActive)
        {
            return;
        }

        // Skip typewriter if it's currently typing
        if (dialogueUIController?.IsTyping() == true)
        {
            dialogueUIController.SkipTypewriter();
        }
        else if (!HasChoices())
        {
            // If not typing and no choices, advance to next node
            Advance();
        }
        // If there are choices, do nothing - wait for user to select a choice
    }
}
