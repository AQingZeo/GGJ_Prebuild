using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameContracts;

/// <summary>
/// What happens after the trigger is fired.
/// Execute after OnTriggerEnter() or OnMouseDown().
/// </summary>
public enum InteractionType
{
    ClickOnly,      // Only click interaction (no collision trigger)
    CollisionOnly,  // Only collision trigger (immediate on enter, no click)
    Both            // Both click and collision interaction
}

public class ItemInteractable : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemId = ""; // For inventory pickup and/or logging
    [SerializeField] private bool addToInventoryOnTrigger = false; // If true, add itemId to inventory on trigger/click
    [SerializeField] private ItemDefinition itemDefinition; // Optional: use this itemId/display if set

    [Header("Flag Settings")]
    [SerializeField] private string setFlagKey = "";
    [SerializeField] private bool setFlagBoolValue = false;
    [SerializeField] private int setFlagIntValue = 0;

    [Header("Dialogue")]
    [SerializeField] private string dialogueId = ""; // Optional

    [Header("Behavior")]
    [SerializeField] private bool destroyOnTrigger = true;
    [SerializeField] private InteractionType interactionType = InteractionType.Both;

    /// <summary>
    /// Execute after OnTriggerEnter() (collision-based interaction).
    /// Only works if interactionType is CollisionOnly or Both.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only process collision if interaction type allows it
        if (interactionType == InteractionType.ClickOnly) return;
        
        // Check if trigger is from player (you may want to add a tag check)
        // For now, execute on any trigger
        
        ExecuteTrigger();
    }

    /// <summary>
    /// Unity's built-in mouse click detection (click-based interaction).
    /// Only works in Explore state and when interactionType allows clicks.
    /// Requires the object to have a Collider2D (can be a trigger).
    /// </summary>
    private void OnMouseDown()
    {
        // Only process clicks if interaction type allows it
        if (interactionType == InteractionType.CollisionOnly) return;
        
        // Only allow clicks in Explore state
        if (GameStateMachine.Instance == null || GameStateMachine.Instance.CurrentState != GameState.Explore)
            return;

        ExecuteTrigger();
    }

    private string EffectiveItemId => !string.IsNullOrEmpty(itemId) ? itemId : (itemDefinition != null ? itemDefinition.itemId : "");

    private void ExecuteTrigger()
    {
        // Optional: add to inventory (collectible pickup)
        if (addToInventoryOnTrigger)
        {
            string id = EffectiveItemId;
            if (!string.IsNullOrEmpty(id) && GameManager.Instance != null && GameManager.Instance.Inventory != null)
            {
                GameManager.Instance.Inventory.AddItem(id);
            }
        }

        // Set flag if configured
        if (!string.IsNullOrEmpty(setFlagKey) && GameManager.Instance != null && GameManager.Instance.Flags != null)
        {
            var flags = GameManager.Instance.Flags;
            if (setFlagIntValue != 0)
                flags.Set(setFlagKey, setFlagIntValue);
            else
                flags.Set(setFlagKey, setFlagBoolValue);
        }

        // Start dialogue if configured
        if (!string.IsNullOrEmpty(dialogueId))
        {
            if (GameStateMachine.Instance != null)
                GameStateMachine.Instance.SetState(GameState.Dialogue);
            StartCoroutine(StartDialogueWhenReady(dialogueId, destroyOnTrigger));
        }

        if (!string.IsNullOrEmpty(EffectiveItemId) && !addToInventoryOnTrigger)
        {
            Debug.Log($"Item interacted: {EffectiveItemId}");
        }
    }

    private IEnumerator StartDialogueWhenReady(string dialogueId, bool shouldDestroy)
    {
        const string dialogueSceneName = "DialogueScene";
        DialogueManager dm = null;
        
        // Wait for Dialogue scene to load
        int maxAttempts = 20;
        int attempts = 0;
        
        while (dm == null && attempts < maxAttempts)
        {
            var scene = SceneManager.GetSceneByName(dialogueSceneName);
            if (scene.isLoaded)
            {
                // Get DialogueManager from the loaded scene
                var rootObjects = scene.GetRootGameObjects();
                foreach (var obj in rootObjects)
                {
                    dm = obj.GetComponentInChildren<DialogueManager>();
                    if (dm != null) break;
                }
            }
            
            if (dm == null)
            {
                yield return null; // Wait one frame
                attempts++;
            }
        }

        if (dm != null)
        {
            dm.StartDialogue(dialogueId);
        }
        else
        {
            Debug.LogError($"ItemInteractable on {gameObject.name}: DialogueManager not found in DialogueScene after loading!");
        }
        
        if (shouldDestroy && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}