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
    [SerializeField] private float effectDurationToDestroyChest = 0.11f;
    [SerializeField] private float effectDurationToDestroyEffect = 0.5f;
    private bool bombFlip = true;

    [SerializeField] float timeToExplode = 1.4f;
    private float currentTimeBeforeExplode = 0.0f;
    [HideInInspector] public bool bombActive = false;

    private bool goExplode = false;

    [SerializeField] float timeToChangeSprite = 0.1f;
    private float currentTimeBeforeChangeSprite = 0.0f;

    private SpriteRenderer spriteRenderer;

    private bool exploded = false;
    private bool startToDetonate = false;

    private GameObject playerObject;

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
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            Debug.LogWarning("Not Player Object found with tag");
        }

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

    private void OnTriggerStay(Collider other)
    {
        if (exploded)
        {
            if (other.CompareTag("Player"))
            {
                exploded = false;

                playerObject.GetComponent<PlayerStats>().TakeDamage(1);
            }
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
                exploded = true;
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

        StartCoroutine(DestroyExplosion());
    }

    private IEnumerator DestroyExplosion()
    {
        yield return new WaitForSeconds(effectDurationToDestroyEffect);

        Destroy(gameObject);
    }

    private void ChangeSprite()
    {

        if (!goExplode)
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
}
