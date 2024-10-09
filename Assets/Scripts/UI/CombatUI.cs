using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class CombatUI : MonoBehaviour
{
    [SerializeField]
    private GameObject MonsterUIPrefab;
    Animator animator;
    Transform monsterListTran;
    Image PlayerHealth,MonsterHealth;
    Vector3 EnemyStartPos;

    private TextMeshProUGUI text;

    [HideInInspector]
    public PlayerStats player;
    [HideInInspector]
    public MonsterStats monster;
    public MonsterStats monsterBuffer;

    public void OnOpen(PlayerStats p,List<MonsterStats> monsters)
    {
        animator= GetComponent<Animator>();
        monsterListTran = transform.Find("MonsterList");
        PlayerHealth = transform.Find("Player").Find("HealthBar").Find("CurHealth").GetComponent<Image>();
        MonsterHealth= transform.Find("Monster").Find("HealthBar").Find("CurHealth").GetComponent<Image>();
        EnemyStartPos = transform.Find("Monster").transform.position;
        text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        text.text = "";
        player = p;

        int msCount=monsterListTran.childCount;
        for (int i = 0; i < msCount; i++)
        {
            Destroy(monsterListTran.GetChild(i).gameObject);            
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            GameObject m = GameObject.Instantiate(MonsterUIPrefab, monsterListTran);
            string spriteName = ($"Test_{monsters[i].MonsterName}");
            m.GetComponent<Image>().sprite = Resources.Load<Sprite>(spriteName);
            m.GetComponent<Image>().color = new Color(0.5f,0.5f,0.5f);
        }
        monster = monsters[0];
        ChangeNextMonster();

    }
    private void Update()
    {
        PlayerHealth.fillAmount = Mathf.MoveTowards(PlayerHealth.fillAmount, (1.0f*player.CurHealth / player.MaxHealth), Time.deltaTime / 1f);
        MonsterHealth.fillAmount= Mathf.MoveTowards(MonsterHealth.fillAmount, 1.0f*monster.CurHealth / monster.MaxHealth, Time.deltaTime / 1f);
        if (MonsterHealth.fillAmount <= 0.001f)
        {
            if (monsterBuffer != null)
            {
                monster=monsterBuffer; ;
                monsterBuffer = null;
            }
            ChangeNextMonster();

        }

    }

    public void ChangeNextMonster()
    {
        if (monsterListTran.childCount > 0)
        {
            transform.Find("Monster").GetComponent<Image>().sprite = monsterListTran.GetChild(0).GetComponent<Image>().sprite;
            MonsterHealth.fillAmount =(1f* monster.CurHealth)/monster.MaxHealth;
            Destroy(monsterListTran.GetChild(0).gameObject);
        }
        else 
        {
            Debug.Log("No monster left");
        }
    }
    public void Attack()
    {
        animator.Play("CombatUI_Attack");
    }

    public void ChangeText(string t)
    {
        text.text = t;
    }
    public void SelfClose(float second)
    {
        StartCoroutine(WaitForClose(second));
    }
    IEnumerator WaitForClose(float second)
    {
        yield return new WaitForSeconds(second);
        Destroy(gameObject);
    }
}
