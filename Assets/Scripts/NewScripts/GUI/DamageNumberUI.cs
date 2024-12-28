using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumberUI : MonoBehaviour
{
    private TextMeshProUGUI damageText;
    
    // 可选动画的参数
    [SerializeField] private float floatUpSpeed = 30f;
    [SerializeField] private float fadeDuration = 1.5f;
    private float timer;

    private void Awake() {
        damageText= GetComponent<TextMeshProUGUI>();
    }

    public void SetDamageValue(float damage, bool isCrit, EventContext context)
    {
        damageText.text = Mathf.RoundToInt(damage).ToString();

        // 若是暴击，换颜色/变大
        if(isCrit)
        {
            damageText.color = Color.yellow;
            damageText.fontSize = 40;
            damageText.text += "!!";
        }
        else
        {
            damageText.color = Color.white;
            damageText.fontSize = 20;
        }

        // 也可读取 context.HitData.ElementType 去改颜色
    }

    private void Update()
    {
        // 简单向上飘 + 逐渐透明
        timer += Time.deltaTime;
        // UI 向上移动
        transform.Translate(Vector3.up * floatUpSpeed * Time.deltaTime);
        
        if(timer >= fadeDuration)
        {
            // 实际项目中可用对象池回收
            Destroy(gameObject);
        }
    }
}
