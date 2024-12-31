using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System;

[CreateAssetMenu(fileName="NewArtifact", menuName="Artifacts/Artifact")]
public class ArtifactSO : ScriptableObject
{
    [Header("Basic Info")]
    public Sprite icon;
    public string displayName;
    [TextArea] public string description;

    [Header("Gameplay Effects")]
    public GameplayEffect[] attributeEffects; 
    // store multiple GameplayEffects

    [Header("Event Chain Nodes")]
    public EventChainNodeData[] eventChainNodes; 
    // an array of references specifying which chain + nodeAsset

    // Additional data if needed...
}

public enum EventChainType
{
    Attack,
    Hit,
    CharacterDied
    // or any other chain in your manager
    //For each new type, need to go to the ArtifactItem class and add switch case for calling the right "add to event chain" function
}

[Serializable]
public class EventChainNodeData
{
    public ScriptableEventNodeBase nodeAsset;  
    // We'll define a non-generic base below
    public EventChainType chainType;
}
