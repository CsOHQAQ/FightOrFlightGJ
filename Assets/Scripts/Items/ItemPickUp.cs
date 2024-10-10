using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] public Item_ScriptableObject item;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = item.itemSprite;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager.Instance.AddItemToBag(item);

            Destroy(gameObject);
        }
    }
}
