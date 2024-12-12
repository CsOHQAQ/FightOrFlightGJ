using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    // Implementation of IActivatable
    public void BeginUse(ICharacter user, ActivationTrigger trigger) {
        // The usage of consumables might only need to be triggered instantly upon pressing.
        // Decrease the stack count by one.
        CurrentStackCount--;
        // Trigger an event or effect (use an event system or directly call methods like user.AddHealth(...)).
        // If event-based behavior is needed, you can use an OnUsed event (in this design, you could add an IEventItem interface or directly invoke it here).
    }

    public void HoldUse(ICharacter user, ActivationTrigger trigger) {
        // Consumables may not need to implement a hold logic; leave empty if not required.
    }

    public void EndUse(ICharacter user, ActivationTrigger trigger) {
        // No special logic required for ending usage.
    }

    public void OnScroll(ICharacter user, float scrollDelta) {
        // If this consumable doesn't require scroll wheel functionality, this can remain unimplemented or empty.
    }
}
