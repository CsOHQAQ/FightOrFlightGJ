using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Bomb : MonoBehaviour
{
    [HideInInspector] public GameObject Chest;
    [SerializeField] private List<Sprite> bombs;
    [SerializeField] private VisualEffectAsset effectAsset;
    private VisualEffect visualEffect;
    [SerializeField] private float effectDuration = 1.0f;
    private bool bombFlip = true;

    [SerializeField] float timeToExplode = 1.4f;
    private float currentTimeBeforeExplode = 0.0f;
    [HideInInspector] public bool bombActive = false;

    private bool goExplode = false;

    [SerializeField] float timeToChangeSprite = 0.2f;
    private float currentTimeBeforeChangeSprite = 0.0f;

    private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        WaitForChest();
    }

    private IEnumerator WaitForChest()
    {
        while (Chest == null)
        {
            yield return null; // Wait for the next frame
        }
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (visualEffect == null)
        {
            visualEffect = gameObject.AddComponent<VisualEffect>();
        }

        visualEffect.visualEffectAsset = effectAsset;

        bombActive = Chest.GetComponent<OpenChest>().bombActive;
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
        yield return new WaitForSeconds(effectDuration);

        Destroy(Chest);
        Destroy(gameObject);


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
