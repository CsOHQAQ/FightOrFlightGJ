using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenChest : InteractableObject
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;

    [SerializeField] bool randomize = true;
    [SerializeField] List<Item_ScriptableObject> chestItems;
    [SerializeField] private int maxCount = 5;
    [SerializeField] private float detectionDistance = 1.5f;

    private GameObject spawnedItem;

    private bool isOpen = false;
    private bool bombActive = false;

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

        for (int currentItem = 0; currentItem < maxCount; currentItem++)
        {
            if (currentItem == 0)
            {
                Item_ScriptableObject bombItem = ItemManager.Instance.GetItemByName("bomb");

                chestItems.Add(bombItem);
            }
            else
            {
                int randomIndex = Random.Range(0, ItemManager.Instance.GetAllItems().Count - 1);
                Item_ScriptableObject randomItem = ItemManager.Instance.GetItemByID(randomIndex);

                chestItems.Add(randomItem);
            }
        }

        ShuffleChest();
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

            spawnedItem = Instantiate(itemPrefab, transform.position + Vector3.up, itemRotation);

            SpriteRenderer spriteRenderer = spawnedItem.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                Item_ScriptableObject itemData = chestItems[0];
                spriteRenderer.sprite = itemData.itemSprite;

                if (itemData.itemName == "bomb")
                {
                    bombActive = true;
                }
                else
                {
                    PutItemInInventory(itemData);
                }
            }
            chestItems.RemoveAt(0);
        }
    }

    private void PutItemInInventory(Item_ScriptableObject _itemToAdd)
    {
        InventoryManager.Instance.AddItemToBag(_itemToAdd);

        Destroy(spawnedItem, 2.0f);
    }

    private IEnumerator CloseChestAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        isOpen = false;
        UpdateChestSprite();
    }
}
