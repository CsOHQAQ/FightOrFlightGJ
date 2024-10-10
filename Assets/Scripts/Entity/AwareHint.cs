using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwareHint : MonoBehaviour
{
    [SerializeField]
    Transform unawaredMask, searchMask, awaredMask;
    MonsterStats monster;
    // Start is called before the first frame update
    public void Init(MonsterStats m)
    {
        monster = m;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (monster.CurAwareness<1f)
        {
            unawaredMask.transform.localPosition = new Vector3(0, monster.CurAwareness * 0.64f, 0);
            searchMask.transform.localPosition = new Vector3(0, 0, 0);
            awaredMask.transform.localPosition = new Vector3(0, 0, 0);
        }
        if (monster.CurAwareness > 1f&&monster.CurAwareness < 2f)
        {
            unawaredMask.transform.localPosition = new Vector3(0,0.64f, 0);
            searchMask.transform.localPosition = new Vector3(0, (monster.CurAwareness - 1)* 0.64f, 0);
            awaredMask.transform.localPosition = new Vector3(0, 0, 0);
        }
        if (monster.CurAwareness > 2f)
        {
            unawaredMask.transform.localPosition = new Vector3(0, 0.64f, 0);
            searchMask.transform.localPosition = new Vector3(0, 0.64f, 0);
            awaredMask.transform.localPosition = new Vector3(0, (monster.CurAwareness - 2) * 0.64f, 0);
        }
    }
}
