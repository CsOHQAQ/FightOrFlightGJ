using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BlackOutUI : MonoBehaviour
{
    Image img;
    // Start is called before the first frame update
    void Start()
    {
        img= GetComponent<Image>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTransparent(float t)
    {
        img.color = new Color(0, 0, 0, t);
    }

    public IEnumerator TurnBlack(float na,float time)
    {
        float timer = 0;
        float speed =Mathf.Abs( (na - img.color.a) * Time.deltaTime / time);
        while (timer < time||img.color.a>=na)
        {
            timer += Time.deltaTime;
            img.color = new Color(0, 0, 0, Mathf.MoveTowards(img.color.a,na,speed));
            yield return null;
        }
        if(img.color.a>=1)
            DayManager.Instance.EndOfDay();
    }

}
