using UnityEngine;

public class LoginBikeSpawner : MonoBehaviour
{
    public GameObject bikePrefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotationEuler;

    private void Start()
    {
        if (bikePrefab == null) return;

        GameObject bike = Instantiate(bikePrefab, spawnPosition, Quaternion.Euler(spawnRotationEuler));
        
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 500;

        var sound = bike.GetComponent<BikeSoundController>();
        if (sound != null)
        {
            sound.enabled = false;
        }

        var audioSources = bike.GetComponentsInChildren<AudioSource>();
        foreach (var source in audioSources)
        {
            source.mute = true;
            source.Stop();
        }
    }
}
