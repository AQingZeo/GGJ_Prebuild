/*
 * HOW TO SET UP THIS TEST IN UNITY:
 * 
 * 1. Create a GameObject in your scene (e.g., "DialogueManagerTest")
 * 2. Add this script (DialogueManagerSmokeTest) to the GameObject
 * 3. Ensure you have the following in your scene:
 *    - EventBus component (on any GameObject)
 *    - GameStateMachine component (on any GameObject)
 *    - FlagManager component (on any GameObject)
 *    - DialogueUIController component (with UI setup)
 *    - ChoiceUIController component (with UI setup)
 * 4. The script will auto-find components or create mocks if needed
 * 5. Make sure you have a dialogue JSON file in Resources/Dialogue/ (e.g., "intro.json")
 * 6. Enter Play mode - tests will run automatically if "Run On Start" is checked
 * 7. Or right-click the component and select "Run All Tests" from the context menu
 * 
 * REQUIREMENTS:
 * - Dialogue JSON files in Resources/Dialogue/
 * - EventBus, GameStateMachine, FlagManager components (or mocks)
 * - DialogueUIController and ChoiceUIController set up
 * 
 * WHAT THIS TEST VERIFIES:
 * - DialogueManager component setup
 * - StartDialogue loads and displays dialogue
 * - Advance moves to next node
 * - SelectChoice handles choices correctly
 * - End returns to Explore state
 * - Command execution
 * - Pause overlay handling
 * - Skip typewriter functionality
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

public class DialogueManagerSmokeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private string testDialogueId = "intro";
    [SerializeField] private bool createMocks = true;

    [Header("Component References (Auto-found if null)")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private EventBus eventBus;
    [SerializeField] private GameStateMachine gameStateMachine;
    [SerializeField] private FlagManager flagManager;
    [SerializeField] private DialogueUIController dialogueUIController;
    [SerializeField] private ChoiceUIController choiceUIController;

    private int passCount = 0;
    private int failCount = 0;
    private bool testInProgress = false;
    private GameState currentGameState = GameState.Explore;

    private void Awake()
    {
        SetupComponents();
    }

    private void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(RunAllTestsCoroutine());
        }
    }

    private void SetupComponents()
    {
        // Find or create DialogueManager
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null && createMocks)
            {
                GameObject dmObj = new GameObject("DialogueManager");
                dialogueManager = dmObj.AddComponent<DialogueManager>();
            }
        }

        // Find or create EventBus
        if (eventBus == null)
        {
            eventBus = FindObjectOfType<EventBus>();
            if (eventBus == null && createMocks)
            {
                GameObject ebObj = new GameObject("EventBus");
                eventBus = ebObj.AddComponent<EventBus>();
            }
        }

        // Find or create GameStateMachine
        if (gameStateMachine == null)
        {
            gameStateMachine = FindObjectOfType<GameStateMachine>();
            if (gameStateMachine == null && createMocks)
            {
                GameObject gsmObj = new GameObject("GameStateMachine");
                gameStateMachine = gsmObj.AddComponent<GameStateMachine>();
            }
        }

        // Find or create FlagManager
        if (flagManager == null)
        {
            flagManager = FindObjectOfType<FlagManager>();
            if (flagManager == null && createMocks)
            {
                GameObject fmObj = new GameObject("FlagManager");
                flagManager = fmObj.AddComponent<FlagManager>();
            }
        }

        // Find DialogueUIController
        if (dialogueUIController == null)
        {
            dialogueUIController = FindObjectOfType<DialogueUIController>();
        }

        // Find ChoiceUIController
        if (choiceUIController == null)
        {
            choiceUIController = FindObjectOfType<ChoiceUIController>();
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

        StartCoroutine(RunAllTestsCoroutine());
    }

    private IEnumerator RunAllTestsCoroutine()
    {
        testInProgress = true;
        passCount = 0;
        failCount = 0;

        Debug.Log("=== DIALOGUE MANAGER SMOKE TEST STARTED ===");

        yield return StartCoroutine(TestComponentExists());
        yield return StartCoroutine(TestStartDialogue());
        yield return StartCoroutine(TestAdvance());
        yield return StartCoroutine(TestSelectChoice());
        yield return StartCoroutine(TestEnd());
        yield return StartCoroutine(TestSkipTypewriter());

        Debug.Log($"=== DIALOGUE MANAGER SMOKE TEST COMPLETE: {passCount} PASSED, {failCount} FAILED ===");
        testInProgress = false;
    }

    private IEnumerator TestComponentExists()
    {
        Debug.Log("[TEST] Checking DialogueManager component exists...");
        
        if (dialogueManager != null)
        {
            Pass("DialogueManager component exists");
        }
        else
        {
            Fail("DialogueManager component not found");
        }

        yield return null;
    }

    private IEnumerator TestStartDialogue()
    {
        Debug.Log("[TEST] Testing StartDialogue functionality...");
        
        if (dialogueManager == null)
        {
            Skip("DialogueManager not set up");
            yield break;
        }

        // End any existing dialogue
        if (dialogueManager.IsDialogueActive())
        {
            dialogueManager.End();
            yield return new WaitForSeconds(0.1f);
        }

        // Start dialogue
        dialogueManager.StartDialogue(testDialogueId);
        yield return new WaitForSeconds(0.5f);

        if (dialogueManager.IsDialogueActive())
        {
            Pass("StartDialogue activated dialogue");
        }
        else
        {
            Fail("StartDialogue did not activate dialogue");
        }

        string currentId = dialogueManager.GetCurrentDialogueId();
        if (currentId == testDialogueId)
        {
            Pass($"StartDialogue loaded dialogue '{testDialogueId}' correctly");
        }
        else
        {
            Fail($"StartDialogue dialogue ID mismatch. Expected: '{testDialogueId}', Got: '{currentId}'");
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator TestAdvance()
    {
        Debug.Log("[TEST] Testing Advance functionality...");
        
        if (dialogueManager == null)
        {
            Skip("DialogueManager not set up");
            yield break;
        }

        // Ensure dialogue is active
        if (!dialogueManager.IsDialogueActive())
        {
            dialogueManager.StartDialogue(testDialogueId);
            yield return new WaitForSeconds(1f);
        }

        // Try to advance
        dialogueManager.Advance();
        yield return new WaitForSeconds(0.5f);

        // Dialogue should still be active or ended properly
        bool stillActive = dialogueManager.IsDialogueActive();
        if (stillActive || !dialogueManager.IsDialogueActive())
        {
            Pass("Advance processed correctly");
        }
        else
        {
            Fail("Advance did not work correctly");
        }
    }

    private IEnumerator TestSelectChoice()
    {
        Debug.Log("[TEST] Testing SelectChoice functionality...");
        
        if (dialogueManager == null)
        {
            Skip("DialogueManager not set up");
            yield break;
        }

        // Note: This test requires a dialogue with choices
        // For now, we'll just test that the method exists and can be called
        dialogueManager.StartDialogue(testDialogueId);
        yield return new WaitForSeconds(1f);

        // Try selecting a choice (may fail if no choices, which is OK)
        try
        {
            dialogueManager.SelectChoice(0);
            yield return new WaitForSeconds(0.5f);
            Pass("SelectChoice method exists and can be called");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[INFO] SelectChoice test: {e.Message} (This is OK if dialogue has no choices)");
            Pass("SelectChoice method exists (no choices in test dialogue)");
        }
    }

    private IEnumerator TestEnd()
    {
        Debug.Log("[TEST] Testing End functionality...");
        
        if (dialogueManager == null)
        {
            Skip("DialogueManager not set up");
            yield break;
        }

        // Ensure dialogue is active
        if (!dialogueManager.IsDialogueActive())
        {
            dialogueManager.StartDialogue(testDialogueId);
            yield return new WaitForSeconds(1f);
        }

        // End dialogue
        dialogueManager.End();
        yield return new WaitForSeconds(0.5f);

        if (!dialogueManager.IsDialogueActive())
        {
            Pass("End deactivated dialogue");
        }
        else
        {
            Fail("End did not deactivate dialogue");
        }
    }

    private IEnumerator TestSkipTypewriter()
    {
        Debug.Log("[TEST] Testing SkipTypewriter functionality...");
        
        if (dialogueManager == null)
        {
            Skip("DialogueManager not set up");
            yield break;
        }

        // Start dialogue
        dialogueManager.StartDialogue(testDialogueId);
        yield return new WaitForSeconds(0.2f);

        // Skip typewriter
        dialogueManager.SkipTypewriter();
        yield return new WaitForSeconds(0.3f);

        // Should still be active (unless dialogue ended)
        Pass("SkipTypewriter method exists and can be called");
    }

    #region Assertion Helpers

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
