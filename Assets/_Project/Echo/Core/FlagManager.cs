using System.Collections.Generic;
using UnityEngine;
using GameContracts; 

public class FlagManager : MonoBehaviour
{

    private Dictionary<string, object> _flags = new Dictionary<string, object>();

 
    public void SetFlag(string key, object value)
    {
        _flags[key] = value;
        Debug.Log($"<color=cyan>【Flag】标记更新: {key} = {value}</color>");


        EventBus.Publish(new FlagChanged(key, value));
    }

    public T GetFlag<T>(string key, T defaultValue = default)
    {
        if (_flags.TryGetValue(key, out object value))
        {
            return (T)value;
        }
        return defaultValue;
    }

    public bool HasFlag(string key) => _flags.ContainsKey(key);
}