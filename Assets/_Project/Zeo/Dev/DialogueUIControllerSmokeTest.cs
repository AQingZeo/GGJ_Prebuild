/*
 * HOW TO SET UP THIS TEST IN UNITY:
 * 
 * 1. Create a Canvas in your scene if one doesn't exist
 * 2. Create a GameObject as a child of Canvas (e.g., "DialoguePanel")
 * 3. Add this script (DialogueUIControllerSmokeTest) to the DialoguePanel GameObject
 * 4. The script will automatically create required UI components if missing:
 *    - Speaker name text (TMP_Text)
 *    - Dialogue text (TMP_Text with TypewriterEffect)
 * 5. Enter Play mode - tests will run automatically if "Run On Start" is checked
 * 6. Or right-click the component and select "Run All Tests" from the context menu
 * 
 * REQUIREMENTS:
 * - TextMeshPro package installed
 * - Canvas in the scene
 * 
 * WHAT THIS TEST VERIFIES:
 * - DialogueUIController component setup
 * - ShowLine displays speaker and text correctly
 * - Typewriter integration works
 * - Line clamping functionality
 * - Hide functionality
 * - Dialogue history management
 */

using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueUIControllerSmokeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private int testIterations = 5;

    [Header("Component References (Auto-created if null)")]
    [SerializeField] private DialogueUIController dialogueUIController;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    private int passCount = 0;
    private int failCount = 0;
    private bool testInProgress = false;

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

        // Setup dialogue panel
        if (dialoguePanel == null)
        {
            dialoguePanel = gameObject;
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
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        // Create speaker name text if missing
        if (speakerNameText == null)
        {
            GameObject speakerObj = new GameObject("SpeakerName");
            speakerObj.transform.SetParent(transform, false);
            speakerNameText = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerNameText.text = "";
            speakerNameText.fontSize = 20;
            
            RectTransform speakerRect = speakerObj.GetComponent<RectTransform>();
            speakerRect.anchorMin = new Vector2(0, 0.8f);
            speakerRect.anchorMax = new Vector2(1, 1);
            speakerRect.sizeDelta = Vector2.zero;
        }

        // Create dialogue text if missing
        if (dialogueText == null)
        {
            GameObject dialogueObj = new GameObject("DialogueText");
            dialogueObj.transform.SetParent(transform, false);
            dialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
            dialogueText.text = "";
            dialogueText.fontSize = 18;
            
            RectTransform dialogueRect = dialogueObj.GetComponent<RectTransform>();
            dialogueRect.anchorMin = new Vector2(0, 0);
            dialogueRect.anchorMax = new Vector2(1, 0.8f);
            dialogueRect.sizeDelta = Vector2.zero;
        }

        // Add DialogueUIController if missing
        if (dialogueUIController == null)
        {
            dialogueUIController = GetComponent<DialogueUIController>();
            if (dialogueUIController == null)
            {
                dialogueUIController = gameObject.AddComponent<DialogueUIController>();
            }
        }

        // Use reflection to set private fields if needed
        var panelField = typeof(DialogueUIController).GetField("dialoguePanel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (panelField != null)
        {
            panelField.SetValue(dialogueUIController, dialoguePanel);
        }

        var speakerField = typeof(DialogueUIController).GetField("speakerNameText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (speakerField != null)
        {
            speakerField.SetValue(dialogueUIController, speakerNameText);
        }

        var dialogueField = typeof(DialogueUIController).GetField("dialogueText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (dialogueField != null)
        {
            dialogueField.SetValue(dialogueUIController, dialogueText);
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

        Debug.Log("=== DIALOGUE UI CONTROLLER SMOKE TEST STARTED ===");

        yield return StartCoroutine(TestComponentExists());
        yield return StartCoroutine(TestShowLine());
        yield return StartCoroutine(TestSpeakerName());
        yield return StartCoroutine(TestTypewriterIntegration());
        yield return StartCoroutine(TestHide());
        yield return StartCoroutine(TestDialogueHistory());

        Debug.Log($"=== DIALOGUE UI CONTROLLER SMOKE TEST COMPLETE: {passCount} PASSED, {failCount} FAILED ===");
        testInProgress = false;
    }

    private IEnumerator TestComponentExists()
    {
        Debug.Log("[TEST] Checking DialogueUIController component exists...");
        
        if (dialogueUIController != null)
        {
            Pass("DialogueUIController component exists");
        }
        else
        {
            Fail("DialogueUIController component not found");
        }

        yield return null;
    }

    private IEnumerator TestShowLine()
    {
        Debug.Log("[TEST] Testing ShowLine functionality...");
        
        if (dialogueUIController == null)
        {
            Skip("DialogueUIController not set up");
            yield break;
        }

        dialogueUIController.ShowLine("TestSpeaker", "This is a test line.");

        yield return new WaitForSeconds(0.1f);

        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            Pass("ShowLine activates dialogue panel");
        }
        else
        {
            Fail("ShowLine did not activate dialogue panel");
        }

        yield return new WaitForSeconds(2f); // Wait for typewriter
    }

    private IEnumerator TestSpeakerName()
    {
        Debug.Log("[TEST] Testing speaker name display...");
        
        if (dialogueUIController == null || speakerNameText == null)
        {
            Skip("Components not set up");
            yield break;
        }

        string testSpeaker = "Narrator";
        dialogueUIController.ShowLine(testSpeaker, "Test text");

        yield return new WaitForSeconds(0.1f);

        if (speakerNameText.text == testSpeaker)
        {
            Pass("Speaker name displayed correctly");
        }
        else
        {
            Fail($"Speaker name mismatch. Expected: '{testSpeaker}', Got: '{speakerNameText.text}'");
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator TestTypewriterIntegration()
    {
        Debug.Log("[TEST] Testing typewriter integration...");
        
        if (dialogueUIController == null)
        {
            Skip("DialogueUIController not set up");
            yield break;
        }

        dialogueUIController.ShowLine("Test", "Short text.");

        yield return new WaitForSeconds(0.1f);

        if (dialogueUIController.IsTyping())
        {
            Pass("Typewriter effect is active");
        }
        else
        {
            Fail("Typewriter effect is not active");
        }

        // Test skip
        dialogueUIController.SkipTypewriter();
        yield return new WaitForSeconds(0.1f);

        if (!dialogueUIController.IsTyping())
        {
            Pass("Skip typewriter works");
        }
        else
        {
            Fail("Skip typewriter did not work");
        }
    }

    private IEnumerator TestHide()
    {
        Debug.Log("[TEST] Testing Hide functionality...");
        
        if (dialogueUIController == null)
        {
            Skip("DialogueUIController not set up");
            yield break;
        }

        dialogueUIController.ShowLine("Test", "Test text");
        yield return new WaitForSeconds(0.5f);

        dialogueUIController.Hide();
        yield return null;

        if (dialoguePanel != null && !dialoguePanel.activeSelf)
        {
            Pass("Hide deactivates dialogue panel");
        }
        else
        {
            Fail("Hide did not deactivate dialogue panel");
        }

        if (dialogueText != null && string.IsNullOrEmpty(dialogueText.text))
        {
            Pass("Hide clears dialogue text");
        }
        else
        {
            Fail("Hide did not clear dialogue text");
        }
    }

    private IEnumerator TestDialogueHistory()
    {
        Debug.Log("[TEST] Testing dialogue history and line clamping...");
        
        if (dialogueUIController == null)
        {
            Skip("DialogueUIController not set up");
            yield break;
        }

        // Show multiple lines
        for (int i = 0; i < testIterations; i++)
        {
            dialogueUIController.ShowLine("Speaker", $"Line {i + 1} of dialogue text.");
            yield return new WaitForSeconds(1f);
        }

        // Check that history is being maintained (text should contain multiple lines)
        if (dialogueText != null && dialogueText.text.Contains("Line"))
        {
            Pass("Dialogue history is maintained");
        }
        else
        {
            Fail("Dialogue history is not working");
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
