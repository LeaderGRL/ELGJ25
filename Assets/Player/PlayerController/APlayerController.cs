using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class APlayerController<T>  : MonoBehaviour where T : IInputActionCollection, new()
{
    protected T m_actions = default(T);

    private void Awake()
    {
        if (!CanAwake())
        {
            Destroy(this);
            return;
        }
        m_actions = new T();
        m_actions.Enable();
    }

    private void OnEnable()
    {
        if (m_actions == null)
        {
            return;
        }
        RegisterForEvents();
    }

    private void OnDisable()
    {
        if (m_actions == null)
        {
            return;
        }
        UnregisterForEvents();
    }

    protected abstract void RegisterForEvents();
    protected abstract void UnregisterForEvents();
    protected abstract bool CanAwake();
}
