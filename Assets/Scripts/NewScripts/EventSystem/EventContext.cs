using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to pass context data related to attacks and hits.
/// In the AttackEventChain, the focus is on AttackInfo, while in the HitEventChain, the focus is on HitInfo.
/// However, for consistency, EventContext can be shared between Attack and Hit executions.
/// </summary>
public class EventContext: IStoppableContext
{
    public ICharacter Source;   // The Source
    public IHitReceiver Target;     // The target being hit

    public AttackData AttackInfo; // Data related to the attack (type, base damage, ammo type, etc.)
    public HitData HitData;       // Data related to the hit (final damage, hit type, status effects, etc.)

    public bool ShouldContinue { get; set; } = true; // Flag to control execution of the chain
}

public interface IStoppableContext
{
    bool ShouldContinue { get; set; }
}

/// <summary>
/// Structure for attack data (can be extended as needed)
/// </summary>
[System.Serializable]
public class AttackData
{
    public float BaseDamage = 10f;
    public bool IsCritical = false;
    public string AmmoType = "Normal";

    public GameObject ProjectilePrefab;
    // ... Extend with other fields as needed
}

/// <summary>
/// Structure for hit data (can be extended as needed)
/// </summary>
[System.Serializable]
public class HitData
{
    public float FinalDamage;
    public bool WasCrit;
    public bool IsLethalHit;
    public HitInfo HitInfo;
}

/// <summary>
/// Interface for EventNode: All event nodes must implement this interface
/// </summary>
public interface IEventNode<TContext>
{
    int Priority { get; }
    void Process(TContext context);
}

/// <summary>
/// EventChain class: Contains an ordered list of IEventNode
/// When executed, calls the Process method of all nodes in order of priority.
/// </summary>
public class EventChain<TContext>
{
    private readonly List<IEventNode<TContext>> nodes = new List<IEventNode<TContext>>();

    public void AddNode(IEventNode<TContext> node)
    {
        nodes.Add(node);
        // Sort by priority
        nodes.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void RemoveNode(IEventNode<TContext> node)
    {
        nodes.Remove(node);
    }

    public void Execute(TContext context)
    {
        // If TContext is known to have a `ShouldContinue` field, we can do:
        // or if TContext is "EventContext" specifically:
        EventContext evtCtx = context as EventContext; 
        // or if you used an interface like IStoppableContext, cast it to that.

        foreach (var node in nodes)
        {
            // If TContext is EventContext, do:
            if (evtCtx != null && !evtCtx.ShouldContinue) 
                break;

            node.Process(context);
        }
    }
}

