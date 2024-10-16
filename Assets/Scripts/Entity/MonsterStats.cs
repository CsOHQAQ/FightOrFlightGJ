using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class MonsterStats : MonoBehaviour
{
    public string MonsterName;//Just differ from transform.name
    public int MaxHealth;
    public int CurHealth;
    public int Attack;
    public int Defense;
    public int Awareness;
    public float CurAwareness;
    public AwareLevel CurAwareLevel;
    public List<Trait_Type> traits;


    [SerializeField]
    float OpennessTolerate = 0.1f;
    AwareHint awareHint;
    // Start is called before the first frame update
    void Start()
    {
        CurAwareness = 0f;
        awareHint=GetComponentInChildren<AwareHint>();
        awareHint.Init(this);
    }


    // Update is called once per frame
    void Update()
    {
        if (CurAwareness < 1f)
        {
            CurAwareLevel = AwareLevel.NotAwared;
            //GetComponent<FacingPlayer>().CanFacing = false;
        }
        else if (CurAwareness > 1f && CurAwareness < 2f)
        {
            CurAwareLevel = AwareLevel.Searching;
            //GetComponent<FacingPlayer>().CanFacing = true;
        }
        else if(CurAwareness>=3f)
        {
            CurAwareLevel = AwareLevel.Awared;
        }

    }

    public void RefreshAwareness(bool canSeePlayer,float openAngle)
    {
        if(openAngle<OpennessTolerate)
            canSeePlayer = false;
        if (canSeePlayer)
        {
            CurAwareness = Mathf.MoveTowards(CurAwareness, 3,openAngle* Awareness * Time.deltaTime / Mathf.Log(Vector3.Distance(transform.position, GameControl.Game.Player.transform.position)+1));
            
        }            
        else
                CurAwareness = Mathf.MoveTowards(CurAwareness,0,Time.deltaTime/10f*Awareness);
    }

    public enum AwareLevel
    {
        Empty,
        NotAwared, //Awareness 0-1
        Searching, //Awareness 1-2
        Awared, //Awareness 2-3

    }

    public void TakeDamage(int damage)
    {
        CurHealth -= damage;
        CurHealth = Mathf.Clamp(CurHealth, 0,MaxHealth);
    }
}
