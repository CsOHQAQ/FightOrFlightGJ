using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public Equipment_ScriptableObject item;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager.Instance.AddItemToBag(item);

            Destroy(gameObject);
        }
    }
}
