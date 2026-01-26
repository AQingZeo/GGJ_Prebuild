using System.Collections.Generic;

/// <summary>
/// PlayerState: Plain C# - Get Set Save Load similar to flagmanager
/// </summary>
public class PlayerState
{
    private PlayerStateDataModel _state = new PlayerStateDataModel();

    public void NewGame()
    {
        _state = new PlayerStateDataModel();
    }

    // Get methods
    public int GetLevel() => _state.level;
    public int GetMaxHealth() => _state.maxHealth;
    public int GetCurrentHealth() => _state.currentHealth;
    public int GetCurrentSan() => _state.currentSan;
    public int GetMinSan() => _state.minSan;
    public Dictionary<string, object> GetInventory() => _state.inventory;

    // Set methods
    public void SetLevel(int value) => _state.level = value;
    public void SetMaxHealth(int value) => _state.maxHealth = value;
    public void SetCurrentHealth(int value) => _state.currentHealth = value;
    public void SetCurrentSan(int value) => _state.currentSan = value;
    public void SetMinSan(int value) => _state.minSan = value;

    // Inventory methods
    public void AddToInventory(string itemId, object item)
    {
        if (_state.inventory == null)
            _state.inventory = new Dictionary<string, object>();
        _state.inventory[itemId] = item;
    }

    public void RemoveFromInventory(string itemId)
    {
        _state.inventory?.Remove(itemId);
    }

    public bool HasInInventory(string itemId)
    {
        return _state.inventory?.ContainsKey(itemId) ?? false;
    }

    // Save/Load methods
    public PlayerStateDataModel Snapshot()
    {
        // Create a deep copy for saving
        var snapshot = new PlayerStateDataModel
        {
            level = _state.level,
            maxHealth = _state.maxHealth,
            currentHealth = _state.currentHealth,
            currentSan = _state.currentSan,
            minSan = _state.minSan,
            inventory = _state.inventory != null ? new Dictionary<string, object>(_state.inventory) : new Dictionary<string, object>()
        };
        return snapshot;
    }

    public void LoadFromSnapshot(PlayerStateDataModel snapshot)
    {
        if (snapshot == null)
        {
            _state = new PlayerStateDataModel();
            return;
        }

        _state = new PlayerStateDataModel
        {
            level = snapshot.level,
            maxHealth = snapshot.maxHealth,
            currentHealth = snapshot.currentHealth,
            currentSan = snapshot.currentSan,
            minSan = snapshot.minSan,
            inventory = snapshot.inventory != null ? new Dictionary<string, object>(snapshot.inventory) : new Dictionary<string, object>()
        };
    }
}
