using System.Collections.Generic;

/// <summary>
/// PlayerState: Plain C# - Get Set Save Load similar to flagmanager
/// </summary>
public class PlayerState
{
    private PlayerStateDataModel _state = new PlayerStateDataModel();
    private Dictionary<string, object> _inventory = new Dictionary<string, object>();

    public void NewGame()
    {
        _state = new PlayerStateDataModel();
        _inventory = new Dictionary<string, object>();
    }

    // Get methods
    public int GetLevel() => _state.level;
    public int GetMaxHealth() => _state.maxHealth;
    public int GetCurrentHealth() => _state.currentHealth;
    public int GetCurrentSan() => _state.currentSan;
    public int GetMinSan() => _state.minSan;
    public Dictionary<string, object> GetInventory() => _inventory;

    // Set methods
    public void SetLevel(int value) => _state.level = value;
    public void SetMaxHealth(int value) => _state.maxHealth = value;
    public void SetCurrentHealth(int value) => _state.currentHealth = value;
    public void SetCurrentSan(int value) => _state.currentSan = value;
    public void SetMinSan(int value) => _state.minSan = value;

    // Inventory methods
    public void AddToInventory(string itemId, object item)
    {
        if (_inventory == null) _inventory = new Dictionary<string, object>();
        _inventory[itemId] = item;
    }

    public void RemoveFromInventory(string itemId) => _inventory?.Remove(itemId);

    public bool HasInInventory(string itemId) => _inventory?.ContainsKey(itemId) ?? false;

    // Save/Load methods (items are pickup-once; no count in save)
    public PlayerStateDataModel Snapshot()
    {
        var ids = _inventory != null ? new List<string>(_inventory.Keys) : new List<string>();
        return new PlayerStateDataModel
        {
            level = _state.level,
            maxHealth = _state.maxHealth,
            currentHealth = _state.currentHealth,
            currentSan = _state.currentSan,
            minSan = _state.minSan,
            inventoryIds = ids
        };
    }

    public void LoadFromSnapshot(PlayerStateDataModel snapshot)
    {
        if (snapshot == null)
        {
            _state = new PlayerStateDataModel();
            _inventory = new Dictionary<string, object>();
            return;
        }
        _state = new PlayerStateDataModel
        {
            level = snapshot.level,
            maxHealth = snapshot.maxHealth,
            currentHealth = snapshot.currentHealth,
            currentSan = snapshot.currentSan,
            minSan = snapshot.minSan
        };
        _inventory = new Dictionary<string, object>();
        if (snapshot.inventoryIds != null)
        {
            foreach (var id in snapshot.inventoryIds)
            {
                if (!string.IsNullOrEmpty(id))
                    _inventory[id] = 1;
            }
        }
    }
}
