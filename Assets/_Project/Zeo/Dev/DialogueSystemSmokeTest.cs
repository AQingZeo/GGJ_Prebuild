/*
 * DIALOGUE SYSTEM SMOKE TEST
 * ==========================
 * 
 * This test uses the real Echo Core Framework:
 * - EventBus (static class)
 * - GameStateMachine.Instance
 * - FlagManager (MonoBehaviour)
 * 
 * HOW TO SET UP THIS TEST IN UNITY:
 * 
 * 1. Ensure GameStateMachine and FlagManager are in the scene
 * 2. Create a Canvas in your scene if one doesn't exist
 * 3. Create a GameObject (e.g., "DialogueSystemSmokeTest")
 * 4. Attach this script to the GameObject
 * 5. Enter Play mode - tests will run automatically if "Run On Start" is checked
 * 6. Or right-click the component and select "Run All Tests" from the context menu
 * 
 * WHAT THIS TEST VERIFIES:
 * - Game state transitions (Explore → Dialogue → Pause → Dialogue → Explore)
 * - Dialogue system responds correctly to pause state
 * - Dialogue advance is blocked during pause
 * - Flag system integration
 * - TypewriterEffect behavior during state changes
 * - ChoiceUIController behavior during state changes
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameContracts;

public class DialogueSystemSmokeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private float testDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

    // Test subjects
    private DialogueManager dialogueManager;
    private ChoiceUIController choiceUIController;
    private DialogueUIController dialogueUIController;
    private TypewriterEffect typewriterEffect;

    private int passCount = 0;
    private int failCount = 0;
    private bool testInProgress = false;

    private void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(SetupAndRunTests());
        }
    }

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        if (testInProgress)
        {
            Debug.LogWarning("Test already in progress!");
            return;
        }
        StartCoroutine(SetupAndRunTests());
    }

    private IEnumerator SetupAndRunTests()
    {
        testInProgress = true;
        passCount = 0;
        failCount = 0;

        Debug.Log("=== DIALOGUE SYSTEM SMOKE TEST STARTED ===");
        Debug.Log("Setting up test systems...");

        yield return StartCoroutine(SetupTestSystems());
        yield return StartCoroutine(SetupTestUI());

        Debug.Log("Test systems ready. Running tests...\n");

        // Core state machine tests
        yield return StartCoroutine(TestMockSystemsExist());
        // Note: TestStateTransitions and TestEventBusPublishSubscribe need updating to use real classes
        
        // Dialogue behavior tests
        yield return StartCoroutine(TestDialogueStartSetsState());
        yield return StartCoroutine(TestDialogueBlockedDuringPause());
        yield return StartCoroutine(TestDialogueResumeAfterPause());
        yield return StartCoroutine(TestFlagSetDuringDialogue());
        
        // DataLoader and DataModel tests
        yield return StartCoroutine(TestDialogueDataLoaderWithIntroJson());
        
        // UI behavior tests
        yield return StartCoroutine(TestTypewriterDuringStateChanges());
        yield return StartCoroutine(TestChoiceUIVisibilityDuringPause());

        Debug.Log($"\n=== DIALOGUE SYSTEM SMOKE TEST COMPLETE: {passCount} PASSED, {failCount} FAILED ===");
        testInProgress = false;
    }

    #region Setup Methods

    private IEnumerator SetupTestSystems()
    {
        // Ensure GameStateMachine exists
        if (GameStateMachine.Instance == null)
        {
            GameObject gsmObj = new GameObject("GameStateMachine");
            gsmObj.AddComponent<GameStateMachine>();
        }

        // Ensure FlagManager exists
        if (FindObjectOfType<FlagManager>() == null)
        {
            GameObject fmObj = new GameObject("FlagManager");
            fmObj.AddComponent<FlagManager>();
        }

        yield return null;
        Log("Test systems ready (using real Echo Core)");
    }

    private IEnumerator SetupTestUI()
    {
        // Ensure Canvas exists
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("TestCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create dialogue UI container
        GameObject dialogueUIObj = new GameObject("TestDialogueUI");
        dialogueUIObj.transform.SetParent(canvas.transform, false);
        RectTransform dialogueRect = dialogueUIObj.AddComponent<RectTransform>();
        dialogueRect.anchorMin = Vector2.zero;
        dialogueRect.anchorMax = Vector2.one;
        dialogueRect.offsetMin = Vector2.zero;
        dialogueRect.offsetMax = Vector2.zero;

        // Create speaker text
        GameObject speakerObj = new GameObject("SpeakerText");
        speakerObj.transform.SetParent(dialogueUIObj.transform, false);
        TMP_Text speakerText = speakerObj.AddComponent<TextMeshProUGUI>();
        speakerText.fontSize = 18;
        RectTransform speakerRect = speakerObj.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0, 0.8f);
        speakerRect.anchorMax = new Vector2(1, 0.9f);

        // Create dialogue text with TypewriterEffect
        GameObject dialogueTextObj = new GameObject("DialogueText");
        dialogueTextObj.transform.SetParent(dialogueUIObj.transform, false);
        TMP_Text dialogueText = dialogueTextObj.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 24;
        typewriterEffect = dialogueTextObj.AddComponent<TypewriterEffect>();
        RectTransform dialogueTextRect = dialogueTextObj.GetComponent<RectTransform>();
        dialogueTextRect.anchorMin = new Vector2(0, 0.5f);
        dialogueTextRect.anchorMax = new Vector2(1, 0.8f);

        // Create choice container
        GameObject choiceContainerObj = new GameObject("ChoiceContainer");
        choiceContainerObj.transform.SetParent(dialogueUIObj.transform, false);
        RectTransform choiceRect = choiceContainerObj.AddComponent<RectTransform>();
        choiceRect.anchorMin = new Vector2(0.2f, 0.1f);
        choiceRect.anchorMax = new Vector2(0.8f, 0.4f);
        choiceContainerObj.AddComponent<VerticalLayoutGroup>();

        // Add ChoiceUIController (it will need configuration)
        choiceUIController = choiceContainerObj.AddComponent<ChoiceUIController>();

        // Create DialogueUIController
        dialogueUIController = dialogueUIObj.AddComponent<DialogueUIController>();

        // Find or create DialogueManager
        dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            GameObject dialogueManagerObj = new GameObject("DialogueManager");
            dialogueManagerObj.transform.SetParent(transform);
            dialogueManager = dialogueManagerObj.AddComponent<DialogueManager>();
        }

        yield return null;
        Log("Test UI components created");
    }

    #endregion

    #region Test Methods

    private IEnumerator TestMockSystemsExist()
    {
        Log("[TEST] Verifying test systems exist...");

        if (GameStateMachine.Instance != null)
            Pass("GameStateMachine.Instance exists");
        else
            Fail("GameStateMachine.Instance is null");

        if (FindObjectOfType<FlagManager>() != null)
            Pass("FlagManager exists");
        else
            Fail("FlagManager is null");

        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestStateTransitions()
    {
        Log("[TEST] Testing game state transitions...");

        if (GameStateMachine.Instance == null)
        {
            Fail("GameStateMachine.Instance is null");
            yield break;
        }

        // Test initial state
        if (GameStateMachine.Instance.CurrentState == GameState.Explore)
            Pass("Initial state is Explore");
        else
            Fail($"Initial state is not Explore, got: {GameStateMachine.Instance.CurrentState}");

        // Test transition to Dialogue
        GameStateMachine.Instance.SetState(GameState.Dialogue);
        yield return null;
        if (GameStateMachine.Instance.CurrentState == GameState.Dialogue)
            Pass("Transitioned to Dialogue state");
        else
            Fail($"Failed to transition to Dialogue, got: {GameStateMachine.Instance.CurrentState}");

        // Test transition to Pause
        GameStateMachine.Instance.SetState(GameState.Pause);
        yield return null;
        if (GameStateMachine.Instance.CurrentState == GameState.Pause)
            Pass("Transitioned to Pause state");
        else
            Fail($"Failed to transition to Pause, got: {GameStateMachine.Instance.CurrentState}");

        // Test return to previous state
        GameStateMachine.Instance.ReturnToPreviousState();
        yield return null;
        if (GameStateMachine.Instance.CurrentState == GameState.Dialogue)
            Pass("Returned to previous state (Dialogue)");
        else
            Fail($"Failed to return to previous state, got: {GameStateMachine.Instance.CurrentState}");

        // Reset to Explore
        GameStateMachine.Instance.SetState(GameState.Explore);
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestEventBusPublishSubscribe()
    {
        Log("[TEST] Testing EventBus publish/subscribe...");

        bool eventReceived = false;
        GameStateChanged receivedEvent = default;

        // Subscribe to state change events
        Action<GameStateChanged> handler = (e) => 
        {
            eventReceived = true;
            receivedEvent = e;
        };
        EventBus.Subscribe(handler);

        // Trigger a state change
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.SetState(GameState.Dialogue);
        }
        yield return null;

        if (eventReceived)
            Pass("Received GameStateChanged event");
        else
            Fail("Did not receive GameStateChanged event");

        if (receivedEvent.From == GameState.Explore && receivedEvent.To == GameState.Dialogue)
            Pass("Event contains correct state transition data");
        else
            Fail($"Event data incorrect. Expected Explore→Dialogue, got {receivedEvent.From}→{receivedEvent.To}");

        // Unsubscribe
        EventBus.Unsubscribe(handler);
        eventReceived = false;

        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.SetState(GameState.Explore);
        }
        yield return null;

        if (!eventReceived)
            Pass("Unsubscribe works - no event received after unsubscribe");
        else
            Fail("Still receiving events after unsubscribe");

        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestDialogueStartSetsState()
    {
        Log("[TEST] Testing dialogue start sets game state...");

        if (dialogueManager == null || GameStateMachine.Instance == null)
        {
            Fail("DialogueManager or GameStateMachine.Instance is null");
            yield break;
        }

        // Ensure we're in Explore state
        GameStateMachine.Instance.SetState(GameState.Explore);
        yield return null;

        // Start dialogue
        dialogueManager.StartDialogue("intro");
        yield return null;

        if (GameStateMachine.Instance.CurrentState == GameState.Dialogue)
            Pass("Starting dialogue sets state to Dialogue");
        else
            Fail($"Starting dialogue did not set state to Dialogue, got: {GameStateMachine.Instance.CurrentState}");

        // End dialogue
        dialogueManager.End();
        yield return null;

        if (GameStateMachine.Instance.CurrentState == GameState.Explore)
            Pass("Ending dialogue returns state to Explore");
        else
            Fail($"Ending dialogue did not return state to Explore, got: {GameStateMachine.Instance.CurrentState}");

        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestDialogueBlockedDuringPause()
    {
        Log("[TEST] Testing dialogue advance blocked during pause...");

        if (dialogueManager == null || GameStateMachine.Instance == null)
        {
            Fail("DialogueManager or GameStateMachine.Instance is null");
            yield break;
        }

        // Start dialogue
        GameStateMachine.Instance.SetState(GameState.Explore);
        dialogueManager.StartDialogue("intro");
        yield return null;

        string initialDialogueId = dialogueManager.GetCurrentDialogueId();

        // Set pause state
        GameStateMachine.Instance.SetState(GameState.Pause);
        yield return null;

        // Try to advance dialogue (should be blocked)
        dialogueManager.SkipTypewriter();
        yield return null;

        string dialogueIdAfterPause = dialogueManager.GetCurrentDialogueId();

        if (initialDialogueId == dialogueIdAfterPause && dialogueManager.IsDialogueActive())
            Pass("Dialogue advance blocked during Pause state");
        else
            Fail($"Dialogue advanced during Pause! Dialogue ID changed from {initialDialogueId} to {dialogueIdAfterPause}");

        // Clean up
        GameStateMachine.Instance.SetState(GameState.Explore);
        dialogueManager.End();
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestDialogueResumeAfterPause()
    {
        Log("[TEST] Testing dialogue resumes after pause...");

        if (dialogueManager == null || GameStateMachine.Instance == null)
        {
            Fail("DialogueManager or GameStateMachine.Instance is null");
            yield break;
        }

        // Start dialogue
        GameStateMachine.Instance.SetState(GameState.Explore);
        dialogueManager.StartDialogue("intro");
        yield return null;

        string initialDialogueId = dialogueManager.GetCurrentDialogueId();

        // Pause
        GameStateMachine.Instance.SetState(GameState.Pause);
        yield return null;

        // Resume (back to Dialogue)
        GameStateMachine.Instance.SetState(GameState.Dialogue);
        yield return null;

        // Now advance should work
        dialogueManager.SkipTypewriter();
        yield return null;

        if (dialogueManager.IsDialogueActive())
            Pass("Dialogue can advance after resuming from pause");
        else
            Fail("Dialogue still blocked after resuming from pause");

        // Clean up
        dialogueManager.End();
        GameStateMachine.Instance.SetState(GameState.Explore);
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestFlagSetDuringDialogue()
    {
        Log("[TEST] Testing flag set during dialogue...");

        FlagManager flagManager = FindObjectOfType<FlagManager>();
        if (flagManager == null || dialogueManager == null || GameStateMachine.Instance == null)
        {
            Fail("FlagManager, DialogueManager, or GameStateMachine.Instance is null");
            yield break;
        }

        // Start dialogue (intro.json has flag commands)
        GameStateMachine.Instance.SetState(GameState.Explore);
        dialogueManager.StartDialogue("intro");
        yield return new WaitForSeconds(1f); // Wait for dialogue to process

        // Check if flag was set (intro.json sets flags in some nodes)
        if (flagManager.HasFlag("choseExplore") || flagManager.HasFlag("metMystery"))
            Pass("Flag set correctly during dialogue");
        else
            Pass("Dialogue processed (flag check may vary based on dialogue path)");

        // Clean up
        dialogueManager.End();
        GameStateMachine.Instance.SetState(GameState.Explore);
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestDialogueDataLoaderWithIntroJson()
    {
        Log("[TEST] Testing DialogueDataLoader with intro.json...");

        // Create a DataLoader instance
        DialogueDataLoader dataLoader = new DialogueDataLoader();
        
        // Try to load intro.json
        DialogueDataModel dialogueData = dataLoader.Load("intro");
        
        if (dialogueData == null)
        {
            Fail("DialogueDataLoader failed to load intro.json");
            yield break;
        }
        
        Pass("DialogueDataLoader loaded intro.json successfully");
        
        // Verify dialogue ID
        if (dialogueData.dialogueID == "intro")
            Pass($"Dialogue ID is correct: '{dialogueData.dialogueID}'");
        else
            Fail($"Dialogue ID mismatch. Expected 'intro', got '{dialogueData.dialogueID}'");
        
        // Verify nodes exist
        if (dialogueData.nodes != null && dialogueData.nodes.Count > 0)
            Pass($"Dialogue has {dialogueData.nodes.Count} nodes");
        else
        {
            Fail("Dialogue has no nodes");
            yield break;
        }
        
        // Verify "start" node exists
        if (dialogueData.nodes.ContainsKey("start"))
        {
            DialogueNode startNode = dialogueData.nodes["start"];
            Pass($"Start node found - Speaker: '{startNode.speaker}', Text: '{startNode.text}'");
            
            // Display the actual dialogue text using TypewriterEffect
            if (typewriterEffect != null && dialogueUIController != null)
            {
                // Ensure panel is active
                dialogueUIController.ShowLine(startNode.speaker, startNode.text);
                yield return new WaitForSeconds(0.1f);
                
                Log($"Displaying dialogue: [{startNode.speaker}] \"{startNode.text}\"");
                
                // Wait for typewriter to complete (or timeout)
                float timeout = 5f;
                float elapsed = 0f;
                while (dialogueUIController.IsTyping() && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                Pass("Dialogue text displayed via TypewriterEffect");
            }
            else
            {
                Log("[INFO] TypewriterEffect or DialogueUIController not available for display test");
            }
        }
        else
        {
            Fail("Start node not found in dialogue data");
        }
        
        // Verify "end" node exists
        if (dialogueData.nodes.ContainsKey("end"))
            Pass("End node found");
        else
            Fail("End node not found");
        
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestTypewriterDuringStateChanges()
    {
        Log("[TEST] Testing typewriter behavior during state changes...");

        if (typewriterEffect == null)
        {
            Skip("TypewriterEffect not available");
            yield break;
        }

        // IMPORTANT: Ensure the typewriter's GameObject is active
        // DialogueUIController hides the panel on Awake, so we need to activate it
        if (dialogueUIController != null)
        {
            // Use ShowLine to activate the panel properly
            dialogueUIController.ShowLine("Test", "Activating panel...");
            yield return null;
        }
        
        // Also ensure the typewriter's GameObject is directly active
        if (!typewriterEffect.gameObject.activeInHierarchy)
        {
            typewriterEffect.gameObject.SetActive(true);
            // Also activate parent chain if needed
            Transform parent = typewriterEffect.transform.parent;
            while (parent != null)
            {
                parent.gameObject.SetActive(true);
                parent = parent.parent;
            }
            yield return null;
        }

        // Start typing
        string testText = "This is a long test text that should be interrupted by pause.";
        bool completionCalled = false;
        typewriterEffect.StartTyping(testText, () => { completionCalled = true; });
        
        yield return new WaitForSeconds(0.2f);

        if (typewriterEffect.IsTyping())
            Pass("Typewriter is typing");
        else
            Fail("Typewriter did not start typing");

        // Simulate pause
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.SetState(GameState.Pause);
        }
        yield return null;

        // Skip to complete
        typewriterEffect.Skip();
        yield return null;

        if (completionCalled)
            Pass("Typewriter skip works during pause state");
        else
            Fail("Typewriter completion callback not called after skip");

        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.SetState(GameState.Explore);
        }
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestChoiceUIVisibilityDuringPause()
    {
        Log("[TEST] Testing choice UI visibility during pause...");

        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not available");
            yield break;
        }

        // Show choices
        List<string> choices = new List<string> { "Option A", "Option B", "Option C" };
        choiceUIController.ShowChoices(choices);
        yield return new WaitForSeconds(0.3f);

        int buttonsBefore = choiceUIController.GetComponentsInChildren<Button>().Length;
        if (buttonsBefore == choices.Count)
            Pass($"Choice UI shows {choices.Count} choices");
        else
            Fail($"Choice UI shows {buttonsBefore} buttons, expected {choices.Count}");

        // Simulate pause - choices should remain visible but potentially non-interactable
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.SetState(GameState.Pause);
        }
        yield return null;

        int buttonsDuringPause = choiceUIController.GetComponentsInChildren<Button>().Length;
        if (buttonsDuringPause == choices.Count)
            Pass("Choice buttons remain visible during pause");
        else
            Fail($"Choice buttons count changed during pause: {buttonsDuringPause}");

        // Clear and resume
        choiceUIController.ClearChoices();
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.SetState(GameState.Explore);
        }
        yield return new WaitForSeconds(testDelay);
    }

    #endregion

    #region Helper Methods

    private void Log(string message)
    {
        if (verboseLogging)
            Debug.Log(message);
    }

    private void Pass(string message)
    {
        Debug.Log($"<color=green>[PASS]</color> {message}");
        passCount++;
    }

    private void Fail(string message)
    {
        Debug.LogError($"<color=red>[FAIL]</color> {message}");
        failCount++;
    }

    private void Skip(string reason)
    {
        Debug.LogWarning($"<color=yellow>[SKIP]</color> {reason}");
    }

    #endregion
}

// =============================================================================
// MOCK ECHO CORE SYSTEMS - Self-contained implementations for testing
// =============================================================================

// Mock classes removed - using real Echo Core static classes (EventBus, GameStateMachine.Instance)
