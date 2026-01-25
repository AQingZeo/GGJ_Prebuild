
using GameContracts;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialogueBox; 
    public TextMeshProUGUI contentText; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialogueBox.SetActive(false);
    }

    private void Start()
    {
        EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
    }

    public void StartDialogue(string text)
    {
        contentText.text = text;
        GameManager.Instance.StateMachine.SetState(GameState.Dialogue);
    }

    public void EndDialogue()
    {
        GameManager.Instance.StateMachine.SetState(GameState.Explore);
    }

    private void OnGameStateChanged(GameStateChanged eventData)
    {
        bool isDialogue = (eventData.NewState == GameState.Dialogue);
        dialogueBox.SetActive(isDialogue);

        if (isDialogue)
        {
            Debug.Log("【系统】进入对话模式，UI已显示，输入已锁定。");
        }
        else
        {
            Debug.Log("【系统】退出对话模式，UI已隐藏，玩家可移动。");
        }
    }
    public void DisplayNextSentence()
    {
        Debug.Log("【对话系统】收到路由指令：显示下一句（当前直接结束）");
        EndDialogue();
    }

    public void OnClickDialogueBox()
    {
        EndDialogue();
    }
}