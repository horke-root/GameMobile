using UnityEngine;

public class StatsService : MonoBehaviour
{
    [SerializeField] private BikeController bike;
    [SerializeField] private Rigidbody2D bikeRb;

    public float roundTotalDistance;
    public float roundStuntDistance;
    public bool enableStuntTracking = true;

    private float stuntMinAngle;
    private Vector3 lastPosition;
    private bool isTracking = false;

    public void StartNewRound(int stuntMinAngle, BikeController bike)
    {
        roundTotalDistance = 0f;
        roundStuntDistance = 0f;
        lastPosition = bike.transform.position;
        isTracking = true;
        this.bike = bike;
        this.bikeRb = bike.GetComponent<Rigidbody2D>();
        this.stuntMinAngle = stuntMinAngle;
    }

    public void UpdateBikeReference(BikeController newBike)
    {
        this.bike = newBike;
        this.bikeRb = newBike.GetComponent<Rigidbody2D>();
        this.lastPosition = newBike.transform.position;
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    void FixedUpdate()
    {
        if (!isTracking || bikeRb == null) return;

        float distanceStep = Vector3.Distance(bike.transform.position, lastPosition);

        if (distanceStep > 0.001f)
        {
            roundTotalDistance += distanceStep;

            if (enableStuntTracking && bike.currentPitchAngle > stuntMinAngle)
            {
                roundStuntDistance += distanceStep;
            }

            lastPosition = bike.transform.position;
        }
    }
}
