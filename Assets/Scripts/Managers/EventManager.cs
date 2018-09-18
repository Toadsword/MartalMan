using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

    public delegate void FreezeAction();
    public static event FreezeAction OnFreeze;

    public static void DoFreeze()
    {
        if (OnFreeze != null)
            OnFreeze();
    }

    public delegate void FreezePause();
    public static event FreezePause OnPause;

    public static void DoPause()
    {
        if (OnPause != null)
            OnPause();
    }

    public delegate void ResetUnsavedAction();
    public static event ResetUnsavedAction OnResetUnsaved;

    public static void ResetUnsaved()
    {
        if (OnResetUnsaved != null)
            OnResetUnsaved();
    }
    

    public delegate void SaveAction();
    public static event SaveAction OnSave;

    public static void SaveCurrentState()
    {
        if (OnSave != null)
            OnSave();
    }
}
