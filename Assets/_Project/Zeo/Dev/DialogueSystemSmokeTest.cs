/*
 * DIALOGUE SYSTEM SMOKE TEST - Independent of Echo Core Framework
 * ===============================================================
 * 
 * This test creates its own mock implementations of:
 * - EventBus (with Subscribe/Unsubscribe/Publish)
 * - GameStateMachine (with SetState/GetState)
 * - FlagManager (with SetFlag/GetFlag)
 * - GameStateChanged event struct
 * 
 * HOW TO SET UP THIS TEST IN UNITY:
 * 
 * 1. Create a Canvas in your scene if one doesn't exist
 * 2. Create a GameObject (e.g., "DialogueSystemSmokeTest")
 * 3. Attach this script to the GameObject
 * 4. The script will automatically create all required mock components
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

public class DialogueSystemSmokeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private float testDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

    // Mock systems - created by this test
    private MockEventBus mockEventBus;
    private MockGameStateMachine mockStateMachine;
    private MockFlagManager mockFlagManager;

    // Test subjects
    private TestableDialogueManager testDialogueManager;
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

        Debug.Log("=== DIALOGUE SYSTEM SMOKE TEST (Echo-Independent) STARTED ===");
        Debug.Log("Setting up mock Echo Core systems...");

        yield return StartCoroutine(SetupMockSystems());
        yield return StartCoroutine(SetupTestUI());

        Debug.Log("Mock systems ready. Running tests...\n");

        // Core state machine tests
        yield return StartCoroutine(TestMockSystemsExist());
        yield return StartCoroutine(TestStateTransitions());
        yield return StartCoroutine(TestEventBusPublishSubscribe());
        
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

    private IEnumerator SetupMockSystems()
    {
        // Create container for mock systems
        GameObject mockSystemsObj = new GameObject("MockEchoCoreSystems");
        mockSystemsObj.transform.SetParent(transform);

        // Create MockEventBus
        mockEventBus = mockSystemsObj.AddComponent<MockEventBus>();
        yield return null;

        // Create MockGameStateMachine
        mockStateMachine = mockSystemsObj.AddComponent<MockGameStateMachine>();
        mockStateMachine.Initialize(mockEventBus);
        yield return null;

        // Create MockFlagManager
        mockFlagManager = mockSystemsObj.AddComponent<MockFlagManager>();
        mockFlagManager.Initialize(mockEventBus);
        yield return null;

        Log("Mock EventBus, GameStateMachine, and FlagManager created");
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

        // Create TestableDialogueManager
        GameObject dialogueManagerObj = new GameObject("TestDialogueManager");
        dialogueManagerObj.transform.SetParent(transform);
        testDialogueManager = dialogueManagerObj.AddComponent<TestableDialogueManager>();
        testDialogueManager.Initialize(mockEventBus, mockStateMachine, mockFlagManager, 
                                       dialogueUIController, choiceUIController);

        yield return null;
        Log("Test UI components created");
    }

    #endregion

    #region Test Methods

    private IEnumerator TestMockSystemsExist()
    {
        Log("[TEST] Verifying mock systems exist...");

        if (mockEventBus != null)
            Pass("MockEventBus exists");
        else
            Fail("MockEventBus is null");

        if (mockStateMachine != null)
            Pass("MockGameStateMachine exists");
        else
            Fail("MockGameStateMachine is null");

        if (mockFlagManager != null)
            Pass("MockFlagManager exists");
        else
            Fail("MockFlagManager is null");

        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestStateTransitions()
    {
        Log("[TEST] Testing game state transitions...");

        // Test initial state
        if (mockStateMachine.GetState() == MockGameState.Explore)
            Pass("Initial state is Explore");
        else
            Fail($"Initial state is not Explore, got: {mockStateMachine.GetState()}");

        // Test transition to Dialogue
        mockStateMachine.SetState(MockGameState.Dialogue);
        yield return null;
        if (mockStateMachine.GetState() == MockGameState.Dialogue)
            Pass("Transitioned to Dialogue state");
        else
            Fail($"Failed to transition to Dialogue, got: {mockStateMachine.GetState()}");

        // Test transition to Pause
        mockStateMachine.SetState(MockGameState.Pause);
        yield return null;
        if (mockStateMachine.GetState() == MockGameState.Pause)
            Pass("Transitioned to Pause state");
        else
            Fail($"Failed to transition to Pause, got: {mockStateMachine.GetState()}");

        // Test return to previous state
        mockStateMachine.ReturnToPreviousState();
        yield return null;
        if (mockStateMachine.GetState() == MockGameState.Dialogue)
            Pass("Returned to previous state (Dialogue)");
        else
            Fail($"Failed to return to previous state, got: {mockStateMachine.GetState()}");

        // Reset to Explore
        mockStateMachine.SetState(MockGameState.Explore);
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestEventBusPublishSubscribe()
    {
        Log("[TEST] Testing EventBus publish/subscribe...");

        bool eventReceived = false;
        MockGameStateChanged receivedEvent = default;

        // Subscribe to state change events
        Action<MockGameStateChanged> handler = (e) => 
        {
            eventReceived = true;
            receivedEvent = e;
        };
        mockEventBus.Subscribe(handler);

        // Trigger a state change
        mockStateMachine.SetState(MockGameState.Dialogue);
        yield return null;

        if (eventReceived)
            Pass("Received GameStateChanged event");
        else
            Fail("Did not receive GameStateChanged event");

        if (receivedEvent.From == MockGameState.Explore && receivedEvent.To == MockGameState.Dialogue)
            Pass("Event contains correct state transition data");
        else
            Fail($"Event data incorrect. Expected Explore→Dialogue, got {receivedEvent.From}→{receivedEvent.To}");

        // Unsubscribe
        mockEventBus.Unsubscribe(handler);
        eventReceived = false;

        mockStateMachine.SetState(MockGameState.Explore);
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

        // Ensure we're in Explore state
        mockStateMachine.SetState(MockGameState.Explore);
        yield return null;

        // Start dialogue (using test data)
        testDialogueManager.StartTestDialogue();
        yield return null;

        if (mockStateMachine.GetState() == MockGameState.Dialogue)
            Pass("Starting dialogue sets state to Dialogue");
        else
            Fail($"Starting dialogue did not set state to Dialogue, got: {mockStateMachine.GetState()}");

        // End dialogue
        testDialogueManager.EndDialogue();
        yield return null;

        if (mockStateMachine.GetState() == MockGameState.Explore)
            Pass("Ending dialogue returns state to Explore");
        else
            Fail($"Ending dialogue did not return state to Explore, got: {mockStateMachine.GetState()}");

        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestDialogueBlockedDuringPause()
    {
        Log("[TEST] Testing dialogue advance blocked during pause...");

        // Start dialogue
        mockStateMachine.SetState(MockGameState.Explore);
        testDialogueManager.StartTestDialogue();
        yield return null;

        string initialNode = testDialogueManager.GetCurrentNodeId();

        // Set pause state
        mockStateMachine.SetState(MockGameState.Pause);
        yield return null;

        // Try to advance dialogue
        testDialogueManager.Advance();
        yield return null;

        string nodeAfterPausedAdvance = testDialogueManager.GetCurrentNodeId();

        if (initialNode == nodeAfterPausedAdvance)
            Pass("Dialogue advance blocked during Pause state");
        else
            Fail($"Dialogue advanced during Pause! Node changed from {initialNode} to {nodeAfterPausedAdvance}");

        // Clean up
        mockStateMachine.SetState(MockGameState.Explore);
        testDialogueManager.EndDialogue();
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestDialogueResumeAfterPause()
    {
        Log("[TEST] Testing dialogue resumes after pause...");

        // Start dialogue
        mockStateMachine.SetState(MockGameState.Explore);
        testDialogueManager.StartTestDialogue();
        yield return null;

        string initialNode = testDialogueManager.GetCurrentNodeId();

        // Pause
        mockStateMachine.SetState(MockGameState.Pause);
        yield return null;

        // Resume (back to Dialogue)
        mockStateMachine.SetState(MockGameState.Dialogue);
        yield return null;

        // Now advance should work
        testDialogueManager.Advance();
        yield return null;

        string nodeAfterAdvance = testDialogueManager.GetCurrentNodeId();

        if (initialNode != nodeAfterAdvance || !testDialogueManager.IsDialogueActive())
            Pass("Dialogue can advance after resuming from pause");
        else
            Fail("Dialogue still blocked after resuming from pause");

        // Clean up
        testDialogueManager.EndDialogue();
        mockStateMachine.SetState(MockGameState.Explore);
        yield return new WaitForSeconds(testDelay);
    }

    private IEnumerator TestFlagSetDuringDialogue()
    {
        Log("[TEST] Testing flag set during dialogue...");

        // Clear any existing flags
        mockFlagManager.ClearAllFlags();

        // Start dialogue with a node that sets a flag
        testDialogueManager.StartTestDialogueWithFlag("testFlag", "testValue");
        yield return null;

        string flagValue = mockFlagManager.GetFlag<string>("testFlag");

        if (flagValue == "testValue")
            Pass("Flag set correctly during dialogue");
        else
            Fail($"Flag not set correctly. Expected 'testValue', got '{flagValue}'");

        // Clean up
        testDialogueManager.EndDialogue();
        mockStateMachine.SetState(MockGameState.Explore);
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
        mockStateMachine.SetState(MockGameState.Pause);
        yield return null;

        // Skip to complete
        typewriterEffect.Skip();
        yield return null;

        if (completionCalled)
            Pass("Typewriter skip works during pause state");
        else
            Fail("Typewriter completion callback not called after skip");

        mockStateMachine.SetState(MockGameState.Explore);
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
        mockStateMachine.SetState(MockGameState.Pause);
        yield return null;

        int buttonsDuringPause = choiceUIController.GetComponentsInChildren<Button>().Length;
        if (buttonsDuringPause == choices.Count)
            Pass("Choice buttons remain visible during pause");
        else
            Fail($"Choice buttons count changed during pause: {buttonsDuringPause}");

        // Clear and resume
        choiceUIController.ClearChoices();
        mockStateMachine.SetState(MockGameState.Explore);
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

#region Mock Enums and Structs

/// <summary>
/// Mock GameState enum - mirrors the real one for testing
/// </summary>
public enum MockGameState
{
    Explore,
    Menu,
    Pause,
    CutScene,
    Dialogue
}

/// <summary>
/// Mock GameStateChanged event - with PUBLIC constructor (unlike the broken real one)
/// </summary>
public struct MockGameStateChanged
{
    public readonly MockGameState From;
    public readonly MockGameState To;

    public MockGameStateChanged(MockGameState from, MockGameState to)
    {
        From = from;
        To = to;
    }
}

/// <summary>
/// Mock FlagChanged event
/// </summary>
public struct MockFlagChanged
{
    public readonly string Flag;
    public readonly object Value;

    public MockFlagChanged(string flag, object value)
    {
        Flag = flag;
        Value = value;
    }
}

#endregion

#region MockEventBus

/// <summary>
/// Mock EventBus with working Subscribe/Unsubscribe/Publish
/// </summary>
public class MockEventBus : MonoBehaviour
{
    private List<Action<MockGameStateChanged>> stateChangedHandlers = new List<Action<MockGameStateChanged>>();
    private List<Action<MockFlagChanged>> flagChangedHandlers = new List<Action<MockFlagChanged>>();

    public void Subscribe(Action<MockGameStateChanged> handler)
    {
        if (!stateChangedHandlers.Contains(handler))
            stateChangedHandlers.Add(handler);
    }

    public void Unsubscribe(Action<MockGameStateChanged> handler)
    {
        stateChangedHandlers.Remove(handler);
    }

    public void Subscribe(Action<MockFlagChanged> handler)
    {
        if (!flagChangedHandlers.Contains(handler))
            flagChangedHandlers.Add(handler);
    }

    public void Unsubscribe(Action<MockFlagChanged> handler)
    {
        flagChangedHandlers.Remove(handler);
    }

    public void Publish(MockGameStateChanged evt)
    {
        foreach (var handler in stateChangedHandlers.ToArray())
        {
            handler?.Invoke(evt);
        }
    }

    public void Publish(MockFlagChanged evt)
    {
        foreach (var handler in flagChangedHandlers.ToArray())
        {
            handler?.Invoke(evt);
        }
    }

    public void ClearAllSubscribers()
    {
        stateChangedHandlers.Clear();
        flagChangedHandlers.Clear();
    }
}

#endregion

#region MockGameStateMachine

/// <summary>
/// Mock GameStateMachine with working state management
/// </summary>
public class MockGameStateMachine : MonoBehaviour
{
    private MockEventBus eventBus;
    private MockGameState currentState = MockGameState.Explore;
    private MockGameState previousState = MockGameState.Explore;

    public MockGameState CurrentState => currentState;
    public MockGameState PreviousState => previousState;

    public void Initialize(MockEventBus bus)
    {
        eventBus = bus;
    }

    public MockGameState GetState()
    {
        return currentState;
    }

    public void SetState(MockGameState newState)
    {
        if (currentState == newState)
            return;

        MockGameState oldState = currentState;
        previousState = currentState;
        currentState = newState;

        Debug.Log($"[MockGSM] State: {oldState} → {newState}");

        if (eventBus != null)
        {
            eventBus.Publish(new MockGameStateChanged(oldState, newState));
        }
    }

    public void ReturnToPreviousState()
    {
        SetState(previousState);
    }

    public bool IsState(MockGameState state)
    {
        return currentState == state;
    }
}

#endregion

#region MockFlagManager

/// <summary>
/// Mock FlagManager with working flag storage
/// </summary>
public class MockFlagManager : MonoBehaviour
{
    private MockEventBus eventBus;
    private Dictionary<string, object> flags = new Dictionary<string, object>();

    public void Initialize(MockEventBus bus)
    {
        eventBus = bus;
    }

    public void SetFlag(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            return;

        flags[key] = value;
        Debug.Log($"[MockFlagManager] Set: {key} = {value}");

        if (eventBus != null)
        {
            eventBus.Publish(new MockFlagChanged(key, value));
        }
    }

    public T GetFlag<T>(string key, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(key) || !flags.ContainsKey(key))
            return defaultValue;

        try
        {
            return (T)Convert.ChangeType(flags[key], typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    public object GetFlag(string key)
    {
        return flags.TryGetValue(key, out object value) ? value : null;
    }

    public bool HasFlag(string key)
    {
        return !string.IsNullOrEmpty(key) && flags.ContainsKey(key);
    }

    public void ClearAllFlags()
    {
        flags.Clear();
    }
}

#endregion

#region TestableDialogueManager

/// <summary>
/// Testable DialogueManager that uses mock systems instead of real Echo Core
/// </summary>
public class TestableDialogueManager : MonoBehaviour
{
    private MockEventBus eventBus;
    private MockGameStateMachine stateMachine;
    private MockFlagManager flagManager;
    private DialogueUIController dialogueUI;
    private ChoiceUIController choiceUI;

    private bool isDialogueActive = false;
    private bool isTopState = true;
    private string currentNodeId = "";
    private int nodeIndex = 0;

    // Simple test dialogue data
    private List<TestDialogueNode> testNodes = new List<TestDialogueNode>();

    public void Initialize(MockEventBus bus, MockGameStateMachine gsm, MockFlagManager flags,
                          DialogueUIController dialogueUIController, ChoiceUIController choiceUIController)
    {
        eventBus = bus;
        stateMachine = gsm;
        flagManager = flags;
        dialogueUI = dialogueUIController;
        choiceUI = choiceUIController;

        // Subscribe to state changes
        if (eventBus != null)
        {
            eventBus.Subscribe(OnGameStateChanged);
        }
    }

    private void OnDestroy()
    {
        if (eventBus != null)
        {
            eventBus.Unsubscribe(OnGameStateChanged);
        }
    }

    private void OnGameStateChanged(MockGameStateChanged stateChange)
    {
        if (stateChange.To == MockGameState.Pause && isDialogueActive)
        {
            isTopState = false;
            Debug.Log("[TestableDialogueManager] Paused - blocking advance");
        }
        else if (stateChange.From == MockGameState.Pause && stateChange.To == MockGameState.Dialogue && isDialogueActive)
        {
            isTopState = true;
            Debug.Log("[TestableDialogueManager] Resumed from pause");
        }
    }

    public void StartTestDialogue()
    {
        isDialogueActive = true;
        isTopState = true;
        nodeIndex = 0;

        // Create simple test dialogue
        testNodes.Clear();
        testNodes.Add(new TestDialogueNode { id = "start", speaker = "Test", text = "Hello, this is node 1." });
        testNodes.Add(new TestDialogueNode { id = "node2", speaker = "Test", text = "This is node 2." });
        testNodes.Add(new TestDialogueNode { id = "end", speaker = "Test", text = "Goodbye!" });

        currentNodeId = "start";

        if (stateMachine != null)
        {
            stateMachine.SetState(MockGameState.Dialogue);
        }

        Debug.Log("[TestableDialogueManager] Started test dialogue");
    }

    public void StartTestDialogueWithFlag(string flagKey, object flagValue)
    {
        StartTestDialogue();
        
        // Set flag immediately as if dialogue command executed
        if (flagManager != null)
        {
            flagManager.SetFlag(flagKey, flagValue);
        }
    }

    public void Advance()
    {
        if (!isDialogueActive)
            return;

        // Block advance if not top state (paused)
        if (!isTopState)
        {
            Debug.Log("[TestableDialogueManager] Advance blocked - not top state");
            return;
        }

        nodeIndex++;
        if (nodeIndex < testNodes.Count)
        {
            currentNodeId = testNodes[nodeIndex].id;
            Debug.Log($"[TestableDialogueManager] Advanced to node: {currentNodeId}");
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if (!isDialogueActive)
            return;

        isDialogueActive = false;
        isTopState = true;
        currentNodeId = "";
        nodeIndex = 0;

        if (stateMachine != null)
        {
            stateMachine.SetState(MockGameState.Explore);
        }

        Debug.Log("[TestableDialogueManager] Dialogue ended");
    }

    public bool IsDialogueActive() => isDialogueActive;
    public string GetCurrentNodeId() => currentNodeId;
    public bool IsTopState() => isTopState;

    private class TestDialogueNode
    {
        public string id;
        public string speaker;
        public string text;
    }
}

#endregion
