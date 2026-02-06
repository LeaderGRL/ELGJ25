using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralize event system
/// Subscribe to event by type
/// </summary>
public class EventBus : MonoBehaviour
{
    private static EventBus _instance;
    private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();

    public static EventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                var instanceEventBusGameObject = new GameObject("[EventBus]");
                _instance = instanceEventBusGameObject.AddComponent<EventBus>();
                DontDestroyOnLoad(instanceEventBusGameObject);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Subscribe from an event type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        var type = typeof(T);
        if (!_eventHandlers.ContainsKey(type))
            _eventHandlers[type] = new List<Delegate>();

        _eventHandlers[type].Add(handler);
    }

    /// <summary>
    /// Unsubscribe from an event type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        var type = typeof(T);
        if (_eventHandlers.TryGetValue(type, out var handlers))
            handlers.Remove(handler);
    }

    public void Publish<T>(T eventData) where T : struct
    {
        var type = typeof(T);
        if (_eventHandlers.TryGetValue(type, out var handlers))
        {
            // Create a copy to avoid modification during iteration
            var handlerCopy = new List<Delegate>(handlers);
            foreach (var handler in handlerCopy)
            {
                try
                {
                    (handler as Action<T>)?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Error invoking handler for {type.Name}: {e}");
                }
            }
        }
    }

    /// <summary>
    /// Clear all handlers for a specific event type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ClearHandlers<T>() where T : struct
    {
        var type = typeof(T);
        if (_eventHandlers.ContainsKey(type))
        {
            _eventHandlers.Remove(type);
        }
    }

    /// <summary>
    /// Clear all event handlers
    /// </summary>
    public void ClearAllHandlers()
    {
        _eventHandlers.Clear();
    }

    private void OnDestroy()
    {
        ClearAllHandlers();
    }
}
