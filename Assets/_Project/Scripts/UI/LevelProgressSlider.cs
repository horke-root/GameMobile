using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelProgressSlider : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private RectTransform checkpointMarkerPrefab;
    [SerializeField] private RectTransform maxProgressMarker;
    [SerializeField] private RectTransform markersContainer;

    private Transform playerTransform;
    private float startX;
    private float endX;
    private bool isInitialized = false;
    private string levelKey;
    private float historicalMaxProgress = 0f;

    private void OnEnable()
    {
        EventManager.OnPlayerSpawned += OnPlayerSpawned;
        if (EventManager.ActivePlayer != null)
        {
            OnPlayerSpawned(EventManager.ActivePlayer);
        }
    }

    private void OnDisable()
    {
        EventManager.OnPlayerSpawned -= OnPlayerSpawned;
    }

    private void OnPlayerSpawned(Transform bikeTransform)
    {
        playerTransform = bikeTransform;
        startX = playerTransform.position.x;

        EndOfRoad endOfRoad = FindFirstObjectByType<EndOfRoad>();
        if (endOfRoad != null)
        {
            endX = endOfRoad.transform.position.x;
        }
        else
        {
            EnduroCheckpoint[] checkpoints = FindObjectsByType<EnduroCheckpoint>(FindObjectsSortMode.None);
            float maxCheckX = startX;
            foreach (var cp in checkpoints)
            {
                if (cp.transform.position.x > maxCheckX)
                {
                    maxCheckX = cp.transform.position.x;
                }
            }
            endX = maxCheckX + 100f;
        }

        levelKey = "MaxProgress_" + SceneManager.GetActiveScene().name;
        historicalMaxProgress = PlayerPrefs.GetFloat(levelKey, 0f);

        SpawnCheckpointMarkers();
        UpdateMaxProgressMarker();

        isInitialized = true;
    }

    private void SpawnCheckpointMarkers()
    {
        if (checkpointMarkerPrefab == null || markersContainer == null) return;

        foreach (Transform child in markersContainer)
        {
            if (child != maxProgressMarker)
            {
                Destroy(child.gameObject);
            }
        }

        EnduroCheckpoint[] checkpoints = FindObjectsByType<EnduroCheckpoint>(FindObjectsSortMode.None);
        float totalDist = endX - startX;
        if (totalDist <= 0.1f) return;

        foreach (var cp in checkpoints)
        {
            float cpX = cp.transform.position.x;
            float progress = (cpX - startX) / totalDist;
            progress = Mathf.Clamp01(progress);

            RectTransform marker = Instantiate(checkpointMarkerPrefab, markersContainer);
            marker.gameObject.SetActive(true);

            marker.anchorMin = new Vector2(progress, 0.5f);
            marker.anchorMax = new Vector2(progress, 0.5f);
            marker.anchoredPosition = Vector2.zero;
        }
    }

    private void UpdateMaxProgressMarker()
    {
        if (maxProgressMarker == null) return;

        if (historicalMaxProgress <= 0.005f)
        {
            maxProgressMarker.gameObject.SetActive(false);
        }
        else
        {
            maxProgressMarker.gameObject.SetActive(true);
            maxProgressMarker.anchorMin = new Vector2(historicalMaxProgress, 0.5f);
            maxProgressMarker.anchorMax = new Vector2(historicalMaxProgress, 0.5f);
            maxProgressMarker.anchoredPosition = Vector2.zero;
        }
    }

    private void Update()
    {
        if (!isInitialized || playerTransform == null || progressSlider == null) return;

        float currentX = playerTransform.position.x;
        float totalDist = endX - startX;
        if (totalDist <= 0.1f) return;

        float currentProgress = (currentX - startX) / totalDist;
        currentProgress = Mathf.Clamp01(currentProgress);

        progressSlider.value = currentProgress;

        if (currentProgress > historicalMaxProgress)
        {
            historicalMaxProgress = currentProgress;
            PlayerPrefs.SetFloat(levelKey, historicalMaxProgress);
            PlayerPrefs.Save();
            UpdateMaxProgressMarker();
        }
    }
}
