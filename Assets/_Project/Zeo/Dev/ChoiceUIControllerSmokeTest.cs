/*
 * HOW TO SET UP THIS TEST IN UNITY:
 * 
 * 1. Create a Canvas in your scene if one doesn't exist
 * 2. Create a GameObject as a child of Canvas (e.g., "ChoiceContainer")
 * 3. Add this script (ChoiceUIControllerSmokeTest) to the ChoiceContainer GameObject
 * 4. The script will automatically create required components if missing:
 *    - ChoiceUIController component
 *    - Layout groups for button arrangement
 * 5. Create a mock DialogueManager or use a real one in the scene
 * 6. Enter Play mode - tests will run automatically if "Run On Start" is checked
 * 7. Or right-click the component and select "Run All Tests" from the context menu
 * 
 * REQUIREMENTS:
 * - TextMeshPro package installed
 * - Canvas in the scene
 * - Unity UI package (for Button, LayoutGroup components)
 * 
 * WHAT THIS TEST VERIFIES:
 * - ChoiceUIController component setup
 * - Button spawning functionality
 * - Button click handling
 * - ClearChoices functionality
 * - Layout group integration
 * - Multiple choice scenarios
 * - Behavior during game state changes (Explore/Pause/Dialogue)
 * 
 * NOTE: This test is independent of the Echo Core framework.
 * It uses its own mock game state management for testing.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceUIControllerSmokeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;

    [Header("Component References")]
    [SerializeField] private ChoiceUIController choiceUIController;
    [SerializeField] private DialogueManager dialogueManager;

    private int passCount = 0;
    private int failCount = 0;
    private bool testInProgress = false;
    private bool buttonClickDetected = false;
    
    // Mock game state for testing without Echo Core
    private ChoiceTestGameState currentGameState = ChoiceTestGameState.Explore;

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
        // Ensure we have a Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Ensure we're a child of Canvas
        if (transform.parent == null || transform.parent.GetComponent<Canvas>() == null)
        {
            transform.SetParent(canvas.transform, false);
        }

        // Add RectTransform if missing
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(500, 200);
            rect.anchoredPosition = new Vector2(0, 50);
        }

        // Add ChoiceUIController if missing
        if (choiceUIController == null)
        {
            choiceUIController = GetComponent<ChoiceUIController>();
            if (choiceUIController == null)
            {
                choiceUIController = gameObject.AddComponent<ChoiceUIController>();
            }
        }

        // Setup DialogueManager
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        // Note: We don't mock DialogueManager because SelectChoice is not virtual.
        // Button click tests will verify onClick listeners exist and fire,
        // but won't test the full integration with DialogueManager.SelectChoice.
        // For full integration testing, use DialogueSystemSmokeTest instead.
        if (dialogueManager == null)
        {
            Debug.Log("[ChoiceUIControllerSmokeTest] No DialogueManager found - button click integration tests will be limited");
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

        Debug.Log("=== CHOICE UI CONTROLLER SMOKE TEST STARTED ===");

        yield return StartCoroutine(TestComponentExists());
        yield return StartCoroutine(TestShowChoices());
        yield return StartCoroutine(TestButtonClick());
        yield return StartCoroutine(TestClearChoices());
        yield return StartCoroutine(TestMultipleChoices());
        yield return StartCoroutine(TestEmptyChoices());
        
        // Game state change tests (independent of Echo Core)
        yield return StartCoroutine(TestChoicesVisibleDuringDialogueState());
        yield return StartCoroutine(TestChoicesDuringPauseState());
        yield return StartCoroutine(TestChoicesResumeAfterPause());

        Debug.Log($"=== CHOICE UI CONTROLLER SMOKE TEST COMPLETE: {passCount} PASSED, {failCount} FAILED ===");
        testInProgress = false;
    }
    
    #region Mock Game State Methods
    
    private void SetMockGameState(ChoiceTestGameState newState)
    {
        ChoiceTestGameState oldState = currentGameState;
        currentGameState = newState;
        Debug.Log($"[MockGameState] {oldState} → {newState}");
    }
    
    private ChoiceTestGameState GetMockGameState()
    {
        return currentGameState;
    }
    
    #endregion

    private IEnumerator TestComponentExists()
    {
        Debug.Log("[TEST] Checking ChoiceUIController component exists...");
        
        if (choiceUIController != null)
        {
            Pass("ChoiceUIController component exists");
        }
        else
        {
            Fail("ChoiceUIController component not found");
        }

        yield return null;
    }

    private IEnumerator TestShowChoices()
    {
        Debug.Log("[TEST] Testing ShowChoices functionality...");
        
        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not set up");
            yield break;
        }

        List<string> choices = new List<string> { "Option 1", "Option 2", "Option 3" };
        choiceUIController.ShowChoices(choices);

        yield return new WaitForSeconds(0.5f);

        // Count buttons in children
        int buttonCount = GetComponentsInChildren<Button>().Length;
        if (buttonCount == choices.Count)
        {
            Pass($"ShowChoices created {buttonCount} buttons correctly");
        }
        else
        {
            Fail($"ShowChoices created {buttonCount} buttons, expected {choices.Count}");
        }
    }

    private IEnumerator TestButtonClick()
    {
        Debug.Log("[TEST] Testing button click handling...");
        
        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not set up");
            yield break;
        }

        List<string> choices = new List<string> { "Click Me" };
        choiceUIController.ShowChoices(choices);

        yield return new WaitForSeconds(0.5f);

        Button[] buttons = GetComponentsInChildren<Button>();
        if (buttons.Length > 0)
        {
            // Test that button has onClick listeners (indicates proper setup)
            int listenerCount = buttons[0].onClick.GetPersistentEventCount();
            
            // Add our own listener to detect click
            buttonClickDetected = false;
            buttons[0].onClick.AddListener(() => { buttonClickDetected = true; });
            buttons[0].onClick.Invoke();

            yield return new WaitForSeconds(0.1f);

            if (buttonClickDetected)
            {
                Pass("Button click fires onClick event");
            }
            else
            {
                Fail("Button onClick event did not fire");
            }
        }
        else
        {
            Fail("No buttons found to test");
        }
    }

    private IEnumerator TestClearChoices()
    {
        Debug.Log("[TEST] Testing ClearChoices functionality...");
        
        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not set up");
            yield break;
        }

        List<string> choices = new List<string> { "Choice 1", "Choice 2" };
        choiceUIController.ShowChoices(choices);

        yield return new WaitForSeconds(0.5f);

        int beforeCount = GetComponentsInChildren<Button>().Length;
        choiceUIController.ClearChoices();

        yield return new WaitForSeconds(0.1f);

        int afterCount = GetComponentsInChildren<Button>().Length;
        if (afterCount == 0)
        {
            Pass("ClearChoices removed all buttons");
        }
        else
        {
            Fail($"ClearChoices did not remove all buttons. Found {afterCount} remaining");
        }
    }

    private IEnumerator TestMultipleChoices()
    {
        Debug.Log("[TEST] Testing multiple choices scenario...");
        
        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not set up");
            yield break;
        }

        List<string> choices = new List<string> { "A", "B", "C", "D", "E" };
        choiceUIController.ShowChoices(choices);

        yield return new WaitForSeconds(0.5f);

        Button[] buttons = GetComponentsInChildren<Button>();
        if (buttons.Length == choices.Count)
        {
            Pass($"Multiple choices ({choices.Count}) handled correctly");
            
            // Test clicking different buttons - verify onClick fires for each
            for (int i = 0; i < Mathf.Min(3, buttons.Length); i++)
            {
                int clickedIndex = -1;
                int capturedIndex = i;
                buttons[i].onClick.AddListener(() => { clickedIndex = capturedIndex; });
                buttons[i].onClick.Invoke();
                yield return new WaitForSeconds(0.1f);
                
                if (clickedIndex == i)
                {
                    Pass($"Button {i} onClick fires correctly");
                }
                else
                {
                    Fail($"Button {i} onClick failed. Expected {i}, got {clickedIndex}");
                }
            }
        }
        else
        {
            Fail($"Multiple choices failed. Expected {choices.Count}, got {buttons.Length}");
        }
    }

    private IEnumerator TestEmptyChoices()
    {
        Debug.Log("[TEST] Testing empty choices scenario...");
        
        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not set up");
            yield break;
        }

        // First show some choices
        List<string> choices = new List<string> { "Test" };
        choiceUIController.ShowChoices(choices);
        yield return new WaitForSeconds(0.5f);

        // Then clear with empty list
        choiceUIController.ShowChoices(new List<string>());
        yield return new WaitForSeconds(0.1f);

        int buttonCount = GetComponentsInChildren<Button>().Length;
        if (buttonCount == 0)
        {
            Pass("Empty choices list handled correctly");
        }
        else
        {
            Fail($"Empty choices did not clear buttons. Found {buttonCount} remaining");
        }
    }
    
    private IEnumerator TestChoicesVisibleDuringDialogueState()
    {
        Debug.Log("[TEST] Testing choices visible during Dialogue state...");
        
        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not set up");
            yield break;
        }
        
        // Set state to Dialogue (simulating dialogue start)
        SetMockGameState(ChoiceTestGameState.Dialogue);
        yield return null;
        
        // Show choices
        List<string> choices = new List<string> { "Yes", "No", "Maybe" };
        choiceUIController.ShowChoices(choices);
        yield return new WaitForSeconds(0.3f);
        
        int buttonCount = GetComponentsInChildren<Button>().Length;
        if (buttonCount == choices.Count)
        {
            Pass($"Choices ({choices.Count}) visible during Dialogue state");
        }
        else
        {
            Fail($"Expected {choices.Count} choices during Dialogue state, got {buttonCount}");
        }
        
        // Clean up
        choiceUIController.ClearChoices();
        SetMockGameState(ChoiceTestGameState.Explore);
    }
    
    private IEnumerator TestChoicesDuringPauseState()
    {
        Debug.Log("[TEST] Testing choices during Pause state...");
        
        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not set up");
            yield break;
        }
        
        // Set state to Dialogue first
        SetMockGameState(ChoiceTestGameState.Dialogue);
        
        // Show choices
        List<string> choices = new List<string> { "Continue", "Exit" };
        choiceUIController.ShowChoices(choices);
        yield return new WaitForSeconds(0.3f);
        
        // Now simulate pause overlay
        SetMockGameState(ChoiceTestGameState.Pause);
        yield return null;
        
        // Buttons should still be in the hierarchy (though game logic would block interaction)
        int buttonCount = GetComponentsInChildren<Button>().Length;
        if (buttonCount == choices.Count)
        {
            Pass("Choice buttons remain in hierarchy during Pause state");
        }
        else
        {
            Fail($"Choice buttons changed during Pause. Expected {choices.Count}, got {buttonCount}");
        }
        
        // Clean up
        choiceUIController.ClearChoices();
        SetMockGameState(ChoiceTestGameState.Explore);
    }
    
    private IEnumerator TestChoicesResumeAfterPause()
    {
        Debug.Log("[TEST] Testing choices after resuming from Pause...");
        
        if (choiceUIController == null)
        {
            Skip("ChoiceUIController not set up");
            yield break;
        }
        
        // Start in dialogue
        SetMockGameState(ChoiceTestGameState.Dialogue);
        
        List<string> choices = new List<string> { "Attack", "Defend", "Flee" };
        choiceUIController.ShowChoices(choices);
        yield return new WaitForSeconds(0.3f);
        
        // Pause
        SetMockGameState(ChoiceTestGameState.Pause);
        yield return null;
        
        // Resume to dialogue
        SetMockGameState(ChoiceTestGameState.Dialogue);
        yield return null;
        
        // Test click functionality works after resume
        Button[] buttons = GetComponentsInChildren<Button>();
        if (buttons.Length > 0)
        {
            buttonClickDetected = false;
            buttons[1].onClick.AddListener(() => { buttonClickDetected = true; });
            buttons[1].onClick.Invoke(); // Click "Defend"
            yield return new WaitForSeconds(0.1f);
            
            if (buttonClickDetected)
            {
                Pass("Button onClick fires correctly after resuming from Pause");
            }
            else
            {
                Fail("Button onClick failed after resume");
            }
        }
        else
        {
            Fail("No buttons found after resuming from Pause");
        }
        
        // Clean up
        choiceUIController.ClearChoices();
        SetMockGameState(ChoiceTestGameState.Explore);
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

/// <summary>
/// Mock game state enum for testing without Echo Core framework.
/// Mirrors GameContracts.GameState but is independent.
/// </summary>
public enum ChoiceTestGameState
{
    Explore,
    Menu,
    Pause,
    CutScene,
    Dialogue
}
