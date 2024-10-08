using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GridItem : MonoBehaviour
{
    [SerializeField]
    private Image activeTile;

    [SerializeField]
    private Image itemSprite;

    private int listIndex;

    public void SetupGrid(Sprite sprite, int index)
    {
        itemSprite.sprite = sprite;
        activeTile.enabled = false;
        listIndex = index;
    }

    public void SetActive()
    {
        activeTile.enabled = true;
    }

    public void SetInactive()
    {
        activeTile.enabled = false;
    }

    public int GetListIndex() { return listIndex; }
}
