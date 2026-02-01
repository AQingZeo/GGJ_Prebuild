using UnityEngine;
using GameContracts;

/// <summary>
/// Handles "Use Item" mode: when an item is selected and user clicks in world,
/// routes to IItemUseTarget on clicked object. Consumes item when consumeOnUse and accepted.
/// Uses InputRouter (EventBus) and raycast; no FindObjectOfType.
/// </summary>
public class ItemUseController : MonoBehaviour
{
    [Header("Optional feedback")]
    [SerializeField] private TMPro.TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 2f;

    private float _feedbackHideTime;
    private Camera _mainCam;

    private InventoryService Inventory => GameManager.Instance != null ? GameManager.Instance.Inventory : null;
    private GameStateMachine StateMachine => GameStateMachine.Instance;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<InputIntentEvent>(OnInputIntent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<InputIntentEvent>(OnInputIntent);
    }

    private void Update()
    {
        if (feedbackText != null && feedbackText.gameObject.activeSelf && Time.time > _feedbackHideTime)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    private void OnInputIntent(InputIntentEvent evt)
    {
        if (StateMachine == null || StateMachine.CurrentState != GameState.Explore) return;
        if (Inventory == null || string.IsNullOrEmpty(Inventory.SelectedItemId)) return;
        if (!evt.ClickDown) return;

        if (_mainCam == null) _mainCam = Camera.main;
        if (_mainCam == null) return;

        Vector2 world = _mainCam.ScreenToWorldPoint(evt.PointerScreen);
        var hit = Physics2D.Raycast(world, Vector2.zero);
        if (!hit.collider) return;

        var target = hit.collider.GetComponent<IItemUseTarget>();
        if (target == null) return;

        string itemId = Inventory.SelectedItemId;
        bool accepted = target.TryUseItem(itemId);
        if (accepted)
        {
            if (ShouldConsume(itemId))
            {
                Inventory.RemoveItem(itemId);
            }
            Inventory.ClearSelection();
            ShowFeedback("Used " + itemId);
        }
        else
        {
            ShowFeedback("Can't use here.");
        }
    }

    private bool ShouldConsume(string itemId)
    {
        return Inventory != null && Inventory.IsConsumeOnUse(itemId);
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);
            _feedbackHideTime = Time.time + feedbackDuration;
        }
    }
}
