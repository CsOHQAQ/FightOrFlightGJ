using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{

    [HideInInspector]
    public InventoryGrid selectedInventoryGrid;

    private InventoryManager inventoryManager;

    private UI_InventoryManager inventoryManagerUI;

    private void Start()
    {
        if (inventoryManager == null && inventoryManagerUI == null)
        {
            inventoryManager = InventoryManager.Instance;
            inventoryManagerUI = UI_InventoryManager.Instance;
        }
    }

    private void Update()
    {
        if(selectedInventoryGrid == null) 
        {
            return; 
        }

        if (Input.GetMouseButton(0))
        {
            //Debug.Log(inventoryManager.GetBagItems()[(int)(selectedInventoryGrid.GetTileGridPosition(Input.mousePosition).magnitude)].itemName);
            //Debug.Log(selectedInventoryGrid.GetTileIndex(Input.mousePosition));
            inventoryManagerUI.SetNewActiveItem(selectedInventoryGrid.GetTileIndex(Input.mousePosition), selectedInventoryGrid.isEquip);
        }
    }

}
