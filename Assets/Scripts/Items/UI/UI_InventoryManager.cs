using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryManager : MonoBehaviour
{

    [Header("Grid Attributes")]

    [SerializeField]
    private Canvas inventoryCanvas;

    [SerializeField]
    private InventoryGrid inventoryGrid;

    [SerializeField]
    private GameObject itemSpritePrefab;

    [Header("Active Item Attributes")]

    [SerializeField]
    private TextMeshProUGUI itemNameText;

    [SerializeField]
    private TextMeshProUGUI itemDescriptionText;

    [SerializeField]
    private Image itemSpriteImage;

    private UI_GridItem currentSelectedItem;

    private List<GameObject> currentGridItems;

    public void Start()
    {
        currentGridItems = new List<GameObject>();
        ShowInventoryCanvas();
        PopulateCanvas();
    }

    public void ShowInventoryCanvas()
    {
        inventoryCanvas.gameObject.SetActive(true);
    }

    private void PopulateCanvas()
    {

        currentGridItems.Clear();

        List<Item_ScriptableObject> list = FindObjectOfType<InventoryManager>().GetBagItems();

        float width = inventoryGrid.tileSizeWidth;
        float height = inventoryGrid.tileSizeHeight;

        int columns = (int)(inventoryGrid.rectTransform.position.x / width);
        int rows = (int)(inventoryGrid.rectTransform.position.y / height);

        int c = 0;
        int r = 0;
        int index = 0;

        foreach (Item_ScriptableObject item in list)
        {
            GameObject temp = Instantiate(itemSpritePrefab, inventoryGrid.transform, false);
            temp.transform.localPosition = new Vector3(c * width, -r * height, 0);
            c++;
            if(c > columns)
            {
                c = 0;
                r++;
            }

            temp.GetComponent<UI_GridItem>().SetupGrid(list[index].itemSprite, index);
            index++;

            currentGridItems.Add(temp);

        }
    }

    public void SetNewActiveItem(int index)
    {
        if (currentSelectedItem != null)
        {
            currentSelectedItem.SetInactive();
        }
        currentSelectedItem = currentGridItems[index].GetComponent<UI_GridItem>();

        currentSelectedItem.SetActive();

        InventoryManager bags = FindObjectOfType<InventoryManager>();
        itemNameText.text = bags.GetBagItems()[currentSelectedItem.GetListIndex()].itemName;
        itemDescriptionText.text = bags.GetBagItems()[currentSelectedItem.GetListIndex()].itemDescription;
    }
}
