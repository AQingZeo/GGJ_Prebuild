using System.Collections.Generic;
using GameContracts;

public class FlagManager
{
    private Dictionary<string, FlagValueDto> _flags = new Dictionary<string, FlagValueDto>();

    public void Set(string key, bool value)
    {
        var flagValue = new FlagValueDto
        {
            Type = FlagValueType.Bool,
            BoolValue = value
        };
        _flags[key] = flagValue;
        EventBus.Publish(new FlagChanged(key, flagValue));
    }

    public void Set(string key, int value)
    {
        var flagValue = new FlagValueDto
        {
            Type = FlagValueType.Int,
            IntValue = value
        };
        _flags[key] = flagValue;
        EventBus.Publish(new FlagChanged(key, flagValue));
    }

    public void Set(string key, string value)
    {
        var flagValue = new FlagValueDto
        {
            Type = FlagValueType.String,
            StringValue = value
        };
        _flags[key] = flagValue;
        EventBus.Publish(new FlagChanged(key, flagValue));
    }

    public bool Get(string key, bool defaultValue = false)
    {
        if (_flags.TryGetValue(key, out FlagValueDto value) && value != null && value.Type == FlagValueType.Bool)
            return value.BoolValue;
        return defaultValue;
    }

    public int Get(string key, int defaultValue = 0)
    {
        if (_flags.TryGetValue(key, out FlagValueDto value) && value != null && value.Type == FlagValueType.Int)
            return value.IntValue;
        return defaultValue;
    }

    public string Get(string key, string defaultValue = "")
    {
        if (_flags.TryGetValue(key, out FlagValueDto value) && value != null && value.Type == FlagValueType.String)
            return value.StringValue;
        return defaultValue;
    }

    public bool HasFlag(string key) => _flags.ContainsKey(key);

    public IReadOnlyDictionary<string, FlagValueDto> Snapshot()
    {
        return new Dictionary<string, FlagValueDto>(_flags);
    }

    public void LoadFromSnapshot(Dictionary<string, FlagValueDto> snapshot)
    {
        _flags = new Dictionary<string, FlagValueDto>(snapshot);
    }
}