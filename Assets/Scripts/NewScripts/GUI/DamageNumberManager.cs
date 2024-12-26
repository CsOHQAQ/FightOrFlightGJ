using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }

    [SerializeField] private Canvas mainCanvas;  // 或者在Awake里GetComponentInParent<Canvas>()

    [SerializeField] private DamageNumberUI damageNumberPrefab;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 保证Canvas等引用有效
    }

    public void ShowDamageNumber(Vector3 worldPosition, float finalDamage, bool isCrit, EventContext context)
    {
        // 1. 计算屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        // 2. 在Canvas上生成UI对象 (用对象池或直接 Instantiate)
        DamageNumberUI damageNumber = Instantiate(damageNumberPrefab, mainCanvas.transform);

        // 3. 设置UI位置
        RectTransform rectTrans = damageNumber.GetComponent<RectTransform>();
        rectTrans.anchoredPosition = screenPos;  // 如果Canvas是Screen Space Overlay，可直接用screenPos作为anchoredPosition。 
                                                // 若是Screen Space - Camera，还要做Camera logic或UI scaling转换
        Debug.Log(screenPos);
        // 4. 设置文字、颜色、大小等
        damageNumber.SetDamageValue(finalDamage, isCrit, context);

        // 可根据context中是否有元素伤害、攻击者名称等信息来做更多变化
        // e.g. damageNumber.SetColorBasedOnElement(context.HitData.ElementType);
    }
}
