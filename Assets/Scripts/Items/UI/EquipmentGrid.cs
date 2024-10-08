using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentGrid : MonoBehaviour
{
    [SerializeField]
    public float tileSizeWidth = 32;

    [SerializeField]
    public float tileSizeHeight = 32;

    public RectTransform rectTransform;

    Vector2 positionOntheGrid = new Vector2();
    Vector2Int tileGridPosition = new Vector2Int();

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public Vector2Int GetTileGridPosition(Vector2 mousePosition)
    {
        positionOntheGrid.x = mousePosition.x - rectTransform.position.x;
        positionOntheGrid.y = rectTransform.position.y - mousePosition.y;

        tileGridPosition.x = (int)(positionOntheGrid.x / tileSizeWidth);
        tileGridPosition.y = (int)(positionOntheGrid.y / tileSizeHeight);

        return tileGridPosition;
    }

    public int GetTileIndex(Vector2 mousePosition)
    {
        positionOntheGrid.x = mousePosition.x - rectTransform.position.x;
        positionOntheGrid.y = rectTransform.position.y - mousePosition.y;

        tileGridPosition.x = (int)(positionOntheGrid.x / tileSizeWidth);
        tileGridPosition.y = (int)(positionOntheGrid.y / tileSizeHeight);

        return tileGridPosition.x + tileGridPosition.y * 4;
    }
}
