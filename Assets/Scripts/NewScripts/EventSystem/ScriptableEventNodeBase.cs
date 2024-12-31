using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class ScriptableEventNodeBase : ScriptableObject
{
    public abstract Type GetContextType();
    // no CreateNodeInstance() here because we don't know T
}