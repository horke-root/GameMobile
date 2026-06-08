using UnityEngine;
using UnityEngine.UI;

public class TemperatureUI : MonoBehaviour
{
    [Header("UI")]
    public Slider tempSlider;
    public Image fillImage;

    private BikeController bike;

    private void OnEnable()
    {
        Debug.Log($"[TemperatureUI] OnEnable() called. ActivePlayer={(EventManager.ActivePlayer != null ? EventManager.ActivePlayer.name : "null")}");
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
        bike = bikeTransform.GetComponent<BikeController>();
        Debug.Log($"[TemperatureUI] OnPlayerSpawned received! bike={bike != null}, enableTemp={bike?.enableTemperature}");
    }

    private void Update()
    {
        if (bike == null || tempSlider == null) return;

        if (!bike.enableTemperature)
        {
            if (tempSlider.gameObject.activeSelf) tempSlider.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!tempSlider.gameObject.activeSelf) tempSlider.gameObject.SetActive(true);
        }

        float ratio = bike.engineTemperature / bike.maxEngineTemperature;
        tempSlider.value = ratio;

        if (fillImage != null)
        {
            if (ratio < 0.5f)
            {
                fillImage.color = Color.green;
            }
            else if (ratio < 0.75f)
            {
                float t = Mathf.InverseLerp(0.5f, 0.75f, ratio);
                fillImage.color = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), t);
            }
            else
            {
                fillImage.color = Color.red;
            }
        }
    }
}
