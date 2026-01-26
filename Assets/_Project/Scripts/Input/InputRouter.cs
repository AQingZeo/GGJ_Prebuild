using GameContracts;
using UnityEngine;

/// <summary>
/// Centralized input router. Only this file calls Input.*
/// Publishes raw intent; scenes/overlays interpret it when active.
/// Pause is handled directly here.
/// </summary>
public class InputRouter : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private GameStateMachine stateMachine;

    private void Awake()
    {
        // Make InputRouter persistent (Bootstrap should not be destroyed)
        DontDestroyOnLoad(gameObject);

        stateMachine = GameStateMachine.Instance;
        if (stateMachine == null) Debug.LogError("GameStateMachine.Instance is null");
    }

    private void Update()
    {
        if (stateMachine == null) return;

        var state = stateMachine.CurrentState;

        // Menu: no gameplay input, no pause toggle (per your rule)
        if (state == GameState.Menu) return;

        // Pause toggle (direct)
        if (Input.GetKeyDown(pauseKey))
        {
            if (state == GameState.Pause) stateMachine.ReturnToPreviousState();
            else stateMachine.SetState(GameState.Pause);
            return;
        }

        // No gameplay intents while paused or in cutscene (unless you later want skip)
        if (state == GameState.Pause || state == GameState.CutScene) return;

        // Build intents (no interpretation here)
        float horizontal = ReadHorizontal();
        bool clickDown = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);

        // "Any other key = submit" (exclude pause, exclude mouse)
        bool submitDown =
            Input.anyKeyDown &&
            !Input.GetKeyDown(pauseKey) &&
            !clickDown;

        // Publish one event; consumers decide what to do
        EventBus.Publish(new InputIntentEvent(horizontal, submitDown, clickDown, Input.mousePosition));
    }

    private float ReadHorizontal()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) return -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return 1f;
        return 0f;
    }
}