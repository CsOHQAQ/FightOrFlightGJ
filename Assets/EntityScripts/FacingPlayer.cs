using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingPlayer : MonoBehaviour
{
    public bool CanFacing=true;

    private void Update()
    {
        if (GameControl.Game.Player != null)
        {
            Vector3 direction = GameControl.Game.Player.transform.position - transform.position;
            float angle =-1* (Vector2.SignedAngle(Vector2.right, new Vector2(direction.x, direction.z)) + 90);
            transform.localEulerAngles = new Vector3(0, angle, 0);
        }

    }
}
