using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MotorcycleEngine : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public WheelJoint2D frontWheel;
    public WheelJoint2D rearWheel;

    [Header("Engine Settings")]
    public float maxHP = 10f;             
    public float maxRPM = 12000f;         
    public int[] gearRatios = { 1, 2, 3, 4 }; 
    public float[] speedLimits = { 20, 40, 60, 80 }; 

    [Header("Physics Settings")]
    public float torqueMultiplier = 50f;  
    public float dragPerGear = 0.5f;      
    public Vector2 cmDefault = new Vector2(0, -0.5f);  
    public Vector2 cmWheelie = new Vector2(0, 0);      

    private float currentHP = 0f;
    public int currentGear = 0;
    private float throttleInput = 0f;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.centerOfMass = cmDefault;
    }

    void Update()
    {
        throttleInput = Input.GetAxis("Vertical");  

        HandleGearShift();
        HandleWheelie();
    }

    void FixedUpdate()
    {
        ApplyMotorTorque();
        ApplyNaturalDrag();
    }

    private float tiltInput = 0f;

    public void SetThrottle(float value)
    {
        throttleInput = value; // 0 или 1
    }

    public void SetTilt(float value)
    {
        tiltInput = value; // -1 = наклон вперед, 1 = наклон назад
    }

    void HandleWheelie()
    {
        // используем tiltInput
        if (tiltInput > 0)
        {
            rb.centerOfMass = Vector2.Lerp(rb.centerOfMass, cmWheelie, 0.1f);
            rb.AddTorque(-50f * tiltInput);
        }
        else if (tiltInput < 0)
        {
            rb.centerOfMass = Vector2.Lerp(rb.centerOfMass, cmDefault + new Vector2(0, -0.2f), 0.1f);
            rb.AddTorque(-50f * tiltInput);
        }
        else
        {
            rb.centerOfMass = Vector2.Lerp(rb.centerOfMass, cmDefault, 0.1f);
        }
    }

    void HandleGearShift()
    {
        float speed = rb.linearVelocity.magnitude * 3.6f; // m/s -> km/h

        // Автоматическое переключение передач
        if (currentGear < gearRatios.Length - 1 && speed > speedLimits[currentGear])
        {
            currentGear++;
        }
        else if (currentGear > 0 && speed < speedLimits[currentGear - 1] * 0.7f)
        {
            currentGear--;
        }
    }

    void ApplyMotorTorque()
    {
        // Преобразуем hp в крутящий момент (примерное приближение)
        float torque = throttleInput * maxHP * torqueMultiplier / gearRatios[currentGear];

        // Применяем крутящий момент к заднему колесу
        JointMotor2D motor = rearWheel.motor;
        motor.motorSpeed = -torque; // - для правильного вращения
        motor.maxMotorTorque = Mathf.Abs(torque) * 1000f; 
        rearWheel.motor = motor;

        // Для фронтального колеса обычно меньший момент
        motor = frontWheel.motor;
        motor.motorSpeed = -torque * 0.5f;
        motor.maxMotorTorque = Mathf.Abs(torque) * 500f;
        frontWheel.motor = motor;
    }

    void ApplyNaturalDrag()
    {
        // Замедление, если газ отпущен
        if (throttleInput <= 0)
        {
            float drag = dragPerGear * (currentGear + 1);
            rb.linearVelocity *= 1f - drag * Time.fixedDeltaTime;
        }
    }

    /*void HandleWheelie()
    {
        // Если наклоняемся назад (например, tilt backward)
        if (Input.GetKey(KeyCode.Space)) // пример, для мобилки будет virtual button
        {
            rb.centerOfMass = Vector2.Lerp(rb.centerOfMass, cmWheelie, 0.1f);
            rb.AddTorque(-50f); // импульс для дыбы
        }
        else
        {
            rb.centerOfMass = Vector2.Lerp(rb.centerOfMass, cmDefault, 0.1f);
        }
    }*/
}