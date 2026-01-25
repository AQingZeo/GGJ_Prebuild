using GameContracts;
using UnityEngine;

public class InputRouter : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance == null) return;

        // 根据不同状态执行不同的输入逻辑
        GameState currentState = GameManager.Instance.StateMachine.CurrentState;

        if (currentState == GameState.Explore)
        {
            HandleExploreInput();
        }
        else if (currentState == GameState.Dialogue)
        {
            HandleDialogueInput();
        }
    }

    void HandleExploreInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var ic = FindObjectOfType<InteractionController>();
            if (ic != null) ic.TryInteract();
        }
    }

    void HandleDialogueInput()
    {
        // 在对话时，按 E 或者 空格 都会关闭对话
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            DialogueManager.Instance.EndDialogue();
        }
    }
}