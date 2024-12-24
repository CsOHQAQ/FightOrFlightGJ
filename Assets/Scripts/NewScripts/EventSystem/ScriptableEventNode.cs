using UnityEngine;

/// <summary>
/// A generic ScriptableObject base that defines a node for a particular TContext.
/// 
/// TContext is the type of event context (e.g. AttackContext, HitContext, EnemyDiedContext).
/// </summary>
/// <typeparam name="TContext">The context type for this node (e.g., AttackContext).</typeparam>
public abstract class ScriptableEventNode<TContext> : ScriptableObject
{
    [SerializeField] private int priority;
    public int Priority => priority;

    /// <summary>
    /// Derived classes should implement this to generate an IEventNode<TContext> instance
    /// based on internal configuration.
    /// </summary>
    public abstract IEventNode<TContext> CreateNodeInstance();
}
