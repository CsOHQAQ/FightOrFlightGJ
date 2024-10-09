using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayCanvasManager : MonoBehaviour
{

    [Header("UI Properties")]

    [SerializeField]
    private TextMeshProUGUI numDay;

    [SerializeField]
    private TextMeshProUGUI numStatement;

    [SerializeField]
    private TextMeshProUGUI numMoney;

    [SerializeField]
    private Button adventureButton;

    [Header("Character Properties")]

    [SerializeField]
    private float dayDelay;

    [SerializeField]
    private float characterDelay;

    [SerializeField]
    private float lowCharDelay;

    // Start is called before the first frame update
    void Start()
    {
        numDay.text = string.Empty;
        numStatement.text = string.Empty;
        numMoney.text = string.Empty;
        adventureButton.gameObject.SetActive(false);

        //StartCoroutine(RevealDay(dayDelay));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartDayVisuals()
    {
        StartCoroutine(RevealDay(dayDelay));
    }

    private IEnumerator RevealDay(float delay)
    {
        yield return new WaitForSeconds(delay);

        numDay.text = DayManager.Instance.GetDaysRemaining().ToString();

        StartCoroutine(RevealStatements(dayDelay));
    }

    private IEnumerator RevealStatements(float delay)
    {
        yield return new WaitForSeconds(delay);

        string statement = "Days remain...";

        foreach (char c in statement)
        {
            if (c != ' ')
            {
                yield return new WaitForSeconds(characterDelay);

                numStatement.text = numStatement.text + c;
            }
            else
            {
                yield return new WaitForSeconds(0);

                numStatement.text = numStatement.text + c;
            }
        }

        //string earnedMoney = $"<color=#FFE700> {DayManager.Instance.GetMoney().ToString()}</color>";

        string moneyStatement = "You have earned " + DayManager.Instance.GetMoney().ToString() + " gold of your " + DayManager.Instance.GetQuota().ToString() + " gold tithe";
        //string moneyStatement = "You have earned " + earnedMoney + " gold of your " + DayManager.Instance.GetQuota().ToString() + " gold tithe";

        foreach (char c in moneyStatement)
        {
            if (c != ' ')
            {
                yield return new WaitForSeconds(lowCharDelay);

                numMoney.text += c;
            }
            else
            {
                yield return new WaitForSeconds(0);

                numMoney.text += c;
            }
        }

        yield return new WaitForSeconds(dayDelay);

        adventureButton.gameObject.SetActive(true);
    }

    public void AdventureButtonBehavior()
    {

        DayManager.Instance.ClearCanvas();

        Destroy(gameObject);
    }
}
