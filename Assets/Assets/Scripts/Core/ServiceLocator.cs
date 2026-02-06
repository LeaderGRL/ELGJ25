using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Design pattern used to centralize all singleton managers
/// </summary>
public class ServiceLocator: MonoBehaviour
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new();

    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                var instanceLocatorGameObject = new GameObject("[ServiceLocator]");
                _instance = instanceLocatorGameObject.AddComponent<ServiceLocator>();
                DontDestroyOnLoad(instanceLocatorGameObject);
            }
            return _instance;
        }
    }

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(_instance);
    }

    /// <summary>
    /// Register a service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="service"></param>
    public void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered");
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
            Debug.Log($"[ServiceLocator] Registered: {type.Name}");
        }
    }

    /// <summary>
    /// Unregister a service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Unregister<T>() where T : class
    {
        var type = typeof(T);
        if ( _services.Remove(type))
        {
            Debug.Log($"[ServiceLocator] Unregistered: {type.Name}"); ;
        }
    }

    /// <summary>
    /// Get a registered services
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Get<T>() where T : class
    {
        var type = typeof(T);
        if( _services.TryGetValue(type, out var service)
        {
            return service as T;
        }

        Debug.LogError($"[ServiceLocator] Service {type.Name} not found!");
        return null;
    }

    /// <summary>
    /// Clear all services. Can be usefull on scene unload.
    /// </summary>
    public void ClearAll()
    {
        _services.Clear();
    }
}
