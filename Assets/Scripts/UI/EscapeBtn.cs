using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EscapeBtn : MonoBehaviour
{
    public Button btn;
    void Start()
    {
        btn=GetComponent<Button>();
        btn.onClick.AddListener(Escape);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Escape()
    {
        DayManager.Instance.EndOfDay();
    }
}
