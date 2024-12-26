using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDiedEventContext: IStoppableContext
{
    ICharacter Character;
    ICharacter Instigator;
    public CharacterDiedEventContext(ICharacter character,ICharacter Instigator){
        this.Character = character;
        this.Instigator = Instigator;
    }
    public bool ShouldContinue { get; set; } = true; // Flag to control execution of the chain
}
