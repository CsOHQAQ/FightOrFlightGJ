using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
public class BlackOutUI : MonoBehaviour
{
    Image img;
    bool isDying = false;

    float nextAlpha;
    float changeTime;
    float changeSpeed;
    bool isChanging;
    // Start is called before the first frame update
    void Start()
    {
        isDying = false;
        img= GetComponent<Image>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (isDying && img.color.a > 0.99f)
        {

            img.color = new Color(0, 0, 0, 1);
            GameSceneManager.Instance.OnDeath();
        }
        if (isChanging)
        {
            img.color = new Color(0, 0, 0, Mathf.MoveTowards(img.color.a, nextAlpha, changeSpeed));
            if(Mathf.Abs(img.color.a - nextAlpha) < 0.005f)
            {
                img.color = new Color(0, 0, 0, nextAlpha);
                isChanging = false;
                
            }
        }

    }

    public void SetTransparent(float t)
    {
        img.color = new Color(0, 0, 0, t);
    }

    public void TurnBlack(float na,float time)
    {
        if(na>0.97f)
            isDying=true;
        nextAlpha = na;
        changeSpeed = Mathf.Abs(na - img.color.a) * Time.deltaTime / time;
        changeTime = time;
        isChanging = true;
    }

}
