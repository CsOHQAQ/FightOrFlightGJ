using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private bool destroyOnHit = true;

    private float spawnTime;

    // We'll store a global flight direction
    private Vector3 flightDirection;

    // We'll store an EventContext if we want to pass it into the HitEventChain
    private EventContext eventContext;

    private ICharacter sourceCharacter;
    public ICharacter SourceCharacter{get{return sourceCharacter;}}
    private Vector3 startPosition;
    private Vector3 endPosition;
    
    /// <summary>
    /// Called by the spawner to initialize data,
    /// including a global flight direction.
    /// </summary>
    public void Setup(EventContext context, Vector3 flightDir)
    {
        this.eventContext = context;
        this.sourceCharacter = context.Source;
        this.flightDirection = flightDir.normalized; // store and normalize it
        Debug.Log("PROJECTILE: " + flightDirection);
        spawnTime = Time.time;
    }

    private void Update()
    {
        // Move along the global flightDirection
        transform.position += flightDirection * (speed * Time.deltaTime);

        // Destroy if lifetime expired
        if (Time.time - spawnTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) Avoid hitting the source (if you want that logic)
        if (other.GetComponent<ICharacter>() == sourceCharacter || other.GetComponent<Projectile>()!=null)
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
