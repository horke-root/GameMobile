using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedometerUI : MonoBehaviour
{
    [Header("Ссылки на контроллер")]
    public BikeController bike;

    [Header("Спидометр (Скорость)")]
    public RectTransform speedArrow;
    public float minSpeedAngle = 180f;
    public float maxSpeedAngle = -90f;
    public float maxSpeedValue = 200f;

    [Header("Тахометр (Обороты)")]
    public RectTransform rpmArrow;
    public float minRpmAngle = 180f;
    public float maxRpmAngle = -90f;
    public int maxRPM = 12000;

    [Header("Передача")]
    public TextMeshProUGUI gearText;

    private void OnEnable()
    {
        Debug.Log($"[SpeedometerUI] OnEnable() called. Subscribing. ActivePlayer={(EventManager.ActivePlayer != null ? EventManager.ActivePlayer.name : "null")}");
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

    void OnPlayerSpawned(Transform bike)
    {
        this.bike = bike.GetComponent<BikeController>();
        Debug.Log($"[SpeedometerUI] OnPlayerSpawned received! bike={this.bike != null}");
    }

    private void Update()
    {
        if (bike == null) return;

        UpdateSpeedometer();
        UpdateTachometer();
        UpdateGearDisplay();
    }

    private void UpdateSpeedometer()
    {
        if (speedArrow == null) return;
        float speedRatio = Mathf.Clamp01(bike.currentSpeedKmh / maxSpeedValue);
        float targetAngle = Mathf.Lerp(minSpeedAngle, maxSpeedAngle, speedRatio);
        speedArrow.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }

    private void UpdateTachometer()
    {
        if (rpmArrow == null) return;
        float rpmRatio = Mathf.Clamp01(bike.currentEngineRPM / maxRPM);
        float targetAngle = Mathf.Lerp(minRpmAngle, maxRpmAngle, rpmRatio);
        rpmArrow.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }

    private void UpdateGearDisplay()
    {
        if (gearText == null) return;

        if (bike.currentGear == 0)
        {
            gearText.text = "N";
            gearText.color = Color.green;
        }
        else
        {
            gearText.text = bike.currentGear.ToString();
            gearText.color = Color.white;
        }
    }
}
