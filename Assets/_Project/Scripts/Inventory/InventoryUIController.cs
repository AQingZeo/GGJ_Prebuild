using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameContracts;

/// <summary>
/// Mono UI list (icon+name), selection, visibility only in Explore.
/// Subscribes to InventoryService via GameManager.Instance.Inventory (no Find).
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform listContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private List<ItemDefinition> displayDefinitions = new List<ItemDefinition>();

    [Header("State")]
    [SerializeField] private GameObject panelRoot;

    private List<GameObject> _slots = new List<GameObject>();
    private string _lastSelectedId;

    private InventoryService Inventory => GameManager.Instance != null ? GameManager.Instance.Inventory : null;
    private GameStateMachine StateMachine => GameStateMachine.Instance;

    private void OnEnable()
    {
        if (Inventory != null)
        {
            Inventory.OnInventoryChanged += RefreshList;
            Inventory.OnSelectedItemChanged += OnSelectionChanged;
        }
        EventBus.Subscribe<GameStateChanged>(OnGameStateChanged);
        RefreshList();
        UpdateVisibility();
    }

    private void OnDisable()
    {
        if (Inventory != null)
        {
            Inventory.OnInventoryChanged -= RefreshList;
            Inventory.OnSelectedItemChanged -= OnSelectionChanged;
        }
        EventBus.Unsubscribe<GameStateChanged>(OnGameStateChanged);
    }

    private void OnGameStateChanged(GameStateChanged evt)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        bool explore = StateMachine != null && StateMachine.CurrentState == GameState.Explore;
        GameObject toShowHide = panelRoot != null ? panelRoot : gameObject;
        // Never disable this controller's GameObject, or we unsubscribe and miss the return-to-Explore event
        if (toShowHide == gameObject && transform.childCount > 0)
            toShowHide = transform.GetChild(0).gameObject;
        toShowHide.SetActive(explore);
        if (explore)
            RefreshList();
    }

    private void OnSelectionChanged(string selectedId)
    {
        _lastSelectedId = selectedId;
        RefreshSelectionHighlight();
    }

    private void RefreshList()
    {
        if (listContainer == null || Inventory == null) return;

        foreach (var s in _slots)
        {
            if (s != null) Destroy(s);
        }
        _slots.Clear();

        var items = Inventory.GetAllItems();
        if (items == null) return;

        foreach (var kvp in items)
        {
            string id = kvp.Key;
            int count = Inventory.GetCount(id);
            if (count <= 0) continue;

            GameObject slot = itemSlotPrefab != null
                ? Instantiate(itemSlotPrefab, listContainer)
                : CreateDefaultSlot(listContainer);
            _slots.Add(slot);

            // Single instantiated slot only (not scene search)
            var label = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                var def = GetDisplayDefinition(id);
                label.text = def != null ? def.displayName : id;
                if (count > 1) label.text += " x" + count;
            }

            var icon = slot.GetComponentInChildren<Image>();
            if (icon != null)
            {
                var def = GetDisplayDefinition(id);
                if (def != null && def.icon != null) icon.sprite = def.icon;
            }

            var btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                string captureId = id;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (Inventory != null)
                    {
                        bool alreadySelected = Inventory.SelectedItemId == captureId;
                        if (alreadySelected) Inventory.ClearSelection();
                        else Inventory.SelectItem(captureId);
                    }
                });
            }
        }

        RefreshSelectionHighlight();
    }

    private void RefreshSelectionHighlight()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot == null) continue;
            string id = GetSlotItemId(i);
            bool selected = id == _lastSelectedId;
            var selectable = slot.GetComponent<Selectable>();
            if (selectable != null) selectable.interactable = true;
            var img = slot.GetComponent<Image>();
            if (img != null) img.color = selected ? new Color(0.8f, 0.9f, 1f) : Color.white;
        }
    }

    private string GetSlotItemId(int index)
    {
        if (Inventory == null) return null;
        var items = Inventory.GetAllItems();
        if (items == null) return null;
        int i = 0;
        foreach (var kvp in items)
        {
            if (Inventory.GetCount(kvp.Key) <= 0) continue;
            if (i == index) return kvp.Key;
            i++;
        }
        return null;
    }

    private ItemDefinition GetDisplayDefinition(string itemId)
    {
        foreach (var d in displayDefinitions)
        {
            if (d != null && d.itemId == itemId) return d;
        }
        return null;
    }

    private static GameObject CreateDefaultSlot(Transform parent)
    {
        var go = new GameObject("InventorySlot");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 32);
        var btn = go.AddComponent<Button>();
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = "?";
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return go;
    }
}
