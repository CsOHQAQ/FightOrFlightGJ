using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class OpenChest : InteractableObject
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;

    [SerializeField] bool randomize = true;
    [SerializeField] List<Item_ScriptableObject> chestItems;
    [SerializeField] private int maxCount = 5;
    [SerializeField] private Vector3 bombIncreaseSize = new Vector3(0.2f, 0.2f, 0.2f);

    private GameObject spawnedItem;

    private bool isOpen = false;
    [HideInInspector] public bool bombActive = false;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (randomize)
        {
            PopulateChest();
        }

        UpdateChestSprite();

    }

    public override void Interact(object args = null)
    {
        if (!isOpen)
        {
            isOpen = true;
            UpdateChestSprite();
            SpawnItem();

            if (!bombActive && chestItems.Count != 0)
            {
                StartCoroutine(CloseChestAfterDelay(2.5f));
            }
        }
    }

    private void PopulateChest()
    {
        chestItems = new List<Item_ScriptableObject>();
        int bombId = ItemManager.Instance.GetIndexByName("bomb");
        if (maxCount > 1)
        {
            for (int currentItem = 0; currentItem < maxCount; currentItem++)
            {
                if (currentItem == 0)
                {
                    Item_ScriptableObject bombItem = ItemManager.Instance.GetItemByName("bomb");

                    chestItems.Add(bombItem);
                }
                else
                {
                    int randomIndex = 0;
                    do
                    {
                   
                        randomIndex = Random.Range(0, ItemManager.Instance.GetAllItems().Count - 1);


                    } while (randomIndex== bombId);
                    
                    Item_ScriptableObject randomItem = ItemManager.Instance.GetItemByID(randomIndex);

                    if (randomItem == null)
                    {
                        maxCount++;
                        continue;
                    }

                    chestItems.Add(randomItem);
           
                }
            }

            ShuffleChest();
        }
        else
        {
            int randomIndex = Random.Range(0, ItemManager.Instance.GetAllItems().Count);
            Item_ScriptableObject randomItem = ItemManager.Instance.GetItemByID(randomIndex);

            chestItems.Add(randomItem);
        }

    }

    private void ShuffleChest()
    {
        for (int currentItem = chestItems.Count - 1; currentItem > 0; currentItem--)
        {
            int randomIndex = Random.Range(0, currentItem + 1);

            Item_ScriptableObject tempChestItem = chestItems[currentItem];
            chestItems[currentItem] = chestItems[randomIndex];
            chestItems[randomIndex] = tempChestItem;
        }
    }

    private void UpdateChestSprite()
    {
        spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
    }

    private void SpawnItem()
    {
        if (chestItems.Count > 0 && itemPrefab != null)
        {
            Quaternion itemRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            spawnedItem = Instantiate(itemPrefab, transform.position + Vector3.up * 0.8f, itemRotation);

            SpriteRenderer spriteRenderer = spawnedItem.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                Item_ScriptableObject itemData = chestItems[0];
                spriteRenderer.sprite = itemData.itemSprite;

                if (itemData.itemName == "bomb")
                {
                    spawnedItem.transform.localScale += bombIncreaseSize;
                    bombActive = true;

                    ImplementBombItem(spawnedItem);
                }
                else
                {
                    PutItemInInventory(itemData);


                    // Ensure spawnedItem is still valid before destroying it
                    if (spawnedItem != null)
                    {
                        Destroy(spawnedItem, 2.0f);
                    }
                    else
                    {
                        Debug.Log("spawnedItem is already null or destroyed.");
                    }
                }
            }
            chestItems.RemoveAt(0);
        }
    }

    private void ImplementBombItem(GameObject _theItem)
    {
        Bomb bomb = _theItem.GetComponent<Bomb>();

        if (bomb != null)
        {
            bomb.Chest = gameObject;

            BoxCollider boxCollider = _theItem.GetComponent<BoxCollider>();

            if (boxCollider != null)
            {
                boxCollider.size = new Vector3(8.0f, 8.0f, 14.0f);
            }
            else
            {
                Debug.LogWarning("No BoxCollider found on the item");
            }
        }
        else
        {
            Debug.LogWarning("No Bomb Component found on the item");
        }
    }

    private void PutItemInInventory(Item_ScriptableObject _itemToAdd)
    {
        InventoryManager.Instance.AddItemToBag(_itemToAdd);
    }

    private IEnumerator CloseChestAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        isOpen = false;
        UpdateChestSprite();
    }
}
