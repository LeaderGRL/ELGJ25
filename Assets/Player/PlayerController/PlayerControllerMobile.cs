using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControllerMobile : APlayerController<CrossatroGameActionsMobile>
{
    protected override void RegisterForEvents()
    {
        m_actions.Game.Select.performed += OnSelectPerformed;
        m_actions.Game.Drag.performed += OnDragPerformed;
    }

    protected override void UnregisterForEvents()
    {
        m_actions.Game.Select.performed -= OnSelectPerformed;
        m_actions.Game.Drag.performed -= OnDragPerformed;
    }

    private void OnSelectPerformed(InputAction.CallbackContext a_context)
    {
        Debug.Log("On Select performed");
    }
    
    private void OnDragPerformed(InputAction.CallbackContext a_context)
    {
        var value = a_context.ReadValue<Vector2>();
        Debug.Log($"On Drag performed: {value}");
    }
    
    protected override bool CanAwake()
    {
#if UNITY_ANDROID
        return true;
#endif
        return false;
    }
}
