using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

/// <summary>
/// 2D version of InteractionController.
/// Handles player interaction with 2D world objects using Physics2D.
/// Performs raycast or overlap circle to find IInteractable objects.
/// Only works when game state is Explore.
/// </summary>
public class InteractionController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameStateMachine gameStateMachine;
    [SerializeField] private EventBus eventBus;

    [Header("Raycast Settings")]
    [SerializeField] private float maxInteractionDistance = 10f;
    [SerializeField] private LayerMask interactionLayerMask = -1; // All layers by default
    [SerializeField] private bool useOverlapCircle = false;
    [SerializeField] private float overlapCircleRadius = 2f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private int mouseButton = 0; // 0 = Left, 1 = Right, 2 = Middle

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;

    private GameState currentGameState = GameState.Explore;
    private IInteractable currentInteractable = null;

    private void Awake()
    {
        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }

        // Auto-find GameStateMachine if not assigned
        if (gameStateMachine == null)
        {
            gameStateMachine = FindObjectOfType<GameStateMachine>();
        }

        // Auto-find EventBus if not assigned
        if (eventBus == null)
        {
            eventBus = FindObjectOfType<EventBus>();
        }
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (eventBus != null)
        {
            eventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (eventBus != null)
        {
            eventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }
    }

    private void OnGameStateChanged(GameStateChanged stateChange)
    {
        currentGameState = stateChange.To;
    }

    private void Update()
    {
        // Only allow interaction in Explore state
        if (currentGameState != GameState.Explore)
        {
            return;
        }

        // Update current interactable for hover feedback
        currentInteractable = GetCurrentInteractable();

        // Check for input
        bool interactInput = false;
        
        if (Input.GetMouseButtonDown(mouseButton))
        {
            interactInput = true;
        }
        
        if (Input.GetKeyDown(interactKey))
        {
            interactInput = true;
        }

        if (interactInput)
        {
            TryInteract();
        }
    }

    /// <summary>
    /// Attempt to interact with an object at the mouse position.
    /// </summary>
    public void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("InteractionController2D: No camera assigned!");
            return;
        }

        IInteractable interactable = null;

        if (useOverlapCircle)
        {
            interactable = FindInteractableOverlap();
        }
        else
        {
            interactable = FindInteractableRaycast();
        }

        if (interactable != null && interactable.CanInteract())
        {
            Debug.Log($"<color=yellow>[Interact2D]</color> Interacting with: {(interactable as MonoBehaviour)?.gameObject.name}");
            interactable.Interact();
        }
    }

    /// <summary>
    /// Find an interactable object using 2D raycast from camera through mouse position.
    /// </summary>
    private IInteractable FindInteractableRaycast()
    {
        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // For 2D, we use OverlapPoint or a short raycast
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, 0f, interactionLayerMask);

        // If no hit with zero distance, try a small area
        if (hit.collider == null)
        {
            hit = Physics2D.CircleCast(rayOrigin, 0.1f, Vector2.zero, 0f, interactionLayerMask);
        }

        if (showDebugRay)
        {
            Debug.DrawRay(rayOrigin, Vector2.up * 0.5f, hit.collider != null ? Color.green : Color.red, 0.1f);
        }

        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }
            return interactable;
        }

        return null;
    }

    /// <summary>
    /// Find an interactable object using overlap circle around player position.
    /// </summary>
    private IInteractable FindInteractableOverlap()
    {
        Vector2 playerPosition = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPosition, overlapCircleRadius, interactionLayerMask);

        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = col.GetComponentInParent<IInteractable>();
            }

            if (interactable != null && interactable.CanInteract())
            {
                float distance = Vector2.Distance(playerPosition, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        return closestInteractable;
    }

    /// <summary>
    /// Get the currently highlighted interactable (for UI feedback).
    /// </summary>
    public IInteractable GetCurrentInteractable()
    {
        if (currentGameState != GameState.Explore)
        {
            return null;
        }

        if (useOverlapCircle)
        {
            return FindInteractableOverlap();
        }
        else
        {
            return FindInteractableRaycast();
        }
    }

    /// <summary>
    /// Check if there's an interactable object available.
    /// </summary>
    public bool HasInteractable()
    {
        IInteractable interactable = GetCurrentInteractable();
        return interactable != null && interactable.CanInteract();
    }

    // Gizmo for debugging overlap circle
    private void OnDrawGizmosSelected()
    {
        if (useOverlapCircle)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, overlapCircleRadius);
        }
    }
}
