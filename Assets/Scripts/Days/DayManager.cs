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
    private DayCanvasManager dayCanvasManager;

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
    }

    public int GetCurrentDay() { return currentDay; }

    public void AddMoney(int money) { earnedMoney += money; }

    public int GetMoney() { return earnedMoney; }

    public int GetQuota() { return moneyQuota; }

    public int GetDaysRemaining()
    {
        return endingDay - currentDay;
    }
}
