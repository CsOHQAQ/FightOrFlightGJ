using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour,IHitReceiver
{
    public void OnHit(HitInfo hitInfo)
    {
        Debug.Log("Got Hit on " + hitInfo.HitPoint);
    }
}
