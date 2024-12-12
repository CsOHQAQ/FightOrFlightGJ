using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData {
    public string ID;             // 道具模板ID，用于从数据表中读取配置信息
    public string Name;
    public string Description;
    public string IconPath;
    public float Weight;
    public int Value;
    public int MaxStackCount;     // 堆叠上限。如果为1则不可堆叠
    // 可根据需要添加更多数据字段
}
