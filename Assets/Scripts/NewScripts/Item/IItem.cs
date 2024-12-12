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
    Head,
    Body,
    Hands,
    Legs
}

public interface IEquipable {
    EquipmentSlot SlotType { get; }
    event Action<IItem, ICharacter> OnEquipped;
    event Action<IItem, ICharacter> OnUnequipped;

    void Equip(ICharacter character);
    void Unequip(ICharacter character);
}

public interface IActivatable {
    void BeginUse(ICharacter user, ActivationTrigger trigger);
    void HoldUse(ICharacter user, ActivationTrigger trigger);
    void EndUse(ICharacter user, ActivationTrigger trigger);
    void OnScroll(ICharacter user, float scrollDelta);
}

public interface IAmmo : IItem, IStackable 
{
    string AmmoType { get; }
}
