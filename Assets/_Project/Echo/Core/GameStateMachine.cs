using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance;

    public enum GameState { Explore, Dialogue, Pause }
    public GameState CurrentState = GameState.Explore;

    private void Awake()
    {
        // 经典的单例模式，方便随时访问
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"<color=yellow>[State] 游戏状态切换至: {newState}</color>");
    }
}