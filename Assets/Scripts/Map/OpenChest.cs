using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenChest : InteractableObject
{
    [SerializeField] bool randomize;
    [SerializeField] List<Item_ScriptableObject> chestItems;
    [SerializeField] private int maxCount = 5;

    public bool isOpen = false;
    public bool CanOpen = true;

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
        }
    }

    private void PopulateChest()
    {
        chestItems = new List<Item_ScriptableObject>();

        for (int currentItem = 0; currentItem < maxCount; currentItem++)
        {
            if (currentItem == 0)
            {

            }

            int randomIndex = Random.Range(0, ItemManager.Instance.GetAllItems().Count);
            Item_ScriptableObject randomItem = ItemManager.Instance.GetItemByID(randomIndex);

            chestItems.Add(randomItem);
        }
    }
}
