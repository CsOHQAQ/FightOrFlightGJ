using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttributeSet : AttributeSet
{
    public GameplayAttribute BaseWeaponDamage;
    public GameplayAttribute AttackSpeed;
    public GameplayAttribute MaxAmmo;

    public GameplayAttribute BulletPerShot;
    // --- New attribute for accuracy/stability
    // A float that typically ranges from 0.0 (very inaccurate) up to some higher value
    public GameplayAttribute Accuracy;

    //Should only be overridden. 
    public GameplayAttribute BaseSpreadAngle;
    
}
