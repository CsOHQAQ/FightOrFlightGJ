using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour,IHitReceiver
{
    public void OnHit(HitData hitData)
    {
        Debug.Log("Got Hit on " + hitData.HitInfo.HitPoint);
    }
}
