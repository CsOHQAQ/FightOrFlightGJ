using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : BaseItem, IEquipable, IActivatable {
    public event Action<IItem, ICharacter> OnEquipped;
    public event Action<IItem, ICharacter> OnUnequipped;

    public EquipmentSlot SlotType { get; private set; }
    public string AmmoType { get; private set; }

    // 可加上AttackPower、ReloadTime等属性
    private bool isCharging = false;

    public WeaponItem(ItemData data, EquipmentSlot slotType, string ammoType) : base(data) {
        this.SlotType = slotType;
        this.AmmoType = ammoType;
    }

    public void Equip(ICharacter character) {
        OnEquipped?.Invoke(this, character);
    }

    public void Unequip(ICharacter character) {
        OnUnequipped?.Invoke(this, character);
    }

    // IActivatable实现：根据输入进行攻击或模式切换
    public void BeginUse(ICharacter user, ActivationTrigger trigger) {
        if (trigger == ActivationTrigger.LeftMouse) {
            // 左键按下开始蓄力或准备射击
            isCharging = true;
        } else if (trigger == ActivationTrigger.RightMouse) {
            // 右键可能是开镜或切换射击模式
        }
    }

    public void HoldUse(ICharacter user, ActivationTrigger trigger) {
        if (trigger == ActivationTrigger.LeftMouse && isCharging) {
            // 蓄力中（如果是蓄力武器），增加蓄力数值
        }
    }

    public void EndUse(ICharacter user, ActivationTrigger trigger) {
        if (trigger == ActivationTrigger.LeftMouse && isCharging) {
            // 蓄力完毕，松开左键发射子弹
            isCharging = false;
            // 检查AmmoType在角色背包中是否有弹药
            // 消耗一发子弹并对目标或准星方向进行伤害计算
        } 
        // 如果是右键结束则是关闭瞄准
    }

    public void OnScroll(ICharacter user, float scrollDelta) {
        // 滚轮操作可以用于调节射击角度、切换武器模式，或调整蓄力等
        // 例如根据scrollDelta改变武器的某个内部参数
    }
}
