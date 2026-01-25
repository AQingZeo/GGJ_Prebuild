using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

/// <summary>
/// Handles player interaction with 2D world objects using Physics2D.
/// Performs raycast or overlap circle to find IInteractable objects.
/// Only works when game state is Explore.
/// </summary>
public class InteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask interactionLayerMask = -1;
    [SerializeField] private bool useOverlapCircle = false;
    [SerializeField] private float overlapCircleRadius = 2f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private int mouseButton = 0;

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;

    private GameState currentGameState = GameState.Explore;
    private IInteractable currentInteractable = null;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
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
        EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
    }

    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
    }

    private void OnGameStateChanged(GameStateChanged stateChange)
    {
        currentGameState = stateChange.To;
    }

    private void Update()
    {
        if (currentGameState != GameState.Explore)
        {
            return;
        }

        currentInteractable = GetCurrentInteractable();

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

    public void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("InteractionController: No camera assigned!");
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
            interactable.Interact();
        }
    }

    private IInteractable FindInteractableRaycast()
    {
        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, 0f, interactionLayerMask);

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

    public bool HasInteractable()
    {
        IInteractable interactable = GetCurrentInteractable();
        return interactable != null && interactable.CanInteract();
    }

    private void OnDrawGizmosSelected()
    {
        if (useOverlapCircle)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, overlapCircleRadius);
        }
    }
}
