using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggroCheck : MonoBehaviour
{
    public GameObject PlayerTarget { get; set; }
    private Enemy _enemy;

    private void Awake()
    {
        PlayerTarget = GameObject.FindGameObjectWithTag("Player");

        _enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == PlayerTarget)
        {
            //_enemy.SetAggroStatus(true);
            if (!_enemy.IsAggroed)
            {
                //Should tell enemy to run it's logic handling awareness.
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject == PlayerTarget)
        {
            //_enemy.SetAggroStatus(false);
            //Should never leave aggro currently. 
        }
    }
}
