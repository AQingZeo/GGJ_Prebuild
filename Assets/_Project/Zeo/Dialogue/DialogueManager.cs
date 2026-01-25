using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueUIController dialogueUIController;
    [SerializeField] private ChoiceUIController choiceUIController;
    [SerializeField] private GameStateMachine gameStateMachine;
    [SerializeField] private FlagManager flagManager;
    [SerializeField] private EventBus eventBus;

    private DialogueDataLoader dataLoader;
    private DialogueDataModel currentDialogue;
    private string currentNodeId;
    private bool isDialogueActive = false;
    private bool isPaused = false;
    private GameState previousState; // Store state before dialogue for pause handling

    // Track if dialogue is the top state (not under pause)
    private bool isTopState = true;

    private void Awake()
    {
        dataLoader = new DialogueDataLoader();
        
        // Auto-find references if not assigned
        if (dialogueUIController == null)
            dialogueUIController = FindObjectOfType<DialogueUIController>();
        if (choiceUIController == null)
            choiceUIController = FindObjectOfType<ChoiceUIController>();
        if (gameStateMachine == null)
            gameStateMachine = FindObjectOfType<GameStateMachine>();
        if (flagManager == null)
            flagManager = FindObjectOfType<FlagManager>();
        if (eventBus == null)
            eventBus = FindObjectOfType<EventBus>();
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        // Subscribe to GameStateChanged events via EventBus
        if (eventBus != null)
        {
            eventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }
    }

    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from events
        if (eventBus != null)
        {
            eventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }
    }

    // Called by EventBus when GameState changes
    private void OnGameStateChanged(GameStateChanged stateChange)
    {
        if (stateChange.To == GameState.Dialogue && !isDialogueActive)
        {
            // Dialogue state was set externally - we're ready to start
            isTopState = true;
        }
        else if (stateChange.To == GameState.Pause && isDialogueActive)
        {
            // Pause overlay activated during dialogue
            isPaused = true;
            isTopState = false;
        }
        else if (stateChange.From == GameState.Pause && stateChange.To == GameState.Dialogue && isDialogueActive)
        {
            // Resuming from pause back to dialogue
            isPaused = false;
            isTopState = true;
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

        // Load dialogue data
        currentDialogue = dataLoader.Load(dialogueId);
        if (currentDialogue == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueId}");
            return;
        }

        isDialogueActive = true;
        isTopState = true;
        isPaused = false;

        // Clear previous dialogue history
        if (dialogueUIController != null)
        {
            dialogueUIController.ClearHistory();
        }

        // Set game state to Dialogue
        if (gameStateMachine != null)
        {
            previousState = gameStateMachine.GetState(); // Store current state
            gameStateMachine.SetState(GameState.Dialogue);
        }
        else
        {
            previousState = GameState.Explore; // Default previous state
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
            Debug.LogError($"Dialogue {dialogueId} has no nodes");
            End();
        }
    }

    /// <summary>
    /// Advance to the next dialogue node. Ignores if paused or not top state.
    /// </summary>
    public void Advance()
    {
        if (!isDialogueActive)
        {
            Debug.LogWarning("Cannot advance: No active dialogue");
            return;
        }

        // Ignore Advance() calls when not top state (pause overlay is active)
        if (!isTopState)
        {
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
        if (currentNode.choices != null && currentNode.choices.Count > 0)
        {
            return;
        }

        // Move to next node
        if (!string.IsNullOrEmpty(currentNode.nextNodeId))
        {
            currentNodeId = currentNode.nextNodeId;
            ProcessCurrentNode();
        }
        else
        {
            // No next node, end dialogue
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
        if (choiceUIController != null)
        {
            choiceUIController.ClearChoices();
        }

        // Clear dialogue history for next node
        if (dialogueUIController != null)
        {
            dialogueUIController.ClearHistory();
        }

        // Execute choice commands
        if (selectedChoice.commands != null && selectedChoice.commands.Count > 0)
        {
            ExecuteCommands(selectedChoice.commands);
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

        isDialogueActive = false;
        isTopState = true;
        isPaused = false;
        currentDialogue = null;
        currentNodeId = null;

        // Hide dialogue UI
        if (dialogueUIController != null)
        {
            dialogueUIController.Hide();
        }
        if (choiceUIController != null)
        {
            choiceUIController.ClearChoices();
        }

        // Return to Explore state
        if (gameStateMachine != null)
        {
            gameStateMachine.SetState(previousState);
        }
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

        // Execute node commands
        if (node.commands != null && node.commands.Count > 0)
        {
            ExecuteCommands(node.commands);
        }

        // Clear choices first (they'll be shown after typing completes)
        if (choiceUIController != null)
        {
            choiceUIController.ClearChoices();
        }

        // Display dialogue text
        if (dialogueUIController != null)
        {
            bool hasChoices = node.choices != null && node.choices.Count > 0;
            
            if (hasChoices)
            {
                dialogueUIController.ShowLine(node.speaker, node.text, () => ShowChoicesForCurrentNode());
            }
            else
            {
                dialogueUIController.ShowLine(node.speaker, node.text);
            }
        }

        // Check if this is an end node (empty speaker and text)
        if (string.IsNullOrEmpty(node.speaker) && string.IsNullOrEmpty(node.text))
        {
            // End node reached
            End();
        }
    }

    [Header("Choice Settings")]
    [SerializeField] private float choiceDelayAfterTypewriter = 0.3f; // Delay before showing choices (seconds)

    /// <summary>
    /// Show choices for the current dialogue node.
    /// Called after typewriter completes. Includes a small delay.
    /// </summary>
    private void ShowChoicesForCurrentNode()
    {
        DialogueNode node = GetCurrentNode();
        if (node == null) return;

        if (node.choices != null && node.choices.Count > 0)
        {
            // Start coroutine to show choices after delay
            StartCoroutine(ShowChoicesWithDelay(node.choices));
        }
    }

    /// <summary>
    /// Coroutine to show choices after a short delay.
    /// </summary>
    private IEnumerator ShowChoicesWithDelay(List<DialogueChoice> choices)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(choiceDelayAfterTypewriter);

        if (choiceUIController != null)
        {
            List<string> choiceTexts = new List<string>();
            foreach (var choice in choices)
            {
                choiceTexts.Add(choice.text);
            }
                choiceUIController.ShowChoices(choiceTexts);
        }
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
    /// Execute a list of dialogue commands (flag, sfx, world, etc.).
    /// </summary>
    private void ExecuteCommands(List<DialogueCommand> commands)
    {
        if (commands == null)
        {
            return;
        }

        foreach (var command in commands)
        {
            ExecuteCommand(command);
        }
    }

    /// <summary>
    /// Execute a single dialogue command.
    /// </summary>
    private void ExecuteCommand(DialogueCommand command)
    {
        if (command == null || string.IsNullOrEmpty(command.command))
        {
            return;
        }

        string cmd = command.command.ToLower();
        List<string> args = command.args ?? new List<string>();

        switch (cmd)
        {
            case "flag":
            case "setflag":
                HandleFlagCommand(args);
                break;

            case "sfx":
            case "sound":
            case "playsound":
                HandleSoundCommand(args);
                break;

            case "world":
            case "worldmode":
                HandleWorldCommand(args);
                break;

            case "log":
            case "print":
            case "debug":
                HandleLogCommand(args);
                break;

            default:
                Debug.LogWarning($"Unknown dialogue command: {command.command}");
                break;
        }
    }

    /// <summary>
    /// Handle log commands: ["message"] - prints message to console
    /// Useful for debugging and testing command execution.
    /// </summary>
    private void HandleLogCommand(List<string> args)
    {
        if (args == null || args.Count < 1)
        {
            Debug.LogWarning("Log command requires at least 1 argument (message)");
            return;
        }

        string message = string.Join(" ", args);
        Debug.Log($"<color=cyan>[DialogueCommand]</color> {message}");
    }

    /// <summary>
    /// Handle flag commands: ["set", "flagName", "value"] or ["get", "flagName"]
    /// </summary>
    private void HandleFlagCommand(List<string> args)
    {
        if (args == null || args.Count < 2)
        {
            Debug.LogWarning("Flag command requires at least 2 arguments");
            return;
        }

        string operation = args[0].ToLower();
        string flagName = args[1];

        if (flagManager == null)
        {
            Debug.LogWarning("FlagManager not found");
            return;
        }

        switch (operation)
        {
            case "set":
                if (args.Count >= 3)
                {
                    string value = args[2];
                    flagManager.SetFlag(flagName, value);
                    Debug.Log($"<color=green>[Flag]</color> Set {flagName} = {value}");
                }
                break;

            case "get":
                object flagValue = flagManager.GetFlag(flagName);
                Debug.Log($"<color=green>[Flag]</color> Get {flagName} = {flagValue}");
                break;

            default:
                Debug.LogWarning($"Unknown flag operation: {operation}");
                break;
        }
    }

    /// <summary>
    /// Handle sound commands: ["play", "soundName"] or ["stop", "soundName"]
    /// </summary>
    private void HandleSoundCommand(List<string> args)
    {
        if (args == null || args.Count < 2)
        {
            Debug.LogWarning("Sound command requires at least 2 arguments");
            return;
        }

        string operation = args[0].ToLower();
        string soundName = args[1];

        // TODO: Implement sound playing logic
        Debug.Log($"Sound command: {operation} {soundName}");
    }

    /// <summary>
    /// Handle world mode commands: ["set", "Reality"] or ["set", "Dream"]
    /// </summary>
    private void HandleWorldCommand(List<string> args)
    {
        if (args == null || args.Count < 2)
        {
            Debug.LogWarning("World command requires at least 2 arguments");
            return;
        }

        string operation = args[0].ToLower();
        string modeName = args[1];

        if (operation == "set")
        {
            // Parse world mode
            if (System.Enum.TryParse<WorldMode>(modeName, true, out WorldMode mode))
            {
                // Publish WorldModeChanged event
                // if (eventBus != null)
                // {
                //     eventBus.Publish(new WorldModeChanged(mode));
                // }
                Debug.Log($"World mode changed to: {mode}");
            }
            else
            {
                Debug.LogWarning($"Invalid world mode: {modeName}");
            }
        }
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
        if (dialogueUIController != null && dialogueUIController.IsTyping())
        {
            dialogueUIController.SkipTypewriter();
        }
        else
        {
            // Only advance if there are no choices (choices require explicit selection)
            DialogueNode currentNode = GetCurrentNode();
            bool hasChoices = currentNode != null && currentNode.choices != null && currentNode.choices.Count > 0;
            
            if (!hasChoices)
            {
                // If not typing and no choices, advance to next node
                Advance();
            }
            // If there are choices, do nothing - wait for user to select a choice
        }
    }
}
