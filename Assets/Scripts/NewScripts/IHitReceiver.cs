using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitReceiver {
    void OnHit(HitInfo hitInfo);
}

public struct HitInfo {
    
    public Vector3 HitPoint;
    public Vector3 HitNormal;
    public object AdditionalData; // Optional field for special cases, or use specialized fields
}