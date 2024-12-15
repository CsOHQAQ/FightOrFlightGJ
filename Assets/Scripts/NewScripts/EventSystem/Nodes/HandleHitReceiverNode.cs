using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleHitReceiverNode : IEventNode
{
    //This should be the first during hit event chain
    public int Priority => -1000;

    public void Process(EventContext context) {
        if (context == null || context.AttackInfo == null || context.HitData == null) return;

        //TODO: Handle Hit VFX or SFX with the respective HitReceiver
        //Break the hit event chain if there is no ICharacter attached. (Could also add logic for hit on intractable objects like explosion barrels)
        if (context.Target is not ICharacter character) {
            context.ShouldContinue = false; // Break the event chain
            Debug.Log("HitReceiver does not implement ICharacter or other valid types. Breaking the event chain.");
            context.Target.OnHit(context.HitData);
        }
    }
}
