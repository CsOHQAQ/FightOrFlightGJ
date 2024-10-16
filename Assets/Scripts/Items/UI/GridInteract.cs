using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventoryGrid))]
public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryController inventoryController;

    [SerializeField]
    InventoryGrid inventoryGrid;

    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryController.selectedInventoryGrid = inventoryGrid;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryController.selectedInventoryGrid = null;
    }

    private void OnEnable()
    {
        inventoryController = FindObjectOfType(typeof(InventoryController)) as InventoryController;
        inventoryGrid = GetComponent<InventoryGrid>();
    }
}
