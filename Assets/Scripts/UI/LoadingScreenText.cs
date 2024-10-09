using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreenText : MonoBehaviour
{

    [SerializeField]
    private float charDelay;

    [SerializeField]
    private string elipses;

    [SerializeField]
    private string message;

    private TextMeshProUGUI textMeshPro;

    // Start is called before the first frame update
    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();

        StartCoroutine(LoadingText());
    }

    private IEnumerator LoadingText()
    {
        yield return new WaitForSeconds(charDelay);

        string temp = string.Empty;

        foreach (char c in elipses)
        {

            temp += c;

            textMeshPro.text = message + temp;

            yield return new WaitForSeconds(charDelay);

        }

        textMeshPro.text = message;

        StartCoroutine(LoadingText());

    }
}
