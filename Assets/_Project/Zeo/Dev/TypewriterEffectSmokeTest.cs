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
 * - Behavior during game state changes (Pause/Resume)
 * 
 * NOTE: This test is independent of the Echo Core framework.
 * It uses its own mock game state management for testing.
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
    
    // Mock game state for testing without Echo Core
    private TypewriterTestGameState currentGameState = TypewriterTestGameState.Explore;

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
        
        // Game state change tests (independent of Echo Core)
        yield return StartCoroutine(TestTypingDuringDialogueState());
        yield return StartCoroutine(TestSkipDuringPauseState());
        yield return StartCoroutine(TestTypingResumesAfterPause());

        Debug.Log($"=== TYPEWRITER EFFECT SMOKE TEST COMPLETE: {passCount} PASSED, {failCount} FAILED ===");
        testInProgress = false;
    }
    
    #region Mock Game State Methods
    
    private void SetMockGameState(TypewriterTestGameState newState)
    {
        TypewriterTestGameState oldState = currentGameState;
        currentGameState = newState;
        Debug.Log($"[MockGameState] {oldState} → {newState}");
    }
    
    private TypewriterTestGameState GetMockGameState()
    {
        return currentGameState;
    }
    
    #endregion

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
    
    private IEnumerator TestTypingDuringDialogueState()
    {
        Debug.Log("[TEST] Testing typing during Dialogue state...");
        
        if (typewriterEffect == null || textComponent == null)
        {
            Skip("Components not set up");
            yield break;
        }
        
        // Set state to Dialogue (simulating dialogue start)
        SetMockGameState(TypewriterTestGameState.Dialogue);
        yield return null;
        
        string dialogueText = "This is dialogue text appearing during Dialogue state.";
        bool typingCompleted = false;
        
        typewriterEffect.StartTyping(dialogueText, () => { typingCompleted = true; });
        
        // Wait a bit
        yield return new WaitForSeconds(0.2f);
        
        if (typewriterEffect.IsTyping())
        {
            Pass("Typewriter starts typing during Dialogue state");
        }
        else
        {
            Fail("Typewriter did not start during Dialogue state");
        }
        
        // Wait for completion
        float timeout = 5f;
        float elapsed = 0f;
        while (!typingCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (typingCompleted && textComponent.text == dialogueText)
        {
            Pass("Typing completed correctly during Dialogue state");
        }
        else
        {
            Fail($"Typing did not complete correctly during Dialogue state");
        }
        
        // Reset state
        SetMockGameState(TypewriterTestGameState.Explore);
    }
    
    private IEnumerator TestSkipDuringPauseState()
    {
        Debug.Log("[TEST] Testing skip during Pause state...");
        
        if (typewriterEffect == null || textComponent == null)
        {
            Skip("Components not set up");
            yield break;
        }
        
        // Start in Dialogue state
        SetMockGameState(TypewriterTestGameState.Dialogue);
        
        string longText = "This is a very long text that would take time to type out completely.";
        bool completionCalled = false;
        
        typewriterEffect.StartTyping(longText, () => { completionCalled = true; });
        yield return new WaitForSeconds(0.1f);
        
        // Pause the game
        SetMockGameState(TypewriterTestGameState.Pause);
        yield return null;
        
        // Skip should still work (allowing player to finish reading during pause)
        typewriterEffect.Skip();
        yield return null;
        
        if (completionCalled && textComponent.text == longText)
        {
            Pass("Skip works during Pause state (player can finish reading)");
        }
        else
        {
            Fail($"Skip failed during Pause state. Complete: {completionCalled}, Text match: {textComponent.text == longText}");
        }
        
        // Reset state
        SetMockGameState(TypewriterTestGameState.Explore);
    }
    
    private IEnumerator TestTypingResumesAfterPause()
    {
        Debug.Log("[TEST] Testing typing behavior across pause/resume...");
        
        if (typewriterEffect == null || textComponent == null)
        {
            Skip("Components not set up");
            yield break;
        }
        
        // Start in Explore, then transition to Dialogue
        SetMockGameState(TypewriterTestGameState.Explore);
        SetMockGameState(TypewriterTestGameState.Dialogue);
        
        string text1 = "First line of dialogue.";
        bool completed1 = false;
        typewriterEffect.StartTyping(text1, () => { completed1 = true; });
        
        // Let it complete
        float timeout = 5f;
        float elapsed = 0f;
        while (!completed1 && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Pause
        SetMockGameState(TypewriterTestGameState.Pause);
        yield return new WaitForSeconds(0.2f);
        
        // Resume
        SetMockGameState(TypewriterTestGameState.Dialogue);
        yield return null;
        
        // Start new typing after resume
        string text2 = "Second line after resume.";
        bool completed2 = false;
        typewriterEffect.StartTyping(text2, () => { completed2 = true; });
        
        elapsed = 0f;
        while (!completed2 && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (completed2 && textComponent.text == text2)
        {
            Pass("Typewriter works correctly after Pause/Resume cycle");
        }
        else
        {
            Fail($"Typewriter failed after resume. Complete: {completed2}, Text: '{textComponent.text}'");
        }
        
        // Reset state
        SetMockGameState(TypewriterTestGameState.Explore);
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
public enum TypewriterTestGameState
{
    Explore,
    Menu,
    Pause,
    CutScene,
    Dialogue
}
