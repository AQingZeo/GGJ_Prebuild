using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the display of dialogue choice buttons.
/// Spawns buttons dynamically and calls DialogueManager.SelectChoice when clicked.
/// </summary>
public class ChoiceUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform buttonContainer;

    [Header("Settings")]
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private bool verticalLayout = true;

    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private VerticalLayoutGroup verticalLayoutGroup;
    private HorizontalLayoutGroup horizontalLayoutGroup;

    private void Awake()
    {
        // Auto-find DialogueManager if not assigned
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        // Setup button container
        if (buttonContainer == null)
        {
            buttonContainer = transform;
        }

        // Setup layout group
        SetupLayoutGroup();
    }

    private void Start()
    {
        // Create default button prefab if none assigned
        if (choiceButtonPrefab == null)
        {
            CreateDefaultButtonPrefab();
        }
    }

    /// <summary>
    /// Setup the layout group for button arrangement.
    /// </summary>
    private void SetupLayoutGroup()
    {
        if (buttonContainer == null) return;

        // Remove existing layout groups
        VerticalLayoutGroup existingVertical = buttonContainer.GetComponent<VerticalLayoutGroup>();
        HorizontalLayoutGroup existingHorizontal = buttonContainer.GetComponent<HorizontalLayoutGroup>();

        if (existingVertical != null)
            DestroyImmediate(existingVertical);
        if (existingHorizontal != null)
            DestroyImmediate(existingHorizontal);

        // Add appropriate layout group
        if (verticalLayout)
        {
            verticalLayoutGroup = buttonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.spacing = buttonSpacing;
            verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.childForceExpandWidth = true;
        }
        else
        {
            horizontalLayoutGroup = buttonContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.spacing = buttonSpacing;
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayoutGroup.childControlHeight = true;
            horizontalLayoutGroup.childControlWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = false;
            horizontalLayoutGroup.childForceExpandWidth = false;
        }
    }

    /// <summary>
    /// Create a default button prefab if none is assigned.
    /// To customize button appearance, edit the values below or create your own prefab in Unity.
    /// </summary>
    private void CreateDefaultButtonPrefab()
    {
        // Create a simple button template programmatically
        GameObject buttonObj = new GameObject("ChoiceButtonTemplate");
        // Don't parent it to buttonContainer - keep it as a standalone template
        buttonObj.transform.SetParent(null);
        DontDestroyOnLoad(buttonObj); // Keep it across scenes

        // ===== BUTTON SIZE AND LAYOUT =====
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(450, 70); // Width x Height

        // ===== BUTTON BACKGROUND COLOR =====
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f); // Dark blue-grey

        // ===== BUTTON INTERACTION COLORS =====
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);     // Default state
        colors.highlightedColor = new Color(0.25f, 0.3f, 0.4f, 1f);   // Mouse hover
        colors.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);       // Clicked
        colors.selectedColor = new Color(0.2f, 0.25f, 0.35f, 1f);     // Selected
        button.colors = colors;

        // ===== BUTTON TEXT (CHILD OBJECT) =====
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // ===== TEXT APPEARANCE =====
        TMP_Text buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Choice";
        buttonText.fontSize = 22;                                  // Font size
        buttonText.alignment = TextAlignmentOptions.Center;        // Text alignment
        buttonText.color = new Color(0.95f, 0.95f, 1f, 1f);       // Off-white
        buttonText.fontStyle = FontStyles.Normal;                  // Style (Normal, Bold, Italic)

        // Set as prefab reference (this will be used as template)
        choiceButtonPrefab = buttonObj;
        buttonObj.SetActive(false); // Hide template
    }

    /// <summary>
    /// Show choice buttons with the given texts.
    /// Called by DialogueManager when choices are available.
    /// </summary>
    public void ShowChoices(List<string> choiceTexts)
    {
        if (choiceTexts == null || choiceTexts.Count == 0)
        {
            ClearChoices();
            return;
        }

        // Create default button prefab if none assigned
        if (choiceButtonPrefab == null)
        {
            CreateDefaultButtonPrefab();
        }

        // Clear existing choices
        ClearChoices();

        // Create buttons for each choice
        for (int i = 0; i < choiceTexts.Count; i++)
        {
            CreateChoiceButton(i, choiceTexts[i]);
        }
    }

    /// <summary>
    /// Create a single choice button.
    /// </summary>
    private void CreateChoiceButton(int index, string choiceText)
    {
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("Choice button prefab is not assigned!");
            return;
        }

        // Instantiate button
        GameObject buttonObj = Instantiate(choiceButtonPrefab, buttonContainer);
        buttonObj.SetActive(true);

        // Get button component
        Button button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Choice button prefab must have a Button component!");
            Destroy(buttonObj);
            return;
        }

        // Set button text
        TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = choiceText;
        }

        // Add click listener
        int choiceIndex = index; // Capture for closure
        button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));

        // Add to list
        currentChoiceButtons.Add(buttonObj);
    }

    /// <summary>
    /// Handle choice button click.
    /// </summary>
    private void OnChoiceSelected(int choiceIndex)
    {
        if (dialogueManager != null)
        {
            dialogueManager.SelectChoice(choiceIndex);
        }
        else
        {
            Debug.LogError("DialogueManager not found! Cannot select choice.");
        }
    }

    /// <summary>
    /// Clear all choice buttons.
    /// Called by DialogueManager when choices are no longer needed.
    /// </summary>
    public void ClearChoices()
    {
        foreach (GameObject button in currentChoiceButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }

        currentChoiceButtons.Clear();
    }

    private void OnDestroy()
    {
        ClearChoices();
    }
}
