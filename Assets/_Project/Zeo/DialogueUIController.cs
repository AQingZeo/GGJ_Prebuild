using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls the dialogue UI panel, displaying speaker name and dialogue text.
/// Has proportional clamp on how many lines of text to display.
/// </summary>
public class DialogueUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TypewriterEffect typewriterEffect;

    [Header("Settings")]
    [SerializeField] private float maxLinesProportion = 0.3f; // Max 30% of screen height for dialogue text
    [SerializeField] private float lineSpacing = 1.2f;

    private Canvas canvas;
    private RectTransform dialogueTextRect;
    private float baseFontSize;
    private List<string> dialogueHistory = new List<string>();
    private int maxVisibleLines;

    private void Awake()
    {
        // Auto-find components if not assigned
        if (dialoguePanel == null)
            dialoguePanel = gameObject;

        if (dialogueText == null)
            dialogueText = GetComponentInChildren<TMP_Text>();

        if (typewriterEffect == null)
            typewriterEffect = dialogueText?.GetComponent<TypewriterEffect>();

        if (typewriterEffect == null && dialogueText != null)
        {
            // Add TypewriterEffect if it doesn't exist
            typewriterEffect = dialogueText.gameObject.AddComponent<TypewriterEffect>();
        }

        // Get canvas for screen calculations
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        if (dialogueText != null)
        {
            dialogueTextRect = dialogueText.GetComponent<RectTransform>();
            baseFontSize = dialogueText.fontSize;
        }

        // Calculate max visible lines based on screen proportion
        CalculateMaxVisibleLines();

        // Hide dialogue panel initially
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Recalculate on start in case canvas size changed
        CalculateMaxVisibleLines();
    }

    /// <summary>
    /// Calculate the maximum number of visible lines based on screen proportion.
    /// </summary>
    private void CalculateMaxVisibleLines()
    {
        if (canvas == null || dialogueTextRect == null)
        {
            maxVisibleLines = 3; // Default fallback
            return;
        }

        // Get screen height in canvas units
        float screenHeight = canvas.pixelRect.height;
        float maxTextHeight = screenHeight * maxLinesProportion;

        // Calculate lines based on font size and line spacing
        float lineHeight = baseFontSize * lineSpacing;
        maxVisibleLines = Mathf.Max(1, Mathf.FloorToInt(maxTextHeight / lineHeight));
    }

    /// <summary>
    /// Display a dialogue line with speaker name and text.
    /// Called by DialogueManager to show dialogue content.
    /// </summary>
    public void ShowLine(string speaker, string text)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // Update speaker name
        if (speakerNameText != null)
        {
            speakerNameText.text = string.IsNullOrEmpty(speaker) ? "" : speaker;
        }

        // Add to history
        if (!string.IsNullOrEmpty(text))
        {
            dialogueHistory.Add(text);
        }

        // Clamp dialogue history to max visible lines
        ClampDialogueHistory();

        // Build display text from history
        string displayText = BuildDisplayText();

        // Update dialogue text and start typewriter
        if (dialogueText != null)
        {
            if (typewriterEffect != null)
            {
                // Use typewriter effect
                typewriterEffect.StartTyping(displayText);
            }
            else
            {
                // Fallback: show text immediately
                dialogueText.text = displayText;
            }
        }
    }

    /// <summary>
    /// Clamp the dialogue history to the maximum number of visible lines.
    /// </summary>
    private void ClampDialogueHistory()
    {
        if (dialogueHistory.Count > maxVisibleLines)
        {
            // Remove oldest entries
            int removeCount = dialogueHistory.Count - maxVisibleLines;
            dialogueHistory.RemoveRange(0, removeCount);
        }
    }

    /// <summary>
    /// Build the display text from dialogue history.
    /// </summary>
    private string BuildDisplayText()
    {
        if (dialogueHistory.Count == 0)
        {
            return "";
        }

        // Join all history lines with newlines
        return string.Join("\n", dialogueHistory);
    }

    /// <summary>
    /// Hide the dialogue panel.
    /// Called by DialogueManager when dialogue ends.
    /// </summary>
    public void Hide()
    {
        // Stop typewriter if running
        if (typewriterEffect != null)
        {
            typewriterEffect.StopTyping();
        }

        // Clear dialogue history
        dialogueHistory.Clear();

        // Clear text
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = "";
        }

        // Hide panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Check if the typewriter is currently typing.
    /// </summary>
    public bool IsTyping()
    {
        return typewriterEffect != null && typewriterEffect.IsTyping();
    }

    /// <summary>
    /// Skip the typewriter effect and show full text immediately.
    /// </summary>
    public void SkipTypewriter()
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.Skip();
        }
    }

    /// <summary>
    /// Clear the dialogue history (useful for starting a new dialogue).
    /// </summary>
    public void ClearHistory()
    {
        dialogueHistory.Clear();
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
    }
}
