using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateConfig : MonoBehaviour
{
    private static StateConfig instance;
    public static StateConfig Instance => instance;

    public GameplayEffect WalkDebuffEffect;

    private void Awake()
    {
        if(instance!= null && instance!=this)
        {
            Debug.LogWarning("Multiple singletons found. Destroying the new one.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
