using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenChest : InteractableObject
{
    [SerializeField] bool randomize = true;
    [SerializeField] List<Item_ScriptableObject> chestItems;
    [SerializeField] private int maxCount = 5;

    public bool isOpen = false;
    public bool CanOpen = true;

    private float chestOpeness = 0f; // Range from 0 (closed) to 1 (fully open)

    private void Awake()
    {
        if (randomize)
        {
            PopulateChest();
        }

    }

    public override void Interact(object args = null)
    {
        if (args != null && args is InteractDetect)
        {
            InteractDetect interactDetect = (InteractDetect)args;
            chestOpeness = Mathf.Clamp(interactDetect.ScrollValue, 0f, 1f); // Clamps openness between 0 and 1
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
                int randomIndex = Random.Range(0, ItemManager.Instance.GetAllItems().Count);
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
}
