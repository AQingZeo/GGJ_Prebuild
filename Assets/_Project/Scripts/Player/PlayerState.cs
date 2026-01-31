using System.Collections.Generic;

/// <summary>
/// PlayerState: inventory only. No level/health/sanity; masks are flags (e.g. mask_00_on).
/// GetCurrentSan() returns constant so ChaosEffect compiles (logic preserved, unused).
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

    /// <summary>Stub for ChaosEffect; feature abandoned. Returns max so chaos never applies.</summary>
    public int GetCurrentSan() => 100;

    public Dictionary<string, object> GetInventory() => _inventory;

    public void AddToInventory(string itemId, object item)
    {
        if (_inventory == null) _inventory = new Dictionary<string, object>();
        _inventory[itemId] = item;
    }

    public void RemoveFromInventory(string itemId) => _inventory?.Remove(itemId);

    public bool HasInInventory(string itemId) => _inventory?.ContainsKey(itemId) ?? false;

    public PlayerStateDataModel Snapshot()
    {
        var ids = _inventory != null ? new List<string>(_inventory.Keys) : new List<string>();
        return new PlayerStateDataModel { inventoryIds = ids };
    }

    public void LoadFromSnapshot(PlayerStateDataModel snapshot)
    {
        if (snapshot == null)
        {
            _state = new PlayerStateDataModel();
            _inventory = new Dictionary<string, object>();
            return;
        }
        _state = new PlayerStateDataModel();
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
