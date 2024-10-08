using System;
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

    [Header("Equipment Item Attributes")]
    [SerializeField]
    private InventoryGrid equipmentGrid;

    private List<GameObject> currentEquipmentItems;

    [Header("Button Attributes")]
    [SerializeField]
    private Button interactButton;

    [SerializeField]
    private TextMeshProUGUI interactButtonText;

    [Header("Display Attributes")]
    [SerializeField]
    private Sprite emptySprite;

    public void Start()
    {
        currentGridItems = new List<GameObject>();
        currentEquipmentItems = new List<GameObject>();
        ShowInventoryCanvas();
        PopulateCanvas();
    }

    public void ShowInventoryCanvas()
    {
        inventoryCanvas.gameObject.SetActive(true);
        itemNameText.text = string.Empty;
        itemDescriptionText.text = string.Empty;
        itemSpriteImage.sprite = emptySprite;
    }

    private void PopulateCanvas()
    {
        PopulateInventoryGrid();
        PopulateEquipmentGrid();
    }

    private void ClearCanvas()
    {
        foreach (GameObject item in currentGridItems)
        {
            Destroy(item);
        }
        currentGridItems.Clear();
        foreach(GameObject item in currentEquipmentItems)
        {
            Destroy(item);
        }
        currentEquipmentItems.Clear();
        itemDescriptionText.text = string.Empty;
        itemNameText.text = string.Empty;
        itemSpriteImage.sprite = emptySprite;
    }

    private void PopulateInventoryGrid()
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
            if (c > columns)
            {
                c = 0;
                r++;
            }

            temp.GetComponent<UI_GridItem>().SetupGrid(list[index].itemSprite, index);
            index++;

            currentGridItems.Add(temp);

        }
    }

    private void PopulateEquipmentGrid()
    {
        Equipment_ScriptableObject[] equipment = FindObjectOfType<InventoryManager>().GetEquipment();

        float width = equipmentGrid.tileSizeWidth;
        float height = equipmentGrid.tileSizeHeight;

        int columns = (int)(equipmentGrid.rectTransform.position.x / width);

        int c = 0;

        foreach (Item_ScriptableObject item in equipment)
        {
            GameObject temp = Instantiate(itemSpritePrefab, equipmentGrid.transform, false);
            temp.transform.localPosition = new Vector3(c * width, 0, 0);
            temp.GetComponent<UI_GridItem>().SetupGrid(equipment[c].itemSprite, c);
            c++;
            currentEquipmentItems.Add(temp);
        }


    }
    /// <summary>
    /// Function to set the new active item, should only be called from the InventoryController
    /// </summary>
    /// <param name="index">Index of the item being passed, should be handled by code over in IC</param>
    /// <param name="isEquipment">false if bag grid, true if the equip grid</param>
    public void SetNewActiveItem(int index, bool isEquipment)
    {
        if (currentSelectedItem != null)
        {
            currentSelectedItem.SetInactive();
        }

        InventoryManager bags = FindObjectOfType<InventoryManager>();

        if (isEquipment)
        {
            if (index >= currentEquipmentItems.Count) { return; }
            currentSelectedItem = currentEquipmentItems[index].GetComponent<UI_GridItem>();

            itemNameText.text = bags.GetEquipment()[currentSelectedItem.GetListIndex()].itemName;
            itemDescriptionText.text = bags.GetEquipment()[currentSelectedItem.GetListIndex()].itemDescription;
            itemSpriteImage.sprite = bags.GetEquipment()[currentSelectedItem.GetListIndex()].itemSprite;

            interactButton.gameObject.SetActive(false);
        }
        else
        {
            if (index >= currentGridItems.Count) { return; }
            currentSelectedItem = currentGridItems[index].GetComponent<UI_GridItem>();

            itemNameText.text = bags.GetBagItems()[currentSelectedItem.GetListIndex()].itemName;
            itemDescriptionText.text = bags.GetBagItems()[currentSelectedItem.GetListIndex()].itemDescription;
            itemSpriteImage.sprite = bags.GetBagItems()[currentSelectedItem.GetListIndex()].itemSprite;

            interactButton.gameObject.SetActive(true);

            switch (bags.GetBagItems()[currentSelectedItem.GetListIndex()].itemType)
            {
                case ITEM_TYPE.EQUIPMENT:
                    interactButtonText.SetText("Equip");
                    break;
                case ITEM_TYPE.CONSUMEABLE:
                    interactButtonText.SetText("Use");
                    break;
                default:
                    interactButtonText.SetText("");
                    break;
            }
        }
        currentSelectedItem.SetActive();

    }

    public void InteractButtonClicked()
    {
       if(currentSelectedItem != null)
        {
            InventoryManager man = FindObjectOfType<InventoryManager>();
            if(man.TryInteractItem(man.GetBagItems()[currentSelectedItem.GetListIndex()]))
            {
                Debug.Log("Made it here");
                currentSelectedItem = null;
                ClearCanvas();
                PopulateCanvas();
            }
        }
    }
}
