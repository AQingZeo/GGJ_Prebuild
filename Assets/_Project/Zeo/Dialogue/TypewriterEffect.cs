using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Typewriter effect that animates text character by character.
/// Supports skip functionality to instantly complete the animation.
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private bool useUnscaledTime = false;

    private TMP_Text textComponent;
    private Coroutine typewriterCoroutine;
    private string fullText;
    private bool isTyping = false;
    private System.Action onComplete;
    private System.Func<char, int, char> characterTransform; // Transform function: (originalChar, index) => transformedChar

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent == null)
        {
            Debug.LogError("TypewriterEffect requires a TMP_Text component!");
        }
    }

    /// <summary>
    /// Start typing the given text with typewriter effect.
    /// </summary>
    /// <param name="text">The full text to display</param>
    /// <param name="onComplete">Callback when typing is complete</param>
    /// <param name="characterTransform">Optional function to transform each character: (char, index) => char</param>
    public void StartTyping(string text, System.Action onComplete = null, System.Func<char, int, char> characterTransform = null)
    {
        if (textComponent == null)
        {
            Debug.LogError("TMP_Text component not found!");
            return;
        }

        // Stop any existing typewriter
        StopTyping();

        fullText = text;
        this.onComplete = onComplete;
        this.characterTransform = characterTransform;
        isTyping = true;

        // Start the typewriter coroutine
        typewriterCoroutine = StartCoroutine(TypewriterCoroutine());
    }

    /// <summary>
    /// Skip the typewriter effect and show the full text immediately.
    /// </summary>
    public void Skip()
    {
        if (isTyping && typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            
            // Show full text immediately with transformation applied
            if (textComponent != null && !string.IsNullOrEmpty(fullText))
            {
                if (characterTransform != null)
                {
                    string transformedText = "";
                    for (int i = 0; i < fullText.Length; i++)
                    {
                        transformedText += characterTransform(fullText[i], i);
                    }
                    textComponent.text = transformedText;
                }
                else
                {
                    textComponent.text = fullText;
                }
            }

            isTyping = false;

            // Call completion callback
            onComplete?.Invoke();
            onComplete = null;
        }
    }

    /// <summary>
    /// Stop the typewriter effect without showing the full text.
    /// </summary>
    public void StopTyping()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;
        onComplete = null;
        characterTransform = null;
    }

    /// <summary>
    /// Check if the typewriter is currently typing.
    /// </summary>
    public bool IsTyping()
    {
        return isTyping;
    }

    /// <summary>
    /// Coroutine that animates the text character by character.
    /// </summary>
    private IEnumerator TypewriterCoroutine()
    {
        if (textComponent == null || string.IsNullOrEmpty(fullText))
        {
            yield break;
        }

        textComponent.text = "";
        float delay = 1f / charactersPerSecond;

        for (int i = 0; i <= fullText.Length; i++)
        {
            // Build the displayed text with optional character transformation
            string displayedText = "";
            for (int j = 0; j < i; j++)
            {
                char originalChar = fullText[j];
                char displayChar = characterTransform != null ? characterTransform(originalChar, j) : originalChar;
                displayedText += displayChar;
            }
            
            textComponent.text = displayedText;

            // Wait for the next character
            if (useUnscaledTime)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
        }

        // Typing complete
        isTyping = false;
        typewriterCoroutine = null;

        // Call completion callback
        onComplete?.Invoke();
        onComplete = null;
    }
}
