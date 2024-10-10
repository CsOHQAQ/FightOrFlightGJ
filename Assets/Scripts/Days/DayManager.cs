using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Money")]

    [SerializeField]
    private int moneyQuota;

    private int earnedMoney;

    [Header("Days")]

    [SerializeField]
    private int startingDay;

    private int currentDay;

    [SerializeField]
    private int endingDay;

    [Header("User Interface")]

    [SerializeField]
    private GameObject dayCanvasManager;

    private DayCanvasManager canvasManager;

    [Header("Debug")]

    [SerializeField]
    private bool autoStart;

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

    private void Start()
    {
        currentDay = startingDay;

        earnedMoney = 0;

        if (autoStart)
        {
            //CreateDayCanvas();
            EndOfDay();
        }
    }

    public void EndOfDay()
    {
        UI_InventoryManager.Instance.ShowInventoryCanvas();
        UI_InventoryManager.Instance.EnterStore();
    }

    public void GoToNextDay()
    {
        currentDay++;

        GameControl.Reset();

        Debug.Log("Day: " + currentDay);

        UI_InventoryManager.Instance.ExitStore();

        if(currentDay > endingDay)
        {
            GameSceneManager.Instance.TransitionToScene("EndOfGameScene");
        }
        else
        {
            GameSceneManager.Instance.TransitionToScene("TileMapScene");
            CreateDayCanvas();
        }
    }

    public int GetCurrentDay() { return currentDay; }

    public void AddMoney(int money)
    {
        earnedMoney += money;
        Debug.Log(earnedMoney);
    }

    public int GetMoney() { return earnedMoney; }

    public int GetQuota() { return moneyQuota; }

    public int GetDaysRemaining()
    {
        return endingDay - currentDay;
    }

    public void CreateDayCanvas()
    {
        GameObject temp = Instantiate(dayCanvasManager);
        canvasManager = temp.GetComponent<DayCanvasManager>();

        canvasManager.StartDayVisuals();
    }

    public void ClearCanvas()
    {
        canvasManager = null;
    }

    public bool GetSuccess()
    {
        if(currentDay >= endingDay && earnedMoney >= moneyQuota)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
