using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableItem : BaseItem, IStackable, IActivatable {
    
    private int currentStack;
    public int CurrentStackCount {
        get => currentStack;
        set => currentStack = Math.Min(value, data.MaxStackCount);
    }

    public int MaxStackCount => data.MaxStackCount;

    public ConsumableItem(ItemData data, int initialCount = 1) : base(data) {
        CurrentStackCount = initialCount;
    }

    public bool CanStackWith(IItem other) {
        return other is ConsumableItem c && c.data.ID == this.data.ID;
    }

    // IActivatable实现
    public void BeginUse(ICharacter user, ActivationTrigger trigger) {
        // 消耗品的使用可能只需要在按下瞬间触发即可
        // 减少一单位堆叠
        CurrentStackCount--;
        // 触发事件或效果（可用事件系统或直接调用user.AddHealth(...)）
        // 如果要事件化，可使用OnUsed事件（在此设计中可新增IEventItem接口或直接在此调用）
    }

    public void HoldUse(ICharacter user, ActivationTrigger trigger) {
        // 对消耗品或许不需要实现长按逻辑，留空即可
    }

    public void EndUse(ICharacter user, ActivationTrigger trigger) {
        // 不需要特殊结束逻辑
    }

    public void OnScroll(ICharacter user, float scrollDelta) {
        // 若此消耗品不需滚轮操作，可以不实现或留空
    }
}
