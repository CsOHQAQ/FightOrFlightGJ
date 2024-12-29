using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private bool destroyOnHit = true;

    private float spawnTime;

    // We'll store an EventContext if we want to pass it into the HitEventChain
    private EventContext eventContext;

    private ICharacter sourceCharacter;
    
    private Vector3 startPosition;
    private Vector3 endPosition;

    /// <summary>
    /// Called by the spawner (weapon or node) to initialize projectile data.
    /// Typically, you'd pass an eventContext or partial data to build a new one.
    /// </summary>
    public void Setup(EventContext context)
    {
        this.eventContext = context;
        //this.speed = projectileSpeed;
        this.sourceCharacter = context.Source;

        spawnTime = Time.time;
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Check lifetime
        if (Time.time - spawnTime > lifeTime)
        {
            endPosition = transform.position;
            // Possibly do distance logic or finalize here
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) Avoid hitting the source (if you want that logic)
        if (other.GetComponent<ICharacter>() == sourceCharacter)
        {
            return; 
        }
        Debug.Log("HIT" + other.gameObject.name);
        // 2) If it’s an IHitReceiver, we want to call the HitEventChain
        IHitReceiver hitReceiver = other.GetComponent<IHitReceiver>();
        if (hitReceiver != null)
        {
            // If we have an eventContext from the weapon, let's reuse it
            if (eventContext != null)
            {
                // Fill out the HitData
                var hitInfo = new HitInfo {
                    HitPoint = transform.position,
                    HitNormal = -transform.forward,
                    AdditionalData = null
                };

                eventContext.HitData = new HitData {
                    HitInfo = hitInfo,
                    WasCrit = false,
                    IsLethalHit = false
                };
                eventContext.Target = hitReceiver;
                
                // Now call the HitEventChain
                EventChainManager.Instance.ExecuteHitChain(ref eventContext);
            }
            else
            {
                // If no eventContext, do a simplified approach:
                EventContext tempContext = new EventContext {
                    Source = sourceCharacter,
                    Target = hitReceiver,
                    HitData = new HitData {
                        HitInfo = new HitInfo {
                            HitPoint = transform.position,
                            HitNormal = -transform.forward
                        }
                    }
                };

                // Then run the chain
                EventChainManager.Instance.ExecuteHitChain(ref tempContext);
            }
        }

        endPosition = transform.position;

        // Optionally debug distance traveled, etc.
        // float distance = Vector3.Distance(startPosition, endPosition);

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
