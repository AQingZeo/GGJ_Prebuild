using UnityEngine;
using GameContracts;

public class GameStateMachine : MonoBehaviour
{

    public static GameStateMachine Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public GameState PreviousState { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentState = GameState.Explore;
    }

    public void SetState(GameState newState)
    {
        if (newState == CurrentState) return;

        PreviousState = CurrentState;
        CurrentState = newState;

        Debug.Log($"<color=orange>¡¾×´Ì¬»ú¡¿×´Ì¬ÇÐ»»: {PreviousState} -> {CurrentState}</color>");

        EventBus.Publish(new GameStateChanged(PreviousState, CurrentState));
    }

    public void ReturnToPreviousState()
    {
        SetState(PreviousState);
    }
}