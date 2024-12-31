using System;
using System.Collections.Generic;
using UnityEngine;

// Example: If you want ArtifactItem to also be an IItem, do:
// public class ArtifactItem : IEquipable, IItem
// Then implement the needed IItem properties (Name, Description, Icon, etc.)
public class ArtifactItem : IEquipable
{
    private ArtifactSO data; 
    private GameplayEffectSpecHandle[] effectHandles; 
    private List<object> addedNodes = new List<object>();

    // -------------------------------------------------------------------------
    // 1) Required by IEquipable
    // -------------------------------------------------------------------------
    public event Action<IItem, IPlayerCharacter> OnEquipped;
    public event Action<IItem, IPlayerCharacter> OnUnequipped;

    // We'll store slot type in a settable property rather than a read-only
    public EquipmentSlot SlotType { get; private set; }

    // -------------------------------------------------------------------------
    // 2) Constructor
    // -------------------------------------------------------------------------
    public ArtifactItem(ArtifactSO data, EquipmentSlot slot = EquipmentSlot.Artifact)
    {
        this.data = data;
        // Now we can assign slot to SlotType
        this.SlotType = slot;
    }

    // -------------------------------------------------------------------------
    // 3) Equip method must match signature: void Equip(IPlayerCharacter)
    // -------------------------------------------------------------------------
    public void Equip(IPlayerCharacter character)
    {
        var player = character as PlayerCharacter;
        if (player == null) 
            return;

        // Since your AbilitySystemComponent methods are called e.g. ApplyEffectToSelf
        // you presumably want to do something like:
        var asc = player.GetAbilitySystemComponent();
        if (asc == null)
        {
            Debug.LogWarning("No AbilitySystemComponent found on player.");
            return;
        }

        // 1) Apply all attributeEffects
        // If data.attributeEffects is an array of GameplayEffect
        if (data.attributeEffects != null && data.attributeEffects.Length > 0)
        {
            effectHandles = new GameplayEffectSpecHandle[data.attributeEffects.Length];
            for (int i = 0; i < data.attributeEffects.Length; i++)
            {
                var effect = data.attributeEffects[i];
                if (effect != null)
                {
                    // FIX #2: Call asc.ApplyEffectToSelf(...) or asc.ApplyEffectToTarget(...).
                    // We'll do ApplyEffectToSelf with a default effectLevel=1f (or from data if needed)
                    effectHandles[i] = asc.ApplyEffectToSelf(effect, 1f); 
                }
            }
        }

        // 2) Add event chain nodes
        if (data.eventChainNodes != null && data.eventChainNodes.Length > 0)
        {
            foreach (var chainData in data.eventChainNodes)
            {
                if (chainData.nodeAsset == null) 
                    continue;

                Type contextType = chainData.nodeAsset.GetContextType();

                if (contextType == typeof(EventContext))
                {
                    var typedAsset = chainData.nodeAsset as ScriptableEventNode<EventContext>;
                    if (typedAsset != null)
                    {
                        var runtimeNode = typedAsset.CreateNodeInstance();
                        switch (chainData.chainType)
                        {
                            case EventChainType.Attack:
                                EventChainManager.Instance.AddNodeToAttackChain(runtimeNode);
                                break;
                            case EventChainType.Hit:
                                EventChainManager.Instance.AddNodeToHitChain(runtimeNode);
                                break;
                            // etc.
                        }
                        addedNodes.Add(runtimeNode);
                    }
                }
                else if (contextType == typeof(CharacterDiedEventContext))
                {
                    var typedAsset = chainData.nodeAsset as ScriptableEventNode<CharacterDiedEventContext>;
                    if (typedAsset != null)
                    {
                        var runtimeNode = typedAsset.CreateNodeInstance();
                        if (chainData.chainType == EventChainType.CharacterDied)
                        {
                            EventChainManager.Instance.AddNodeToCharacterDiedChain(runtimeNode);
                        }
                        addedNodes.Add(runtimeNode);
                    }
                }
                else
                {
                    Debug.LogWarning($"ArtifactItem: Unsupported context type {contextType}");
                }
            }
        }

        // 3) Fire the event
        // FIX #3: We must pass an IItem. If ArtifactItem is NOT IItem, we can do:
        OnEquipped?.Invoke(null, character); 
        // or if you want to pass (IItem)this, then you must implement IItem on ArtifactItem.
    }

    // -------------------------------------------------------------------------
    // 4) Unequip method
    // -------------------------------------------------------------------------
    public void Unequip(IPlayerCharacter character)
    {
        var player = character as PlayerCharacter;
        if (player == null) 
            return;

        var asc = player.GetAbilitySystemComponent();
        if (asc == null)
        {
            Debug.LogWarning("No AbilitySystemComponent found on player for remove.");
            return;
        }

        // 1) Remove all attributeEffects
        if (effectHandles != null)
        {
            for (int i = 0; i < effectHandles.Length; i++)
            {
                // FIX #4: effectHandles[i] != null doesn't work if it's a struct.
                // Instead, check HandleID != 0 or something like that:
                if (effectHandles[i].HandleID != 0)
                {
                    // FIX #5: rename to asc.RemoveEffectSpec(effectHandles[i])
                    asc.RemoveEffectSpec(effectHandles[i]);
                }
            }
        }

        // 2) Remove event chain nodes
        foreach (var nodeObj in addedNodes)
        {
            if (nodeObj is IEventNode<EventContext> eNodeEC)
            {
                EventChainManager.Instance.RemoveNodeFromAttackChain(eNodeEC);
                EventChainManager.Instance.RemoveNodeFromHitChain(eNodeEC);
            }
            else if (nodeObj is IEventNode<CharacterDiedEventContext> eNodeCD)
            {
                EventChainManager.Instance.RemoveNodeFromCharacterDiedChain(eNodeCD);
            }
        }
        addedNodes.Clear();

        // 3) Fire the event
        // Similarly, we pass null or cast properly
        OnUnequipped?.Invoke(null, character);
    }
}
