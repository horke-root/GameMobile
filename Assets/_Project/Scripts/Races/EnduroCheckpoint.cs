using UnityEngine;

public class EnduroCheckpoint : MonoBehaviour
{
    public RaceService raceService;

    private bool isTriggered = false;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[EnduroCheckpoint] OnTriggerEnter2D: collider={collision.gameObject.name}, tag={collision.gameObject.tag}, isTriggered={isTriggered}, raceService={(raceService != null)}");
        if (collision.gameObject.CompareTag("Bike") && !isTriggered)
        {
            isTriggered = true;
            if (raceService != null)
            {
                Debug.Log("[EnduroCheckpoint] Checkpoint reached! Calling raceService.OnCheckpointReached()");
                raceService.OnCheckpointReached();
            }
            else
            {
                Debug.LogError("[EnduroCheckpoint] raceService is NULL! Cannot register checkpoint!");
            }
        }
    }
}
