using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_DeathCanvas : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("UI Objects")]
    [SerializeField]
    private TextMeshProUGUI title;

    [SerializeField]
    private TextMeshProUGUI message;

    [SerializeField]
    private Button button;

    [Header("Attributes")]
    [SerializeField]
    private float lineDelay;

    [SerializeField]
    private float charDelay;

    private string stringTitle;
    private string stringMessage;

    void Start()
    {
        stringTitle = title.text;
        title.text = string.Empty;

        stringMessage = message.text;
        message.text = string.Empty;

        button.gameObject.SetActive(false);

        StartCoroutine(PlayDeathScene());
    }

    private IEnumerator PlayDeathScene()
    {
        yield return new WaitForSeconds(lineDelay);

        foreach (char c in stringTitle)
        {
            if (c != ' ')
            {
                yield return new WaitForSeconds(charDelay);

                title.text += c;
            }
            else
            {
                title.text += c;
            }
        }

        yield return new WaitForSeconds(lineDelay);

        foreach (char c in stringMessage)
        {
            if (c != ' ')
            {
                yield return new WaitForSeconds(charDelay);

                message.text += c;
            }
            else
            {
                message.text += c;
            }
        }

        yield return new WaitForSeconds(lineDelay);

        button.gameObject.SetActive(true);
    }
}
