using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemPickup : MonoBehaviour
{

    [Header("UI Objects")]
    [SerializeField]
    private Image background;

    [SerializeField]
    private TextMeshProUGUI textMesh;

    [SerializeField]
    private Image itemImage;

    private List<Item_ScriptableObject> itemStack;

    [Header("Characteristics")]
    [SerializeField]
    private float itemDelay;

    [SerializeField]
    private float stackDelay;

    [SerializeField]
    private float fadeTime;

    private bool bInProgress;


    // Start is called before the first frame update
    void Start()
    {
        itemStack = new List<Item_ScriptableObject>();

        bInProgress = false;

        ForceHiddenPlate();
    }
    private void ForceHiddenPlate()
    {
        background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 0);
        itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0);
    }

    private void HidePlate()
    {
        ///background.gameObject.SetActive(false);
        ///textMesh.gameObject.SetActive(false);
        ///itemImage.gameObject.SetActive(false);

        StartCoroutine(LerpPlate(255, 0));
    }

    private void ShowPlate()
    {
        ///background.gameObject.SetActive(true);
        ///textMesh.gameObject.SetActive(true);
        ///itemImage.gameObject.SetActive(true);

        StartCoroutine(LerpPlate(0, 255));
    }

    private IEnumerator PopulatePlate()
    {
        ShowPlate();

        textMesh.text = "Picked up a " + itemStack[0].itemName;
        itemImage.sprite = itemStack[0].itemSprite;
        itemStack.RemoveAt(0);

        yield return new WaitForSeconds(itemDelay);

        if (itemStack.Count > 0)
        {
            HidePlate();

            yield return new WaitForSeconds(stackDelay);

            StartCoroutine(PopulatePlate());
        }
        else
        {
            HidePlate();

            bInProgress = false;
        }
    }

    public void AddItemToStack(Item_ScriptableObject item)
    {

        //Debug.Log("Item added to stack");

        itemStack.Add(item);
        if (!bInProgress)
        {

            //Debug.Log("First item added");
            //ShowPlate();

            bInProgress = true;
            StartCoroutine(PopulatePlate());
        }
    }

    private IEnumerator LerpPlate(float min, float max)
    {
        float elapsedTime = 0f;

        Color whiteColor = Color.white;

        while (elapsedTime < fadeTime)
        {

            elapsedTime += Time.deltaTime;

            float alpha = Mathf.Lerp(min, max, elapsedTime / fadeTime);

            alpha = alpha / 255;

            //Debug.Log(alpha);

            background.color = new Color(background.color.r, background.color.g, background.color.b, alpha);
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
            itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, alpha);

            yield return null;

        }

        if (max < min)
        {
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 0);
            itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0);
        }
    }
}
