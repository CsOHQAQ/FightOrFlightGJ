using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportWall : MonoBehaviour
{
    public GameObject TeleportTarget;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //TODO: Teleport may be disabled by the moving?
            other.transform.position = TeleportTarget.transform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.position= TeleportTarget.transform.position;
        }
    }
}
