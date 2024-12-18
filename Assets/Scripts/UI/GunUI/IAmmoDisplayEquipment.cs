using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAmmoDisplayEquipment 
{
    bool ShowCrosshair { get; }
    //Sprite CrosshairSprite { get; }
    bool ShowAmmoInfo { get; }
    int CurrentMagazineAmmo { get; }
    int MaxMagazineAmmo { get; }
    bool IsReloading { get; }
    float CurrentLoadingPercentage{get;}
}
