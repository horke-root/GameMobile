using UnityEngine;
using TMPro;

public class BikeUI : MonoBehaviour
{
    [Header("Связь с мотоциклом")]
    public BikeController bike;

    [Header("Элементы UI (Текст на Canvas)")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rpmText;
    public TextMeshProUGUI gearText;

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

    void OnPlayerSpawned(Transform bike)
    {
        this.bike = bike.GetComponent<BikeController>();
    }

    private void Update()
    {
        if (bike == null) return;

        UpdateDashboard();
    }

    private void UpdateDashboard()
    {
        if (speedText != null)
        {
            int speed = Mathf.RoundToInt(bike.currentSpeedKmh);
            speedText.text = $"{Mathf.Abs(speed)} км/ч"; 
        }

        if (rpmText != null)
        {
            int rpm = Mathf.RoundToInt(bike.currentEngineRPM);
            rpmText.text = $"{rpm} об/мин";
        }

        if (gearText != null)
        {
            if (bike.currentGear == 0)
            {
                gearText.text = "N";
                gearText.color = Color.green;
            }
            else
            {
                gearText.text = (bike.currentGear).ToString();
                gearText.color = Color.white;
            }
        }
    }
}
