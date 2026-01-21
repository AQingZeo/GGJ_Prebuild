using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

/// <summary>
/// A world object that can be interacted with.
/// Supports optional dialogue initiation and collectible items.
/// </summary>
public class ItemInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private string interactionPrompt = "Press E to interact";

    [Header("Dialogue")]
    [SerializeField] private bool hasDialogue = false;
    [SerializeField] private string dialogueID = "";

    [Header("Collectible")]
    [SerializeField] private bool isCollectible = false;
    [SerializeField] private string collectibleName = "";
    [SerializeField] private bool setFlagOnCollect = false;
    [SerializeField] private string flagKey = "";
    [SerializeField] private string flagValue = "";

    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private FlagManager flagManager;

    [Header("Visual Feedback")]
    [SerializeField] private bool showPrompt = true;
    [SerializeField] private GameObject highlightObject; // Optional object to highlight when interactable

    private bool hasBeenCollected = false;

    private void Awake()
    {
        // Auto-find DialogueManager if not assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        // Auto-find FlagManager if not assigned
        if (flagManager == null)
        {
            flagManager = FindObjectOfType<FlagManager>();
        }
    }

    /// <summary>
    /// Check if this object can currently be interacted with.
    /// </summary>
    public bool CanInteract()
    {
        if (!canInteract)
        {
            return false;
        }

        // If it's a collectible and already collected, can't interact again
        if (isCollectible && hasBeenCollected)
        {
            return false;
        }

        // If it has dialogue but no dialogue ID, can't interact
        if (hasDialogue && string.IsNullOrEmpty(dialogueID))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Perform the interaction action.
    /// </summary>
    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        // Handle collectible first
        if (isCollectible && !hasBeenCollected)
        {
            CollectItem();
        }

        // Handle dialogue
        if (hasDialogue && !string.IsNullOrEmpty(dialogueID))
        {
            StartDialogue();
        }
    }

    /// <summary>
    /// Collect this item and set flags if configured.
    /// </summary>
    private void CollectItem()
    {
        hasBeenCollected = true;

        // Set flag if configured
        if (setFlagOnCollect && !string.IsNullOrEmpty(flagKey) && flagManager != null)
        {
            // Assuming FlagManager has SetFlag method
            // flagManager.SetFlag(flagKey, flagValue);
            Debug.Log($"Item collected: {collectibleName}. Flag set: {flagKey} = {flagValue}");
        }
        else
        {
            Debug.Log($"Item collected: {collectibleName}");
        }

        // TODO: Add to Player Status String if needed
        // This would require access to a PlayerStatus system

        // Optionally disable the object after collection
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// Start dialogue if dialogue ID is set.
    /// </summary>
    private void StartDialogue()
    {
        if (dialogueManager == null)
        {
            Debug.LogError($"ItemInteractable on {gameObject.name}: DialogueManager not found! Cannot start dialogue.");
            return;
        }

        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning($"ItemInteractable on {gameObject.name}: Dialogue ID is empty!");
            return;
        }

        dialogueManager.StartDialogue(dialogueID);
    }

    /// <summary>
    /// Get the interaction prompt text.
    /// </summary>
    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    /// <summary>
    /// Enable or disable interaction for this object.
    /// </summary>
    public void SetCanInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    /// <summary>
    /// Set the dialogue ID for this interactable.
    /// </summary>
    public void SetDialogueID(string dialogueId)
    {
        this.dialogueID = dialogueId;
        this.hasDialogue = !string.IsNullOrEmpty(dialogueId);
    }

    /// <summary>
    /// Set this as a collectible item.
    /// </summary>
    public void SetCollectible(string collectibleName, bool setFlag = false, string flagKey = "", string flagValue = "")
    {
        this.isCollectible = true;
        this.collectibleName = collectibleName;
        this.setFlagOnCollect = setFlag;
        this.flagKey = flagKey;
        this.flagValue = flagValue;
    }

    /// <summary>
    /// Check if this item has been collected.
    /// </summary>
    public bool HasBeenCollected()
    {
        return hasBeenCollected;
    }

    /// <summary>
    /// Reset the collected state (useful for testing or respawning).
    /// </summary>
    public void ResetCollectedState()
    {
        hasBeenCollected = false;
    }

    // Visual feedback methods (can be called by InteractionController for UI)
    public void OnHighlight()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(true);
        }
    }

    public void OnUnhighlight()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }
}
