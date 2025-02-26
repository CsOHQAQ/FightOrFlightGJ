using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateSetting : MonoBehaviour
{
    private static StateSetting instance;
    public static StateSetting Instance => instance;

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
