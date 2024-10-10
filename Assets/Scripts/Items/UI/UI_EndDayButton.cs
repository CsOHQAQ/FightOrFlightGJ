using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_EndDayButton : MonoBehaviour
{
    public void EndDayButtonOnClick()
    {
        Debug.Log("EndDayClicked");
        UI_InventoryManager.Instance.HideCanvas();
        DayManager.Instance.GoToNextDay();
    }
}
