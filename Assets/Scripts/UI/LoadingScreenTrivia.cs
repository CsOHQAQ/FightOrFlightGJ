using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreenTrivia : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    [TextArea(2, 3)]
    private string[] trivia;

    private TextMeshProUGUI textMeshProUGUI;

    void Start()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();

        textMeshProUGUI.text = string.Empty;

        int randomIndex = Random.Range(0,trivia.Length);

        textMeshProUGUI.text = trivia[randomIndex];
    }
}
