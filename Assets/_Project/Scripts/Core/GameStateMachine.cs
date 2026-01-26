using UnityEngine;
using GameContracts;

public class GameStateMachine : MonoBehaviour
{

    public static GameStateMachine Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public GameState PreviousState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Make GameStateMachine persistent (Bootstrap should not be destroyed)
            DontDestroyOnLoad(gameObject);
            Debug.Log("<color=green>【GameStateMachine】Initialized and made persistent</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>【GameStateMachine】Duplicate instance detected, destroying...</color>");
            Destroy(gameObject);
            return;
        }

        CurrentState = GameState.Menu;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Debug.LogError("<color=red>【GameStateMachine】Instance is being destroyed! This should not happen - Bootstrap should persist.</color>");
            Instance = null;
        }
    }

    public void SetState(GameState newState)
    {
        if (newState == CurrentState) return;

        PreviousState = CurrentState;
        CurrentState = newState;

        Debug.Log($"<color=orange>【状态机】状态切换: {PreviousState} -> {CurrentState}</color>");

        EventBus.Publish(new GameStateChanged(PreviousState, CurrentState));
    }

    public void ReturnToPreviousState()
    {
        SetState(PreviousState);
    }
}