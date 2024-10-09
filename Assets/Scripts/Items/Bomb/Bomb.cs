using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [HideInInspector] public GameObject Chest;
    [SerializeField] private List<Sprite> bombs;
    private bool bombFlip = true;

    [SerializeField] float timeToExplode = 1.4f;
    private float currentTimeBeforeExplode = 0.0f;
    [HideInInspector] public bool bombActive = false;

    [SerializeField] float timeToChangeSprite = 0.2f;
    private float currentTimeBeforeChangeSprite = 0.0f;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (bombActive)
        {
            ChangeSprite();
            ToDetonate();
        }
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

    private void ChangeSprite()
    {
        currentTimeBeforeChangeSprite += Time.deltaTime;

        if (currentTimeBeforeChangeSprite >= timeToChangeSprite)
        {
            spriteRenderer.sprite = bombs[bombFlip ? 1 : 0];

            bombFlip = !bombFlip;
        }
    }
}
