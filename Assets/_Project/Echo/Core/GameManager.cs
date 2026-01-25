using UnityEngine;
using GameContracts;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public FlagManager Flags { get; private set; }
    public GameStateMachine StateMachine { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        StateMachine = GetComponent<GameStateMachine>();
        Flags = GetComponent<FlagManager>();

        Debug.Log("<color=green>【GameManager】所有核心系统初始化完毕！</color>");
    }

    // 以后可以在这里写退出游戏、重置游戏等全局方法
}