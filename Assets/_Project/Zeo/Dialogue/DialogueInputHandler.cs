using UnityEngine;
using GameContracts;

/// <summary>
/// Handles input for dialogue system.
/// Listens for clicks/spacebar to skip typewriter or advance dialogue.
/// Only active when dialogue is active and game state is Dialogue.
/// </summary>
public class DialogueInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Input Settings")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private int mouseButton = 0; // 0 = Left click
    [SerializeField] private bool allowAnyKey = false; // Allow any key to advance

    private void Awake()
    {

    }

    private void Update()
    {
        // Only handle input when dialogue is active and game state is Dialogue
        if (dialogueManager == null || !dialogueManager.IsDialogueActive())
        {
            return;
        }

        if (GameStateMachine.Instance == null || GameStateMachine.Instance.CurrentState != GameState.Dialogue)
        {
            return;
        }

        // Check for input
        bool advanceInput = false;

        // Check mouse button
        if (Input.GetMouseButtonDown(mouseButton))
        {
            advanceInput = true;
        }

        // Check keyboard key
        if (Input.GetKeyDown(advanceKey))
        {
            advanceInput = true;
        }

        // Check any key (if enabled)
        if (allowAnyKey && Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
        {
            advanceInput = true;
        }

        if (advanceInput)
        {
            dialogueManager.SkipTypewriter();
        }
    }
}
