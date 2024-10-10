using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Bomb : MonoBehaviour
{
    [HideInInspector] public GameObject Chest = null;
    [SerializeField] private List<Sprite> bombs;
    [SerializeField] private VisualEffectAsset effectAsset;
    private VisualEffect visualEffect;
    [SerializeField] private float effectDurationToDestroyChest = 0.13f;
    [SerializeField] private float effectDurationToDestroyEffect = 0.5f;
    private bool bombFlip = true;

    [SerializeField] float timeToExplode = 1.4f;
    private float currentTimeBeforeExplode = 0.0f;
    [HideInInspector] public bool bombActive = false;

    private bool goExplode = false;

    [SerializeField] float timeToChangeSprite = 0.1f;
    private float currentTimeBeforeChangeSprite = 0.0f;

    private SpriteRenderer spriteRenderer;

    private bool startToDetonate = false;

    private void OnEnable()
    {
        StartCoroutine(WaitForChest());
    }

    private IEnumerator WaitForChest()
    {
        while (Chest == null)
        {
            yield return null; // Wait for the next frame
        }

        InitializieBomb();
    }

    private void InitializieBomb()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (visualEffect == null)
        {
            visualEffect = gameObject.AddComponent<VisualEffect>();
        }

        visualEffect.visualEffectAsset = effectAsset;

        visualEffect.Stop();

        bombActive = Chest.GetComponent<OpenChest>().bombActive;

        if (bombActive)
        {
            currentTimeBeforeExplode = 0.0f;
            currentTimeBeforeChangeSprite = 0.0f;
            startToDetonate = true;
        }
    }


    void Update()
    {
        
        if (startToDetonate)
        {
            ChangeSprite();
            ToDetonate();
        }

    }

    private void ToDetonate()
    {
        currentTimeBeforeExplode += Time.deltaTime;

        Debug.Log($"This is my current time : {currentTimeBeforeExplode}");

        if (currentTimeBeforeExplode >= timeToExplode)
        {
            if (!goExplode)
            {
                goExplode = true;
                spriteRenderer.enabled = false;
                playExplosion();
            }
            
        }
    }

    private void playExplosion()
    {
        visualEffect.Play();
        StartCoroutine(DestroyChest());
    }

    private IEnumerator DestroyChest()
    {
        yield return new WaitForSeconds(effectDurationToDestroyChest);

        Destroy(Chest);

        DestroyExplosion();
    }

    private IEnumerator DestroyExplosion()
    {
        yield return new WaitForSeconds(effectDurationToDestroyEffect);

        Destroy(gameObject);
    }

    private void ChangeSprite()
    {
        currentTimeBeforeChangeSprite += Time.deltaTime;

        if (currentTimeBeforeChangeSprite >= timeToChangeSprite)
        {
            spriteRenderer.sprite = bombs[bombFlip ? 1 : 0];

            bombFlip = !bombFlip;

            currentTimeBeforeChangeSprite = 0.0f;
        }
    }
}
