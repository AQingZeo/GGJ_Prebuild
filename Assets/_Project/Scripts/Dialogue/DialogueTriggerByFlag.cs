using UnityEngine;
using GameContracts;

/// <summary>
/// Starts a dialogue when a flag is set (no click required).
/// Subscribe to FlagChanged; when the flag matches, set state to Dialogue and start the given dialogue.
/// Place in a scene that loads when you want the trigger active (e.g. ExploreScene or Bootstrap).
/// </summary>
public class DialogueTriggerByFlag : MonoBehaviour
{
    [Tooltip("When this flag is set to the value below, start the dialogue.")]
    [SerializeField] private string flagKey = "";
    [Tooltip("Trigger when the flag is set to this bool value.")]
    [SerializeField] private bool triggerWhenTrue = true;
    [SerializeField] private string dialogueId = "";
    [Tooltip("If true, only trigger once (per session). If false, triggers every time the flag becomes the target value.")]
    [SerializeField] private bool oneShot = true;

    private bool _hasTriggered;

    private void OnEnable()
    {
        EventBus.Subscribe<FlagChanged>(OnFlagChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<FlagChanged>(OnFlagChanged);
    }

    private void OnFlagChanged(FlagChanged e)
    {
        if (string.IsNullOrEmpty(flagKey) || e.Key != flagKey) return;
        if (oneShot && _hasTriggered) return;
        if (e.Value == null || e.Value.Type != FlagValueType.Bool) return;
        if (e.Value.BoolValue != triggerWhenTrue) return;

        _hasTriggered = true;
        if (string.IsNullOrEmpty(dialogueId)) return;

        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.PendingDialogueId = dialogueId;
        if (gm.StateMachine != null)
            gm.StateMachine.SetState(GameState.Dialogue);
        if (gm.Dialogue != null)
            gm.Dialogue.StartDialogue(dialogueId);
    }
}
