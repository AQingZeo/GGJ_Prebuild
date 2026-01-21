/*
 * HOW TO SET UP THIS TEST IN UNITY:
 * 
 * 1. Create a GameObject in your scene (e.g., "TypewriterTest")
 * 2. Add a TextMeshProUGUI component to it (or create a child GameObject with TMP_Text)
 * 3. Attach this script (TypewriterEffectSmokeTest) to the GameObject
 * 4. The script will automatically add TypewriterEffect component if missing
 * 5. Enter Play mode - tests will run automatically if "Run On Start" is checked
 * 6. Or right-click the component and select "Run All Tests" from the context menu
 * 
 * REQUIREMENTS:
 * - TextMeshPro package installed
 * - A Canvas in the scene (for UI elements)
 * 
 * WHAT THIS TEST VERIFIES:
 * - TypewriterEffect component can be added and configured
 * - Text typing animation works correctly
 * - Skip functionality works
 * - Completion callbacks are triggered
 * - Multiple start/stop cycles work without errors
 */

using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffectSmokeTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private string testText = "This is a test of the typewriter effect. It should animate character by character.";
    [SerializeField] private float testSpeed = 30f;

    [Header("UI References (Auto-assigned if null)")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private TypewriterEffect typewriterEffect;

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
        // Find or create text component
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TMP_Text>();
            if (textComponent == null)
            {
                // Create a child GameObject with TMP_Text
                GameObject textObj = new GameObject("TestText");
                textObj.transform.SetParent(transform);
                textComponent = textObj.AddComponent<TextMeshProUGUI>();
                textComponent.text = "";
                textComponent.fontSize = 24;
            }
        }

        // Find or add TypewriterEffect
        if (typewriterEffect == null)
        {
            typewriterEffect = textComponent.GetComponent<TypewriterEffect>();
            if (typewriterEffect == null)
            {
                typewriterEffect = textComponent.gameObject.AddComponent<TypewriterEffect>();
            }
        }

        // Configure typewriter
        if (typewriterEffect != null)
        {
            var field = typeof(TypewriterEffect).GetField("charactersPerSecond", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(typewriterEffect, testSpeed);
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

        Debug.Log("=== TYPEWRITER EFFECT SMOKE TEST STARTED ===");

        yield return StartCoroutine(TestComponentExists());
        yield return StartCoroutine(TestBasicTyping());
        yield return StartCoroutine(TestSkipFunctionality());
        yield return StartCoroutine(TestCompletionCallback());
        yield return StartCoroutine(TestStopTyping());
        yield return StartCoroutine(TestMultipleStarts());

        Debug.Log($"=== TYPEWRITER EFFECT SMOKE TEST COMPLETE: {passCount} PASSED, {failCount} FAILED ===");
        testInProgress = false;
    }

    private IEnumerator TestComponentExists()
    {
        Debug.Log("[TEST] Checking TypewriterEffect component exists...");
        
        if (typewriterEffect != null)
        {
            Pass("TypewriterEffect component exists");
        }
        else
        {
            Fail("TypewriterEffect component not found");
        }

        yield return null;
    }

    private IEnumerator TestBasicTyping()
    {
        Debug.Log("[TEST] Testing basic typing animation...");
        
        if (typewriterEffect == null || textComponent == null)
        {
            Skip("Components not set up");
            yield break;
        }

        bool typingStarted = false;
        bool typingCompleted = false;

        typewriterEffect.StartTyping(testText, () => { typingCompleted = true; });
        typingStarted = typewriterEffect.IsTyping();

        if (typingStarted)
        {
            Pass("Typing animation started");
        }
        else
        {
            Fail("Typing animation did not start");
            yield break;
        }

        // Wait for typing to complete (with timeout)
        float timeout = 10f;
        float elapsed = 0f;
        while (!typingCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (typingCompleted)
        {
            Pass("Typing animation completed");
            if (textComponent.text == testText)
            {
                Pass("Final text matches expected text");
            }
            else
            {
                Fail($"Final text mismatch. Expected: '{testText}', Got: '{textComponent.text}'");
            }
        }
        else
        {
            Fail("Typing animation did not complete within timeout");
        }
    }

    private IEnumerator TestSkipFunctionality()
    {
        Debug.Log("[TEST] Testing skip functionality...");
        
        if (typewriterEffect == null || textComponent == null)
        {
            Skip("Components not set up");
            yield break;
        }

        // Start typing
        typewriterEffect.StartTyping(testText);
        yield return new WaitForSeconds(0.1f); // Let it type a bit

        // Skip
        typewriterEffect.Skip();

        yield return null; // Wait one frame

        if (!typewriterEffect.IsTyping())
        {
            Pass("Skip stopped typing animation");
        }
        else
        {
            Fail("Skip did not stop typing animation");
        }

        if (textComponent.text == testText)
        {
            Pass("Skip showed full text immediately");
        }
        else
        {
            Fail($"Skip text mismatch. Expected: '{testText}', Got: '{textComponent.text}'");
        }
    }

    private IEnumerator TestCompletionCallback()
    {
        Debug.Log("[TEST] Testing completion callback...");
        
        if (typewriterEffect == null)
        {
            Skip("Components not set up");
            yield break;
        }

        bool callbackFired = false;
        typewriterEffect.StartTyping("Short text.", () => { callbackFired = true; });

        // Wait for completion
        float timeout = 5f;
        float elapsed = 0f;
        while (!callbackFired && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (callbackFired)
        {
            Pass("Completion callback was triggered");
        }
        else
        {
            Fail("Completion callback was not triggered");
        }
    }

    private IEnumerator TestStopTyping()
    {
        Debug.Log("[TEST] Testing stop typing...");
        
        if (typewriterEffect == null)
        {
            Skip("Components not set up");
            yield break;
        }

        typewriterEffect.StartTyping(testText);
        yield return new WaitForSeconds(0.1f);

        typewriterEffect.StopTyping();

        yield return null;

        if (!typewriterEffect.IsTyping())
        {
            Pass("StopTyping stopped the animation");
        }
        else
        {
            Fail("StopTyping did not stop the animation");
        }
    }

    private IEnumerator TestMultipleStarts()
    {
        Debug.Log("[TEST] Testing multiple start calls...");
        
        if (typewriterEffect == null)
        {
            Skip("Components not set up");
            yield break;
        }

        // Start multiple times rapidly
        typewriterEffect.StartTyping("First");
        typewriterEffect.StartTyping("Second");
        typewriterEffect.StartTyping("Third");

        yield return new WaitForSeconds(2f);

        // Should complete with "Third"
        if (textComponent.text == "Third")
        {
            Pass("Multiple starts handled correctly (last one wins)");
        }
        else
        {
            Fail($"Multiple starts failed. Expected: 'Third', Got: '{textComponent.text}'");
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
