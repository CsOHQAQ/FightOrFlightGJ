using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private GameObject Chest;
    [SerializeField] float timeToExplode = 1.3f;
    private float currentTimeBeforeExplode = 0.0f;

    void Update()
    {
        ToDetonate();
    }

    private void ToDetonate()
    {
        currentTimeBeforeExplode += Time.deltaTime;

        if (currentTimeBeforeExplode >= timeToExplode)
        {
            Destroy(Chest);
            Destroy(gameObject);
        }
    }
}
