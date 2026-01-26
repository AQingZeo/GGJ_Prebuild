using UnityEngine;
using GameContracts;

/// <summary>
/// Only applied movement to the player using the input from InputRouter.
/// Do not interact with GSM (GameStateMachine).
/// </summary>
public class PalyerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb2d;
    private float horizontalInput = 0f;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 0; // For 2D top-down movement
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
        EventBus.Subscribe<InputIntentEvent>(OnInputIntent);
        EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
    }

    private void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<InputIntentEvent>(OnInputIntent);
        EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
    }

    private void OnInputIntent(InputIntentEvent inputIntent)
    {
        // Only process movement input in Explore state
        if (GameStateMachine.Instance == null || GameStateMachine.Instance.CurrentState != GameState.Explore)
            return;

        // Store horizontal input for movement
        horizontalInput = inputIntent.Horizontal;
    }

    private void OnGameStateChanged(GameStateChanged stateChange)
    {
        // Reset movement when leaving Explore state
        if (stateChange.PreviousState == GameState.Explore && stateChange.NewState != GameState.Explore)
        {
            horizontalInput = 0f;
            // Immediately stop movement
            if (rb2d != null)
            {
                rb2d.velocity = new Vector2(0f, rb2d.velocity.y);
            }
        }
    }

    private void FixedUpdate()
    {
        // Only apply movement in Explore state
        if (GameStateMachine.Instance == null || GameStateMachine.Instance.CurrentState != GameState.Explore)
        {
            // Ensure movement is stopped when not in Explore state
            if (rb2d != null)
            {
                rb2d.velocity = new Vector2(0f, rb2d.velocity.y);
            }
            return;
        }

        // Apply movement using the input from InputRouter
        if (rb2d != null)
        {
            Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb2d.velocity.y);
            rb2d.velocity = movement;
        }
    }
}
