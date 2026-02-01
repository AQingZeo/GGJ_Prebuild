using System;
using System.Collections.Generic;
using UnityEngine;
using GameContracts;

/// <summary>
/// In-memory storage for consumed interactables and per-interactable state (int).
/// Snapshot/LoadFromSnapshot for save; does not write files.
/// </summary>
[Serializable]
public class InteractableStateEntry
{
    public string id;
    public int state;
}

[Serializable]
public class InteractablesSnapshot
{
    public List<string> consumed = new List<string>();
    public List<InteractableStateEntry> states = new List<InteractableStateEntry>();
}

public class InteractableSaveService
{
    private readonly HashSet<string> _consumed = new HashSet<string>();
    private readonly Dictionary<string, int> _states = new Dictionary<string, int>();

    public bool IsConsumed(string id)
    {
        return id != null && _consumed.Contains(id);
    }

    public void Consume(string id)
    {
        if (!string.IsNullOrEmpty(id))
            _consumed.Add(id);
    }

    public int GetState(string id, int defaultState = 0)
    {
        return id != null && _states.TryGetValue(id, out int s) ? s : defaultState;
    }

    public void SetState(string id, int state)
    {
        if (!string.IsNullOrEmpty(id))
        {
            _states[id] = state;
            EventBus.Publish(new InteractableStateChanged(id, state));
        }
    }

    public InteractablesSnapshot Snapshot()
    {
        var snap = new InteractablesSnapshot
        {
            consumed = new List<string>(_consumed),
            states = new List<InteractableStateEntry>()
        };
        foreach (var kvp in _states)
            snap.states.Add(new InteractableStateEntry { id = kvp.Key, state = kvp.Value });
        return snap;
    }

    public void LoadFromSnapshot(InteractablesSnapshot snapshot)
    {
        _consumed.Clear();
        _states.Clear();
        if (snapshot?.consumed != null)
        {
            foreach (var id in snapshot.consumed)
                _consumed.Add(id);
        }
        if (snapshot?.states != null)
        {
            foreach (var e in snapshot.states)
            {
                if (!string.IsNullOrEmpty(e.id))
                    _states[e.id] = e.state;
            }
        }
    }

    public void NewGame()
    {
        _consumed.Clear();
        _states.Clear();
    }
}
