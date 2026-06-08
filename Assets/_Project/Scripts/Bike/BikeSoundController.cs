using UnityEngine;






public class BikeSoundController : MonoBehaviour

{

    [Header("Связь с физикой")]
    public BikeController bike;


    [Header("Audio Sources (Источники звука)")]
    public AudioSource idleAudioSource;
    public AudioSource highRpmAudioSource;
    public AudioSource decelerationAudioSource;
    public AudioSource oneShotAudioSource;


    [Header("Настройки Холостого хода (Idle)")]
    [Range(0f, 1f)] public float idleMaxVolume = 0.8f;
    public float idleMinPitch = 0.8f;
    public float idleMaxPitch = 1.5f;

    [Header("Настройки Высоких оборотов (High RPM)")]
    [Range(0f, 1f)] public float highRpmMaxVolume = 1.0f;
    public float highRpmMinPitch = 0.8f;
    public float highRpmMaxPitch = 2.2f;

    [Header("Настройки Торможения двигателем (Decel)")]
    [Range(0f, 1f)] public float decelMaxVolume = 0.7f;
    public float decelMinPitch = 0.8f;
    public float decelMaxPitch = 1.8f;

    [Header("Аудиоклипы (Разовые)")]
    public AudioClip gearShiftSound;
    public AudioClip engineStartSound;
    public AudioClip engineStallSound;
    [Range(0f, 1f)] public float oneShotVolume = 1.0f;

    [Header("Плавность")]
    [Range(1f, 20f)] public float smoothingSpeed = 10f;


    private float currentIdleVolume, currentIdlePitch;
    private float currentHighRpmVolume, currentHighRpmPitch;
    private float currentDecelVolume, currentDecelPitch;


    private int lastGear;
    private bool wasEngineRunning;

    private void Start()
    {

        if (FindFirstObjectByType<RaceService>() == null)
        {
            if (idleAudioSource) idleAudioSource.Stop();
            if (highRpmAudioSource) highRpmAudioSource.Stop();
            if (decelerationAudioSource) decelerationAudioSource.Stop();
            if (oneShotAudioSource) oneShotAudioSource.Stop();
            enabled = false;
            return;
        }

        if (bike == null) bike = GetComponent<BikeController>();


        if (idleAudioSource) { idleAudioSource.loop = true; idleAudioSource.volume = 0f; idleAudioSource.Play(); }
        if (highRpmAudioSource) { highRpmAudioSource.loop = true; highRpmAudioSource.volume = 0f; highRpmAudioSource.Play(); }
        if (decelerationAudioSource) { decelerationAudioSource.loop = true; decelerationAudioSource.volume = 0f; decelerationAudioSource.Play(); }

        if (bike != null)
        {
            lastGear = bike.currentGear;
            wasEngineRunning = bike.isEngineRunning;
        }
    }

    private void Update()
    {
        if (bike == null) return;

        HandleOneShotSounds();
        UpdateEngineSoundLayers();
    }

    private void HandleOneShotSounds()
    {

        if (bike.currentGear != lastGear)
        {

            if (gearShiftSound != null && oneShotAudioSource != null)
            {

                oneShotAudioSource.pitch = Random.Range(0.9f, 1.1f);
                oneShotAudioSource.PlayOneShot(gearShiftSound, oneShotVolume);
            }
            lastGear = bike.currentGear;
        }


        if (bike.isEngineRunning != wasEngineRunning)
        {
            oneShotAudioSource.pitch = 1f;

            if (bike.isEngineRunning)
            {
                if (engineStartSound != null) oneShotAudioSource.PlayOneShot(engineStartSound, oneShotVolume);
            }
            else
            {
                if (engineStallSound != null) oneShotAudioSource.PlayOneShot(engineStallSound, oneShotVolume);
            }
            wasEngineRunning = bike.isEngineRunning;
        }
    }

    private void UpdateEngineSoundLayers()
    {

        if (!bike.isEngineRunning)
        {
            FadeOutAllSounds();
            return;
        }


        float rpmPercent = Mathf.Clamp01((bike.currentEngineRPM - bike.idleRPM) / (bike.maxRPM - bike.idleRPM));



        float targetIdleVolume = Mathf.Lerp(idleMaxVolume, 0f, rpmPercent * 2f);
        float targetIdlePitch = Mathf.Lerp(idleMinPitch, idleMaxPitch, rpmPercent);



        float targetHighRpmVolume = Mathf.Clamp01(Mathf.InverseLerp(0.2f, 0.8f, rpmPercent)) * highRpmMaxVolume;
        float targetHighRpmPitch = Mathf.Lerp(highRpmMinPitch, highRpmMaxPitch, rpmPercent);


        float targetDecelVolume = 0f;
        float targetDecelPitch = decelMinPitch;


        bool isDecelerating = bike.throttleInput < 0.05f && bike.clutchLock > 0.5f && bike.currentSpeedKmh > 5f;

        if (isDecelerating)
        {
            targetDecelVolume = decelMaxVolume;
            targetDecelPitch = Mathf.Lerp(decelMinPitch, decelMaxPitch, rpmPercent);


            targetHighRpmVolume *= 0.5f;
        }


        float dt = Time.deltaTime * smoothingSpeed;

        currentIdleVolume = Mathf.Lerp(currentIdleVolume, targetIdleVolume, dt);
        currentIdlePitch = Mathf.Lerp(currentIdlePitch, targetIdlePitch, dt);

        currentHighRpmVolume = Mathf.Lerp(currentHighRpmVolume, targetHighRpmVolume, dt);
        currentHighRpmPitch = Mathf.Lerp(currentHighRpmPitch, targetHighRpmPitch, dt);

        currentDecelVolume = Mathf.Lerp(currentDecelVolume, targetDecelVolume, dt);
        currentDecelPitch = Mathf.Lerp(currentDecelPitch, targetDecelPitch, dt);


        if (idleAudioSource) { idleAudioSource.volume = currentIdleVolume; idleAudioSource.pitch = currentIdlePitch; }
        if (highRpmAudioSource) { highRpmAudioSource.volume = currentHighRpmVolume; highRpmAudioSource.pitch = currentHighRpmPitch; }
        if (decelerationAudioSource) { decelerationAudioSource.volume = currentDecelVolume; decelerationAudioSource.pitch = currentDecelPitch; }
    }

    private void FadeOutAllSounds()
    {
        float dt = Time.deltaTime * smoothingSpeed;

        currentIdleVolume = Mathf.Lerp(currentIdleVolume, 0f, dt);
        currentHighRpmVolume = Mathf.Lerp(currentHighRpmVolume, 0f, dt);
        currentDecelVolume = Mathf.Lerp(currentDecelVolume, 0f, dt);

        if (idleAudioSource) idleAudioSource.volume = currentIdleVolume;
        if (highRpmAudioSource) highRpmAudioSource.volume = currentHighRpmVolume;
        if (decelerationAudioSource) decelerationAudioSource.volume = currentDecelVolume;
    }
}
