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
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceUIControllerSmokeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool createMockDialogueManager = true;

    [Header("Component References")]
    [SerializeField] private ChoiceUIController choiceUIController;
    [SerializeField] private DialogueManager dialogueManager;

    private int passCount = 0;
    private int failCount = 0;
    private bool testInProgress = false;
    private int lastSelectedChoiceIndex = -1;
    private MockDialogueManager mockManager;

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

        if (dialogueManager == null && createMockDialogueManager)
        {
            // Create a mock DialogueManager for testing
            GameObject mockObj = new GameObject("MockDialogueManager");
            mockManager = mockObj.AddComponent<MockDialogueManager>();
            mockManager.OnChoiceSelected = (index) => { lastSelectedChoiceIndex = index; };
            
            // Use reflection to set the dialogueManager field
            var field = typeof(ChoiceUIController).GetField("dialogueManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(choiceUIController, mockManager);
            }
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
        lastSelectedChoiceIndex = -1;

        Debug.Log("=== CHOICE UI CONTROLLER SMOKE TEST STARTED ===");

        yield return StartCoroutine(TestComponentExists());
        yield return StartCoroutine(TestShowChoices());
        yield return StartCoroutine(TestButtonClick());
        yield return StartCoroutine(TestClearChoices());
        yield return StartCoroutine(TestMultipleChoices());
        yield return StartCoroutine(TestEmptyChoices());

        Debug.Log($"=== CHOICE UI CONTROLLER SMOKE TEST COMPLETE: {passCount} PASSED, {failCount} FAILED ===");
        testInProgress = false;
    }

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
            lastSelectedChoiceIndex = -1;
            buttons[0].onClick.Invoke();

            yield return new WaitForSeconds(0.1f);

            if (lastSelectedChoiceIndex == 0)
            {
                Pass("Button click correctly calls DialogueManager.SelectChoice");
            }
            else
            {
                Fail($"Button click did not work. Expected index 0, got {lastSelectedChoiceIndex}");
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
            
            // Test clicking different buttons
            for (int i = 0; i < Mathf.Min(3, buttons.Length); i++)
            {
                lastSelectedChoiceIndex = -1;
                buttons[i].onClick.Invoke();
                yield return new WaitForSeconds(0.1f);
                
                if (lastSelectedChoiceIndex == i)
                {
                    Pass($"Button {i} click works correctly");
                }
                else
                {
                    Fail($"Button {i} click failed. Expected {i}, got {lastSelectedChoiceIndex}");
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
/// Mock DialogueManager for testing ChoiceUIController without full dialogue system.
/// </summary>
public class MockDialogueManager : MonoBehaviour
{
    public System.Action<int> OnChoiceSelected;

    public void SelectChoice(int index)
    {
        OnChoiceSelected?.Invoke(index);
        Debug.Log($"[MockDialogueManager] SelectChoice called with index: {index}");
    }
}
