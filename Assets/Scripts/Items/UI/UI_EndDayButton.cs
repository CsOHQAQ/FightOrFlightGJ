using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_EndDayButton : MonoBehaviour
{
    public void EndDayButtonOnClick()
    {
        DayManager.Instance.GoToNextDay();
    }
}
