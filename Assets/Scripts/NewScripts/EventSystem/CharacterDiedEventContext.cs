using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDiedEventContext: IStoppableContext
{
    ICharacter Character;
    ICharacter Instigator;
    
    public bool ShouldContinue { get; set; } = true; // Flag to control execution of the chain
}
