using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to pass context data related to attacks and hits.
/// In the AttackEventChain, the focus is on AttackInfo, while in the HitEventChain, the focus is on HitInfo.
/// However, for consistency, EventContext can be shared between Attack and Hit executions.
/// </summary>
public class EventContext
{
    public ICharacter Source;   // The Source
    public IHitReceiver Target;     // The target being hit

    public AttackData AttackInfo; // Data related to the attack (type, base damage, ammo type, etc.)
    public HitData HitData;       // Data related to the hit (final damage, hit type, status effects, etc.)

    public bool ShouldContinue { get; set; } = true; // Flag to control execution of the chain
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
public interface IEventNode
{
    int Priority { get; }
    void Process(EventContext context);
}

/// <summary>
/// EventChain class: Contains an ordered list of IEventNode
/// When executed, calls the Process method of all nodes in order of priority.
/// </summary>
public class EventChain
{
    private List<IEventNode> nodes = new List<IEventNode>();

    public void AddNode(IEventNode node)
    {
        nodes.Add(node);
        nodes.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void RemoveNode(IEventNode node)
    {
        nodes.Remove(node);
    }

    public void Execute(EventContext context)
    {
        foreach (var node in nodes)
        {
            if (!context.ShouldContinue) break; // Stop if ShouldContinue is false
            node.Process(context);
        }
    }
}
