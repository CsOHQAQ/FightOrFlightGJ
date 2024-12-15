using UnityEngine;

/// <summary>
/// Base definition for ScriptableObject event nodes.
/// Returns an IEventNode instance through CreateNodeInstance().
/// Actual logic can be implemented in derived classes.
/// </summary>
public abstract class ScriptableEventNode : ScriptableObject
{
    [SerializeField] private int priority;
    public int Priority => priority;

    /// <summary>
    /// This method should be implemented by derived classes to generate an IEventNode instance
    /// based on the internal configuration of the ScriptableObject.
    /// </summary>
    public abstract IEventNode CreateNodeInstance();
}
