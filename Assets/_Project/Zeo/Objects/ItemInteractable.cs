using UnityEngine;

public class ItemInteractable : MonoBehaviour, IInteractable
{
    [Header("Zeo 规格设置")]
    public string dialogueText = "你触发了 ItemInteractable！";
    public string flagKey = "MetNPC"; 

    public void Interact()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueText);
        }

        if (GameManager.Instance != null && GameManager.Instance.Flags != null)
        {
            GameManager.Instance.Flags.SetFlag(flagKey, true);
            Debug.Log($"[Flag] 已设置标记: {flagKey}");
        }
    }
}