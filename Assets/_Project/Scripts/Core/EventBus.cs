using System;
using System.Collections.Generic;
using GameContracts;

public static class EventBus
{
    private static readonly Dictionary<Type, List<object>> _subscribers = new Dictionary<Type, List<object>>();

    public static void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        Type type = typeof(T);
        if (!_subscribers.ContainsKey(type)) _subscribers[type] = new List<object>();
        _subscribers[type].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        Type type = typeof(T);
        if (_subscribers.ContainsKey(type)) _subscribers[type].Remove(handler);
    }

    public static void Publish<T>(T eventData) where T : IEvent
    {
        Type type = typeof(T);
        if (_subscribers.ContainsKey(type))
        {
            var handlers = new List<object>(_subscribers[type]);
            foreach (var handler in handlers)
            {
                ((Action<T>)handler)?.Invoke(eventData);
            }
        }
    }
}