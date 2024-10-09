using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndOfGame_Behavior : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("UI Elements")]
    [SerializeField]
    private TextMeshProUGUI outcome;

    [SerializeField]
    private TextMeshProUGUI message;

    [SerializeField]
    private TextMeshProUGUI progress;

    [SerializeField]
    private Button menuButton;

    [SerializeField]
    private Button quitButton;

    [Header("Attributes")]
    [SerializeField]
    private float charDelay;

    [SerializeField]
    private float textMeshDelay;

    [SerializeField]
    private float buttonDelay;

    [SerializeField]
    private string successString;

    [SerializeField]
    private string failureString;

    private bool bWasSuccessful;

    [Header("DEBUG")]
    [SerializeField]
    private bool shouldDebug;

    [SerializeField]
    private bool debugSuccess;

    

    void Start()
    {
        outcome.text = string.Empty;
        message.text = string.Empty;
        progress.text = string.Empty;
        quitButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(false);

        if (shouldDebug)
        {
            StartCoroutine(TypewriterText(debugSuccess));
        }
    }

    private IEnumerator TypewriterText(bool b)
    {
        yield return new WaitForSeconds(textMeshDelay);

        string msg;
        if (b)
        {
            msg = successString;
        }
        else
        {
            msg = failureString;
        }

        foreach (char c in msg)
        {
            if(c != ' ')
            {
                yield return new WaitForSeconds(charDelay);
                outcome.text += c;
            }
            else
            {
                outcome.text += c;
            }
        }

        yield return new WaitForSeconds(textMeshDelay);

        string msg1;
        if(b)
        {
            msg1 = "You were able to pay your tithe";
        }
        else
        {
            msg1 = "You were " + (DayManager.Instance.GetQuota() - DayManager.Instance.GetMoney()).ToString() + " gold short of the tithe";
        }
        foreach(char c in msg1)
        {
            if(c != ' ')
            {
                yield return new WaitForSeconds(charDelay);
                message.text += c;
            }
            else
            {
                message.text += c;
            }
        }

        yield return new WaitForSeconds(textMeshDelay);

        string msg2;
        if(b)
        {
            msg2 = "See you next week...";
        }
        else
        {
            msg2 = "Report to the stockades immediately.";
        }
        foreach(char c in msg2)
        {
            if(c != ' ')
            {
                yield return new WaitForSeconds(charDelay);
                progress.text += c;
            }
            else
            {
                progress.text += c;
            }
        }

        yield return new WaitForSeconds(buttonDelay);

        menuButton.gameObject.SetActive(true);

        yield return new WaitForSeconds(buttonDelay);

        quitButton.gameObject.SetActive(true);

    }
}
