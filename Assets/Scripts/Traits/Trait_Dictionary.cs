using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trait_Dictionary : MonoBehaviour
{
    [OdinSerialize]
    private Dictionary<Trait_Type, Trait_Type> traits;

     public static Trait_Dictionary instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Function compare the bonus value the player gets based on equipment types
    /// </summary>
    /// <param name="attacker">The trait type of the attacker</param>
    /// <param name="defender">The trait type of the defender</param>
    /// <returns>A float modifier</returns>
    public float CompareTraitTypes(Trait_Type attacker, TRAIT_TARGET defender)
    {
        return traits[attacker].TypeBehavior[defender];
    }
}
