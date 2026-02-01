using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls the dialogue UI panel. Text that doesn't fit (overflow) is split into pages;
/// click advances to next page. Choices and following nodes run only after the full line (all pages) are finished.
/// </summary>
public class DialogueUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TypewriterEffect typewriterEffect;
    [SerializeField] private ChaosEffect chaosEffect;

    [Header("Settings")]
    [SerializeField] private float maxLinesProportion = 0.4f; // Max 40% of screen height for dialogue text
    [SerializeField] private float lineSpacing = 1.2f;
    [SerializeField] private bool constrainTextBounds = true; // Ensure text stays within bounds
    [Tooltip("Approximate characters per line for paging. Overflow goes to next page; lower = more pages.")]
    [SerializeField] private int estimatedCharsPerLine = 40;
    [Tooltip("Max chars per page (0 = use maxVisibleLines * estimatedCharsPerLine only). Set e.g. 18–24 to split long lines into pages; 0 = show full line (may truncate if panel is small).")]
    [SerializeField] private int maxCharsPerPageCap = 0;

    [Header("Optional (assign or uses parent Canvas)")]
    [SerializeField] private Canvas canvasRef;

    private Canvas canvas;
    private RectTransform dialogueTextRect;
    private float baseFontSize;
    private List<string> dialogueHistory = new List<string>();
    private int maxVisibleLines;

    // Paging: when one line doesn't fit, split into pages; click advances to next page
    private List<string> _pagedPages;
    private int _pagedIndex;
    private string _pagedFullText;
    private System.Action _pagedOnComplete;

    private void Awake()
    {
        canvas = canvasRef != null ? canvasRef : GetComponentInParent<Canvas>();

        if (dialogueText != null)
        {
            dialogueTextRect = dialogueText.GetComponent<RectTransform>();
            baseFontSize = dialogueText.fontSize;
            
            if (dialogueTextRect != null)
            {
                dialogueText.enableWordWrapping = true;
                dialogueText.overflowMode = constrainTextBounds ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
            }
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
    /// Long lines are split into pages; click advances to next page before advancing to next node.
    /// </summary>
    /// <param name="speaker">Speaker name</param>
    /// <param name="text">Dialogue text</param>
    /// <param name="onComplete">Optional callback when typewriter completes (and all pages shown if paged)</param>
    public void ShowLine(string speaker, string text, System.Action onComplete = null)
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

        if (string.IsNullOrEmpty(text))
        {
            onComplete?.Invoke();
            return;
        }

        List<string> pages = SplitTextIntoPages(text);
        if (pages.Count <= 1)
        {
            // Fits in one page: current behavior
            dialogueHistory.Add(text);
            ClampDialogueHistory();
            string displayText = BuildDisplayText();
            StartTypewriterForText(displayText, onComplete);
            return;
        }

        // Multiple pages: show first page, wait for click to show next
        _pagedPages = pages;
        _pagedIndex = 0;
        _pagedFullText = text;
        _pagedOnComplete = onComplete;
        StartTypewriterForText(_pagedPages[0], OnPageTypedComplete);
    }

    /// <summary>
    /// Start typewriter with optional chaos transform; used by ShowLine and ShowNextPage.
    /// </summary>
    private void StartTypewriterForText(string text, System.Action onComplete)
    {
        if (dialogueText == null) return;

        if (typewriterEffect != null)
        {
            System.Func<char, int, char> characterTransform = null;
            if (chaosEffect != null && chaosEffect.ShouldApplyChaos())
            {
                characterTransform = chaosEffect.GetCharacterTransform();
            }
            typewriterEffect.StartTyping(text, onComplete, characterTransform);
        }
        else
        {
            dialogueText.text = text;
            onComplete?.Invoke();
        }
    }

    private void OnPageTypedComplete()
    {
        if (_pagedPages == null) return;

        if (_pagedIndex >= _pagedPages.Count - 1)
        {
            dialogueHistory.Add(_pagedFullText);
            ClampDialogueHistory();
            var callback = _pagedOnComplete;
            ClearPagingState();
            callback?.Invoke();
        }
        // Else: more pages; wait for user click -> ShowNextPage
    }

    private void ClearPagingState()
    {
        _pagedPages = null;
        _pagedIndex = 0;
        _pagedFullText = null;
        _pagedOnComplete = null;
    }

    /// <summary>
    /// True when a long line was split into pages and not all pages have been shown yet.
    /// </summary>
    public bool HasMorePages()
    {
        return _pagedPages != null && _pagedIndex < _pagedPages.Count - 1;
    }

    /// <summary>
    /// Advance to the next page of the current line (call when not typing and HasMorePages).
    /// </summary>
    public void ShowNextPage()
    {
        if (!HasMorePages()) return;
        _pagedIndex++;
        StartTypewriterForText(_pagedPages[_pagedIndex], OnPageTypedComplete);
    }

    /// <summary>
    /// Split text into page-sized chunks so each fits in the visible window.
    /// </summary>
    private List<string> SplitTextIntoPages(string text)
    {
        int calculated = maxVisibleLines * estimatedCharsPerLine;
        int maxCharsPerPage = maxCharsPerPageCap > 0
            ? Mathf.Max(1, Mathf.Min(calculated, maxCharsPerPageCap))
            : Mathf.Max(1, calculated);
        var pages = new List<string>();

        // Split by newlines first, then recombine into page-sized chunks
        string[] paragraphs = text.Split('\n');
        var currentPage = new System.Text.StringBuilder();

        foreach (string para in paragraphs)
        {
            if (currentPage.Length > 0)
            {
                currentPage.Append('\n');
            }

            if (para.Length <= maxCharsPerPage && currentPage.Length + para.Length <= maxCharsPerPage)
            {
                currentPage.Append(para);
                continue;
            }

            // Flush current page if adding this para would exceed
            if (currentPage.Length + para.Length > maxCharsPerPage && currentPage.Length > 0)
            {
                pages.Add(currentPage.ToString().TrimEnd());
                currentPage.Clear();
            }

            // Break long paragraph by character count at word boundaries
            int start = 0;
            while (start < para.Length)
            {
                int take = Mathf.Min(maxCharsPerPage - currentPage.Length, para.Length - start);
                if (take <= 0)
                {
                    pages.Add(currentPage.ToString().TrimEnd());
                    currentPage.Clear();
                    take = Mathf.Min(maxCharsPerPage, para.Length - start);
                }

                int end = start + take;
                if (end < para.Length)
                {
                    int lastSpace = para.LastIndexOf(' ', end - 1, take);
                    if (lastSpace >= start)
                    {
                        end = lastSpace + 1;
                    }
                }

                currentPage.Append(para.Substring(start, end - start));
                start = end;
                if (start < para.Length && para[start] == ' ')
                {
                    start++;
                }
                if (currentPage.Length >= maxCharsPerPage || start >= para.Length)
                {
                    pages.Add(currentPage.ToString().TrimEnd());
                    currentPage.Clear();
                }
            }
        }

        if (currentPage.Length > 0)
        {
            pages.Add(currentPage.ToString().TrimEnd());
        }

        return pages.Count > 0 ? pages : new List<string> { text };
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
        if (typewriterEffect != null)
        {
            typewriterEffect.StopTyping();
        }
        ClearPagingState();
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
        if (typewriterEffect != null)
        {
            typewriterEffect.StopTyping();
        }
        ClearPagingState();
        dialogueHistory.Clear();
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
    }
}
