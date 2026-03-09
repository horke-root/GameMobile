using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{

    public Transform target;
    public Vector3 offset = new Vector3(5f, 1f, -10f);
    
    [Range(1f, 15f)]
    public float followSpeed = 5f;
    
    public float baseOrthoSize = 7f;
    [Range(1f, 5f)]
    public float zoomSpeed = 2f;
    
    public float maxOrthoSize = 10f;
    public float maxZoomSpeedThreshold = 70f;

    public bool enableCameraShake = true; 
    public float maxShakeMagnitude = 0.15f; 
    public float speedShakeFactor = 0.05f; 


    private Camera cam;
    private Rigidbody2D targetRigidbody;
    private float currentSpeed;
    private Vector3 initialOffset;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow2D требует компонент Camera!");
            enabled = false;
            return;
        }

        
        
        if (target != null)
        {
            targetRigidbody = target.GetComponent<Rigidbody2D>();
            initialOffset = offset;
        }
    }

    void Update()
    {
        if (targetRigidbody != null)
        {
            currentSpeed = targetRigidbody.linearVelocity.magnitude;
        }
    }
    
    void LateUpdate() //LateUpdate: для андроїд
    {
        if (target == null || cam == null)
        {
            return;
        }


        Vector3 desiredPosition = target.position + initialOffset;
        

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed );
        
        Vector3 shakeVector = Vector3.zero;

        if (enableCameraShake)
        {
            float shakePower = Mathf.Clamp01(currentSpeed * speedShakeFactor);
            if (shakePower > 0.01f)
            {
                float noiseX = Mathf.PerlinNoise(Time.time * 20f, 0f) * 2f - 1f;
                float noiseY = Mathf.PerlinNoise(0f, Time.time * 20f) * 2f - 1f;
                shakeVector = new Vector3(noiseX, noiseY, 0) * maxShakeMagnitude * shakePower;
            }
        }
        
    
        transform.position = smoothedPosition + shakeVector;
        
        //Зміна зума
        if (cam.orthographic)
        {
            float speedPercent = Mathf.InverseLerp(0, maxZoomSpeedThreshold, currentSpeed);
            
            float desiredOrthoSize = Mathf.Lerp(baseOrthoSize, maxOrthoSize, speedPercent);

            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize, 
                desiredOrthoSize, 
                zoomSpeed * Time.deltaTime
            );
        }
    }
}