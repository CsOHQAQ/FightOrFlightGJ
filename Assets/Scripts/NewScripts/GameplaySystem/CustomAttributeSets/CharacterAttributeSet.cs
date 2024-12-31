using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttributeSet : AttributeSet
{
    public GameplayAttribute Health;
    public GameplayAttribute MaxHealth;

    //this is for passing on damage or calculating. not damage that the character does. 
    public GameplayAttribute Damage;

}
