using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plain C# facade over PlayerState inventory.
/// Enforces uniqueness unless ItemDefinition.stackable; selection is runtime-only.
/// </summary>
public class InventoryService
{
    private readonly PlayerState _playerState;
    private readonly IReadOnlyList<ItemDefinition> _itemDefinitions;
    private string _selectedItemId;

    public event Action OnInventoryChanged;
    public event Action<string> OnSelectedItemChanged;

    public InventoryService(PlayerState playerState, IReadOnlyList<ItemDefinition> itemDefinitions = null)
    {
        _playerState = playerState ?? throw new ArgumentNullException(nameof(playerState));
        _itemDefinitions = itemDefinitions ?? new List<ItemDefinition>();
    }

    public void AddItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        var def = GetDefinition(id);
        if (def != null && def.stackable)
        {
            int count = GetCount(id);
            _playerState.RemoveFromInventory(id);
            _playerState.AddToInventory(id, count + 1);
        }
        else
        {
            if (_playerState.HasInInventory(id)) return;
            _playerState.AddToInventory(id, 1);
        }

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        var def = GetDefinition(id);
        if (def != null && def.stackable)
        {
            int count = GetCount(id);
            if (count <= 1)
            {
                _playerState.RemoveFromInventory(id);
            }
            else
            {
                _playerState.RemoveFromInventory(id);
                _playerState.AddToInventory(id, count - 1);
            }
        }
        else
        {
            _playerState.RemoveFromInventory(id);
        }

        if (_selectedItemId == id)
        {
            _selectedItemId = null;
            OnSelectedItemChanged?.Invoke(null);
        }

        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(string id)
    {
        return _playerState.HasInInventory(id);
    }

    public int GetCount(string id)
    {
        if (!_playerState.HasInInventory(id)) return 0;
        var raw = _playerState.GetInventory()[id];
        if (raw is int i) return i;
        return 1;
    }

    public IReadOnlyDictionary<string, object> GetAllItems()
    {
        return _playerState.GetInventory();
    }

    public void SelectItem(string id)
    {
        if (id != null && !_playerState.HasInInventory(id)) id = null;
        if (_selectedItemId == id) return;
        _selectedItemId = id;
        OnSelectedItemChanged?.Invoke(id);
    }

    public void ClearSelection()
    {
        SelectItem(null);
    }

    public string SelectedItemId => _selectedItemId;

    /// <summary>
    /// Call after load to refresh UI (e.g. from SaveManager load path).
    /// </summary>
    public void RaiseInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public bool IsConsumeOnUse(string id)
    {
        var d = GetDefinition(id);
        return d != null && d.consumeOnUse;
    }

    private ItemDefinition GetDefinition(string id)
    {
        if (_itemDefinitions == null) return null;
        foreach (var d in _itemDefinitions)
        {
            if (d != null && d.itemId == id) return d;
        }
        return null;
    }
}
