using UnityEngine;
using UnityEngine.Pool;





public class SparkFX : MonoBehaviour

{
    [SerializeField] private ParticleSystem sparkPrefab;
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private Collider2D sparkCollider;


    private IObjectPool<ParticleSystem> _pool;

    void Awake()
    {

        _pool = new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(sparkPrefab),
            actionOnGet: (ps) => ps.gameObject.SetActive(true),
            actionOnRelease: (ps) => ps.gameObject.SetActive(false),
            actionOnDestroy: (ps) => Destroy(ps.gameObject),
            collectionCheck: false,
            defaultCapacity: 5,
            maxSize: 15
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider == sparkCollider && collision.gameObject.CompareTag(groundTag))
        {
            SpawnSparks(collision);
        }
    }

    private void SpawnSparks(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);


        ParticleSystem sparks = _pool.Get();

        if (sparks != null)
        {
            sparks.transform.position = contact.point;
            sparks.transform.up = contact.normal;
            sparks.Play();



            StartCoroutine(ReturnToPoolAfterTime(sparks, sparks.main.duration));
        }
    }

    private System.Collections.IEnumerator ReturnToPoolAfterTime(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ps.gameObject.activeSelf)
            _pool.Release(ps);
    }
}
