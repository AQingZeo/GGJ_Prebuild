using UnityEngine;
using TMPro;
using GameContracts; 

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBox;
    public TextMeshProUGUI contentText;

    // --- 1. 订阅新版的 EventBus ---
    private void OnEnable()
    {
        EventBus.Subscribe<InteractionEvent>(HandleInteraction);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<InteractionEvent>(HandleInteraction);
    }

    private void HandleInteraction(InteractionEvent evt)
    {
        dialogueBox.SetActive(true);
        contentText.text = "收到物体ID: " + evt.ID;

        // old: ChangeState(GameStateMachine.GameState.Dialogue)
        // new: SetState(GameState.Dialogue)
        GameStateMachine.Instance.SetState(GameState.Dialogue);
    }
}