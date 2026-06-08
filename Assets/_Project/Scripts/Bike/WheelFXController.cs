using UnityEngine;
using UnityEngine.Serialization;





public class WheelFXController : MonoBehaviour
{
    [Header("Физика")]
    [FormerlySerializedAs("rearWheelRb")]
    public Rigidbody2D wheelRb;
    public Rigidbody2D frameRb;
    public LayerMask groundLayer;
    public float wheelRadius = 0.28f;

    [Header("Включение Эффектов")]
    public bool enableSmoke = true;
    public bool enableDirt = true;
    public bool enableDust = true;

    [Header("Настройки Пробуксовки (Smoke/Dirt)")]
    public float slipThreshold = 2.0f;
    public float maxEmissionRate = 50f;

    [Header("Настройки Пыли")]
    public float dustSpeedThreshold = 1.0f;
    public float maxDustSpeed = 20f;
    public float maxDustEmissionRate = 30f;

    [Header("Эффекты (Particle Systems)")]
    public ParticleSystem smokeParticles;
    public ParticleSystem dirtParticles;
    public ParticleSystem dustParticles;


    private ParticleSystem.EmissionModule smokeEmission;
    private ParticleSystem.EmissionModule dirtEmission;
    private ParticleSystem.EmissionModule dustEmission;

    private void Start()
    {

        if (smokeParticles != null)
        {
            smokeEmission = smokeParticles.emission;
            smokeEmission.enabled = true;
            smokeEmission.rateOverTime = 0f;
        }

        if (dirtParticles != null)
        {
            dirtEmission = dirtParticles.emission;
            dirtEmission.enabled = true;
            dirtEmission.rateOverTime = 0f;
        }

        if (dustParticles != null)
        {
            dustEmission = dustParticles.emission;
            dustEmission.enabled = true;
            dustEmission.rateOverTime = 0f;
        }
    }

    private void FixedUpdate()
    {
        HandleSurfaceFX();
    }

    private void HandleSurfaceFX()
    {

        Vector2 rayStart = wheelRb.position;
        float rayDistance = wheelRadius + 0.1f;


        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, rayDistance, groundLayer);


        if (hit.collider == null)
        {
            StopAllFX();
            return;
        }


        float actualSpeedMs = frameRb.linearVelocity.magnitude;
        float wheelSpinSpeedMs = (Mathf.Abs(wheelRb.angularVelocity) * Mathf.Deg2Rad) * wheelRadius;
        float slip = wheelSpinSpeedMs - actualSpeedMs;


        float smokeRate = 0f;
        float dirtRate = 0f;

        if (slip > slipThreshold)
        {
            float targetEmission = Mathf.Lerp(0f, maxEmissionRate, slip / 10f);
            float currentRate = Mathf.Clamp(targetEmission, 10f, maxEmissionRate);


            if (hit.collider.CompareTag("Dirt"))
            {
                if (enableDirt) dirtRate = currentRate;
            }
            else
            {
                if (enableSmoke) smokeRate = currentRate;
            }
        }


        float dustRate = 0f;
        if (enableDust && hit.collider.CompareTag("Dirt"))
        {
            if (wheelSpinSpeedMs > dustSpeedThreshold)
            {
                float t = (wheelSpinSpeedMs - dustSpeedThreshold) / (maxDustSpeed - dustSpeedThreshold);
                dustRate = Mathf.Lerp(0f, maxDustEmissionRate, Mathf.Clamp01(t));
            }
        }

        SetEmissionRates(smokeRate, dirtRate, dustRate);
    }


    private void SetEmissionRates(float smokeRate, float dirtRate, float dustRate)
    {
        if (smokeParticles != null) smokeEmission.rateOverTime = smokeRate;
        if (dirtParticles != null) dirtEmission.rateOverTime = dirtRate;
        if (dustParticles != null) dustEmission.rateOverTime = dustRate;
    }

    private void StopAllFX()
    {
        SetEmissionRates(0f, 0f, 0f);
    }
}
