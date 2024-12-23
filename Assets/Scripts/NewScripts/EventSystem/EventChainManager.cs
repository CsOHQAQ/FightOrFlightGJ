using UnityEngine;

/// <summary>
/// EventChainManager: Singleton for managing Attack and Hit event chains.
/// Allows configuration of initial Event Nodes via the Inspector (loaded from ScriptableObjects).
/// </summary>
public class EventChainManager : MonoBehaviour
{
    public static EventChainManager Instance { get; private set; }

    [Header("Initial Node Configuration (Loadable from ScriptableObjects)")]
    public ScriptableEventNode[] initialAttackNodes;
    public ScriptableEventNode[] initialHitNodes;

    public EventChain AttackEventChain { get; private set; }
    public EventChain HitEventChain { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        AttackEventChain = new EventChain();
        HitEventChain = new EventChain();

        // Load initial nodes from ScriptableObjects
        if (initialAttackNodes != null)
        {
            foreach (var nodeAsset in initialAttackNodes)
            {
                var node = nodeAsset.CreateNodeInstance();
                if (node != null)
                    AttackEventChain.AddNode(node);
            }
        }
        //Add Nodes that on default should be added
        HitEventChain.AddNode(new HandleHitReceiverNode());

        if (initialHitNodes != null)
        {
            foreach (var nodeAsset in initialHitNodes)
            {
                var node = nodeAsset.CreateNodeInstance();
                if (node != null)
                    HitEventChain.AddNode(node);
            }
        }
    }

    /// <summary>
    /// Executes the AttackEventChain when an attack is initiated.
    /// </summary>
    public void ExecuteAttackChain(ref EventContext context)
    {
        
        AttackEventChain.Execute(context);
    }

    /// <summary>
    /// Executes the HitEventChain when an attack hits a target.
    /// </summary>
    public void ExecuteHitChain(ref EventContext context)
    {
        HitEventChain.Execute(context);
    }

    /// <summary>
    /// Dynamically adds a node to the chain, e.g., when the player acquires a new item
    /// to add new effects to the Attack or Hit chains.
    /// </summary>
    public void AddNodeToAttackChain(IEventNode node)
    {
        AttackEventChain.AddNode(node);
    }

    public void AddNodeToHitChain(IEventNode node)
    {
        HitEventChain.AddNode(node);
    }

    public void RemoveNodeFromAttackChain(IEventNode node)
    {
        AttackEventChain.RemoveNode(node);
    }

    public void RemoveNodeFromHitChain(IEventNode node)
    {
        HitEventChain.RemoveNode(node);
    }
}
