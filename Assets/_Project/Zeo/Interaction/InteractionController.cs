using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

/// <summary>
/// Handles player interaction with world objects.
/// Performs raycast/overlap on click to find IInteractable objects and calls Interact().
/// Only works when game state is Explore.
/// </summary>
public class InteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameStateMachine gameStateMachine;
    [SerializeField] private EventBus eventBus;

    [Header("Raycast Settings")]
    [SerializeField] private float maxInteractionDistance = 10f;
    [SerializeField] private LayerMask interactionLayerMask = -1; // All layers by default
    [SerializeField] private bool useOverlapSphere = false;
    [SerializeField] private float overlapSphereRadius = 2f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private int mouseButton = 0; // 0 = Left, 1 = Right, 2 = Middle

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
        // Subscribe to GameStateChanged events via EventBus
        if (eventBus != null)
        {
            eventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        }
    }

    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from events
        if (eventBus != null)
        {
            eventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
        }
    }

    /// <summary>
    /// Called by EventBus when GameState changes.
    /// </summary>
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

        // Check for input
        bool interactInput = false;
        
        // Check mouse button
        if (Input.GetMouseButtonDown(mouseButton))
        {
            interactInput = true;
        }
        
        // Check keyboard key
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
    /// Attempt to interact with an object at the mouse position or player position.
    /// </summary>
    public void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("InteractionController: No camera assigned!");
            return;
        }

        IInteractable interactable = null;

        if (useOverlapSphere)
        {
            // Use overlap sphere around player position
            interactable = FindInteractableOverlap();
        }
        else
        {
            // Use raycast from camera through mouse position
            interactable = FindInteractableRaycast();
        }

        if (interactable != null && interactable.CanInteract())
        {
            interactable.Interact();
        }
    }

    /// <summary>
    /// Find an interactable object using raycast from camera through mouse position.
    /// </summary>
    private IInteractable FindInteractableRaycast()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxInteractionDistance, interactionLayerMask))
        {
            // Try to get IInteractable from the hit object
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                // Try to get from parent
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }

            return interactable;
        }

        return null;
    }

    /// <summary>
    /// Find an interactable object using overlap sphere around player position.
    /// </summary>
    private IInteractable FindInteractableOverlap()
    {
        Vector3 playerPosition = transform.position;
        Collider[] colliders = Physics.OverlapSphere(playerPosition, overlapSphereRadius, interactionLayerMask);

        // Find the closest interactable
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = col.GetComponentInParent<IInteractable>();
            }

            if (interactable != null && interactable.CanInteract())
            {
                float distance = Vector3.Distance(playerPosition, col.transform.position);
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

        if (useOverlapSphere)
        {
            return FindInteractableOverlap();
        }
        else
        {
            return FindInteractableRaycast();
        }
    }

    /// <summary>
    /// Check if there's an interactable object available at the current position.
    /// </summary>
    public bool HasInteractable()
    {
        IInteractable interactable = GetCurrentInteractable();
        return interactable != null && interactable.CanInteract();
    }

    // Gizmo for debugging overlap sphere
    private void OnDrawGizmosSelected()
    {
        if (useOverlapSphere)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, overlapSphereRadius);
        }
    }
}
