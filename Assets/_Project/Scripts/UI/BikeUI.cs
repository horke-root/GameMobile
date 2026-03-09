using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Подключаем интерфейсы для удержания
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public MotorcycleEngine bike;
    public bool isThrottleButton = false; // газ
    public bool isBrakeButton = false;    // тормоз
    public bool isTiltForward = false;
    public bool isTiltBack = false;

    public Text speedText;
    public Text gearText;

    private bool isPressed = false;

    void Update()
    {
        if (isPressed)
        {
            if (isThrottleButton)
                bike.SetThrottle(1f);
            else if (isBrakeButton)
                bike.SetThrottle(0f); // или отдельный тормоз
            else if (isTiltForward)
                bike.SetTilt(-1f);
            else if (isTiltBack)
                bike.SetTilt(1f);
        }
        else
        {
            // когда не нажата кнопка — сбрасываем наклон
            if (isTiltForward || isTiltBack)
                bike.SetTilt(0f);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    private void FixedUpdate()
    {
        // Обновляем скорость и передачу
        speedText.text = $"Speed: {(bike.rb.linearVelocity.magnitude * 3.6f):F0} km/h";
        gearText.text = $"Gear: {bike.currentGear + 1}";
    }
}