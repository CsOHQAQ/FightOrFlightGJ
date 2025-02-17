using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum ActivationTrigger {
    LeftMouse,
    RightMouse,
    MiddleMouse,
    KeyboardKey,

}

public interface IItem
{
    string Name { get; }
    string Description { get; }
    string Icon { get; }
    float Weight { get; }
    int Value { get; }

    string GetItemInfo();
}

public interface IStackable {
    int CurrentStackCount { get; set; }
    int MaxStackCount { get; }
    bool CanStackWith(IItem other);
}

public enum EquipmentSlot {
    Weapon,
    Artifact
}

public interface IEquipable {
    EquipmentSlot SlotType { get; }
    event Action<IItem, IPlayerCharacter> OnEquipped;
    event Action<IItem, IPlayerCharacter> OnUnequipped;

    void Equip(IPlayerCharacter character);
    void Unequip(IPlayerCharacter character);
}

public interface IActivatable {

    bool CanActivate();
    void BeginUse(IPlayerCharacter user, ActivationTrigger trigger);
    void HoldUse(IPlayerCharacter user, ActivationTrigger trigger);
    void EndUse(IPlayerCharacter user, ActivationTrigger trigger);
    void OnScroll(IPlayerCharacter user, float scrollDelta);


}

public interface IAmmo : IItem, IStackable 
{
    string AmmoType { get; }
}
