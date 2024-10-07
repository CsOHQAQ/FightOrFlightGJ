using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStats : MonoBehaviour
{
    public string Name;
    public int MaxHealth;
    public int CurHealth;
    public int Attack;
    public int Defense;
    public int Awareness;
    public float CurAwareness;
    public bool isAwared;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CurAwareness > 1f)
        {
            isAwared = true;
        }
        else
        {
            isAwared= false;
        }
        
    }

    public void RefreshAwareness(bool canSeePlayer)
    {
        if (canSeePlayer)
            CurAwareness += Awareness * Time.deltaTime / Vector2.Distance(transform.position, GameControl.Game.Player.transform.position);
        else
            CurAwareness -= Time.deltaTime / Awareness;
    }
}
