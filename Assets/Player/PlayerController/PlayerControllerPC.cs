using System;
using UnityEngine;


public class PlayerControllerPC : APlayerController<CrossatroGameActionsPC>
{
    protected override void RegisterForEvents()
    {
        throw new System.NotImplementedException();
    }

    protected override void UnregisterForEvents()
    {
        throw new System.NotImplementedException();
    }

    protected override bool CanAwake()
    {
#if UNITY_STANDALONE_WIN
        return true;
#endif
        return false;
    }
}
