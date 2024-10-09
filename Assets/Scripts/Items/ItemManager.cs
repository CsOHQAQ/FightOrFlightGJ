using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    [SerializeField] private List<Item_ScriptableObject> allItems = new List<Item_ScriptableObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Item_ScriptableObject GetItemByID(int id)
    {
        return allItems.Find(item => item.itemId == id);
    }

    public Item_ScriptableObject GetItemByName(string name)
    {
        return allItems.Find(item => item.itemName == name);
    }

    public List<Item_ScriptableObject> GetAllItems()
    {
        return new List<Item_ScriptableObject>(allItems);
    }
}
