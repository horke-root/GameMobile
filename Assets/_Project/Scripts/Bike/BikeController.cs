using System.Collections.Generic;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    [Header("Данные Двигателя")]
    public AnimationCurve activeEngineCurve;
    public float maxRPM = 8500f;
    public float maxHP = 4.5f;
    public float idleRPM = 1500f;
    public float engineBrakingFactor = 2.0f;

    [Header("Сцепление и Статус")]
    public bool isEngineRunning = true;
    public bool isEngineBroken = false;
    public float stallRPM = 1000f;
    [Range(0f, 1f)] public float clutchLock = 0f;

    [Header("Трансмиссия (АКПП)")]
    public float primaryDriveRatio = 3.72f;
    public int frontSprocket = 14;
    public int rearSprocket = 41;

    public float[] gearRatios = { 0f, 3.27f, 1.93f, 1.35f, 1.04f };
    public int currentGear = 0;

    public float shiftUpRPM = 7500f;
    public float shiftDownRPM = 3000f;
    public float shiftDelay = 0.3f;
    private float shiftTimer = 0f;

    public float shiftCooldown = 1.0f;
    private float currentCooldown = 0f;

    public bool manualTransmission = false;

    [Header("Стант: Физика и Колеса")]
    public Rigidbody2D frameRb;
    public Rigidbody2D rearWheelRb;
    public Rigidbody2D frontWheelRb;

    public Vector2 centerOfMassOffset = new Vector2(-0.1f, -0.3f);
    public float wheelRadius = 0.28f;
    public float arcadeTorqueMultiplier = 1.5f;

    [Header("Аэродинамика")]
    public float airResistance = 0.5f;
    public float rollingResistance = 0.1f;

    [Header("Стант: Ассистент Вилли")]
    public bool enableWheelieAssist = true;
    public bool enableEasyStuntMode = false;
    public bool enableAutoThrottleFeathering = true;
    public bool enableAdaptiveThrottleSpeed = true;
    public bool enableSmoothStuntPull = true;
    public bool enableGearBasedStuntScaling = false;
    public float balanceAngle = 45f;
    public float criticalAngle = 65f;

    [Header("Управление (Ввод)")]
    public MobileButton gasButton;
    public MobileButton rearBrakeButton;
    public MobileButton frontBrakeButton;
    public MobileButton stuntButton;
    public MobileButton shiftUpButton;
    public MobileButton shiftDownButton;
    public float stuntPullTorque = 150f;

    [Range(0f, 1f)] public float throttleInput = 0f;
    public float throttleSpeed = 5f;
    public float stuntThrottleSpeed = 15f;
    public float mechanicalBrakePower = 30f;

    [Header("Состояние деталей (Износ)")]
    [Range(0f, 1f)] public float engineCondition = 1f;
    [Range(0f, 1f)] public float wheelsCondition = 1f;

    [Header("Температура двигателя (Enduro)")]
    public float engineTemperature = 20f;
    public float maxEngineTemperature = 120f;
    public float baseHeatRate = 10f;
    public float ambientCoolingRate = 5f;
    public float engineHeatMultiplier = 1.0f;
    private const float OVERHEAT_RPM_THRESHOLD = 0.85f;
    public bool isOverheated = false;
    public bool enableTemperature = false;

    public System.Action onOverheat;

    [Header("Дебаг")]
    public float currentEngineRPM;
    public float currentSpeedKmh;
    public float currentPitchAngle;

    private bool InputGas
    {
        get
        {
            bool pressed = gasButton != null && gasButton.IsPressed;
#if UNITY_EDITOR
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                pressed |= UnityEngine.InputSystem.Keyboard.current.wKey.isPressed || UnityEngine.InputSystem.Keyboard.current.upArrowKey.isPressed;
            }
#endif
            return pressed;
        }
    }

    private bool InputStunt
    {
        get
        {
            bool pressed = stuntButton != null && stuntButton.IsPressed;
#if UNITY_EDITOR
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                pressed |= UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed;
            }
#endif
            return pressed;
        }
    }

    private bool InputRearBrake
    {
        get
        {
            bool pressed = rearBrakeButton != null && rearBrakeButton.IsPressed;
#if UNITY_EDITOR
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                pressed |= UnityEngine.InputSystem.Keyboard.current.sKey.isPressed || UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed;
            }
#endif
            return pressed;
        }
    }

    private bool InputFrontBrake
    {
        get
        {
            bool pressed = frontBrakeButton != null && frontBrakeButton.IsPressed;
#if UNITY_EDITOR
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                pressed |= UnityEngine.InputSystem.Keyboard.current.aKey.isPressed || UnityEngine.InputSystem.Keyboard.current.leftArrowKey.isPressed;
            }
#endif
            return pressed;
        }
    }

    private bool InputShiftUp
    {
        get
        {
            bool pressed = shiftUpButton != null && shiftUpButton.IsPressed;
#if UNITY_EDITOR
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                pressed |= UnityEngine.InputSystem.Keyboard.current.eKey.isPressed;
            }
#endif
            return pressed;
        }
    }

    private bool InputShiftDown
    {
        get
        {
            bool pressed = shiftDownButton != null && shiftDownButton.IsPressed;
#if UNITY_EDITOR
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                pressed |= UnityEngine.InputSystem.Keyboard.current.qKey.isPressed;
            }
#endif
            return pressed;
        }
    }

    private float FinalDriveRatio => (float)rearSprocket / frontSprocket;
    private float CurrentTotalRatio => (currentGear == 0) ? 0f : primaryDriveRatio * gearRatios[currentGear] * FinalDriveRatio;

    private bool lastShiftUpPressed = false;
    private bool lastShiftDownPressed = false;

    private void Start()
    {
        if (frameRb != null) frameRb.centerOfMass = centerOfMassOffset;
    }

    private void Update()
    {
        HandleInputs();

        bool shiftUpPressed = InputShiftUp;
        if (shiftUpPressed && !lastShiftUpPressed)
        {
            ShiftUp();
        }
        lastShiftUpPressed = shiftUpPressed;

        bool shiftDownPressed = InputShiftDown;
        if (shiftDownPressed && !lastShiftDownPressed)
        {
            ShiftDown();
        }
        lastShiftDownPressed = shiftDownPressed;
    }

    private void FixedUpdate()
    {
        if (frameRb != null)
        {
            currentPitchAngle = frameRb.transform.eulerAngles.z;
            if (currentPitchAngle > 180f) currentPitchAngle -= 360f;
        }
        if (shiftTimer > 0) shiftTimer -= Time.fixedDeltaTime;
        if (currentCooldown > 0) currentCooldown -= Time.fixedDeltaTime;

        CalculateSpeedAndRPM();
        HandleAutomaticTransmission();
        ApplyEnginePhysics();
        ApplyMechanicalBrakes();
        ApplyAerodynamics();
        UpdateEngineTemperature();
    }

    private void ApplyAerodynamics()
    {
        float speed = frameRb.linearVelocity.magnitude;

        if (speed < 0.1f) return;

        float dragForceMagnitude = speed * speed * airResistance;
        float rollingForceMagnitude = speed * rollingResistance;

        Vector2 dragForce = -frameRb.linearVelocity.normalized * (dragForceMagnitude + rollingForceMagnitude);

        frameRb.AddForce(dragForce);
    }

    private void UpdateEngineTemperature()
    {
        if (!enableTemperature) return;

        float rpmRatio = currentEngineRPM / maxRPM;

        if (rpmRatio >= OVERHEAT_RPM_THRESHOLD && throttleInput > 0.1f)
        {
            float heatPerSecond = baseHeatRate * engineHeatMultiplier;
            float overheatIntensity = Mathf.InverseLerp(OVERHEAT_RPM_THRESHOLD, 1f, rpmRatio);
            engineTemperature += heatPerSecond * overheatIntensity * Time.fixedDeltaTime;
        }
        else
        {
            engineTemperature -= ambientCoolingRate * Time.fixedDeltaTime;
        }

        engineTemperature = Mathf.Clamp(engineTemperature, 0f, maxEngineTemperature);

        if (engineTemperature >= maxEngineTemperature && !isOverheated)
        {
            isOverheated = true;
            isEngineRunning = false;
            onOverheat?.Invoke();
        }

        if (isOverheated && engineTemperature < maxEngineTemperature * 0.8f)
        {
            isOverheated = false;
        }
    }

    private void HandleInputs()
    {
        bool isGasPressed = InputGas;
        bool isStuntPressed = InputStunt;

        float currentAngle = 0f;
        if (frameRb != null)
        {
            currentAngle = frameRb.transform.eulerAngles.z;
            if (currentAngle > 180f) currentAngle -= 360f;
        }

        bool isInStuntMode = currentAngle > 10f || isStuntPressed;
        float activeThrottleSpeed = isInStuntMode ? stuntThrottleSpeed : throttleSpeed;

        if (isInStuntMode && enableAdaptiveThrottleSpeed)
        {
            float powerFactor = Mathf.Clamp(maxHP, 2f, 15f);
            float speedScale = Mathf.InverseLerp(15f, 2f, powerFactor);
            activeThrottleSpeed = Mathf.Lerp(3f, stuntThrottleSpeed, speedScale);
        }

        float targetThrottle = (isGasPressed || isStuntPressed) ? 1f : 0f;

        if (enableAutoThrottleFeathering && isInStuntMode && enableWheelieAssist && targetThrottle > 0f)
        {
            float featherStartAngle = balanceAngle - 10f;
            if (currentAngle > featherStartAngle)
            {
                float t = Mathf.InverseLerp(featherStartAngle, balanceAngle, currentAngle);
                float maxAllowedThrottle = Mathf.Lerp(1f, 0.2f, t);
                targetThrottle = Mathf.Min(targetThrottle, maxAllowedThrottle);
            }
        }

        throttleInput = Mathf.MoveTowards(throttleInput, targetThrottle, Time.deltaTime * activeThrottleSpeed);

        if (InputRearBrake)
        {
            throttleInput = 0f;
        }
    }

    private void ApplyMechanicalBrakes()
    {
        if (InputRearBrake)
            rearWheelRb.AddTorque(-Mathf.Sign(rearWheelRb.angularVelocity) * mechanicalBrakePower);

        if (InputFrontBrake)
            frontWheelRb.AddTorque(-Mathf.Sign(frontWheelRb.angularVelocity) * mechanicalBrakePower * 1.5f);
    }

    private void CalculateSpeedAndRPM()
    {
        currentSpeedKmh = frameRb.linearVelocity.magnitude * 3.6f;

        float wheelRevsPerSecond = Mathf.Abs(rearWheelRb.angularVelocity) / 360f;
        float wheelRPM = wheelRevsPerSecond * 60f;

        if (!isEngineRunning)
        {
            currentEngineRPM = Mathf.MoveTowards(currentEngineRPM, 0f, 4000f * Time.fixedDeltaTime);

            if (currentSpeedKmh < 1f && throttleInput < 0.1f && !isEngineBroken)
            {
                isEngineRunning = true;
                currentEngineRPM = idleRPM;
                currentGear = 1;
            }
            return;
        }

        float mechanicalRPM = wheelRPM * CurrentTotalRatio;
        float targetGasRPM = idleRPM + (throttleInput * (maxRPM - idleRPM));

        if (shiftTimer > 0 || currentGear == 0)
        {
            clutchLock = 0f;
        }
        else
        {
            clutchLock = Mathf.InverseLerp(2500f, 4000f, currentEngineRPM);
            if (mechanicalRPM > 4000f) clutchLock = 1f;
        }

        float targetRPM = Mathf.Lerp(targetGasRPM, mechanicalRPM, clutchLock);
        float inertia = (targetRPM > currentEngineRPM) ? 8000f : 12000f;
        currentEngineRPM = Mathf.MoveTowards(currentEngineRPM, targetRPM, inertia * Time.fixedDeltaTime);

        float actualStallRPM = engineCondition < 0.2f ? stallRPM * 1.5f : stallRPM;

        if (clutchLock > 0.8f && currentEngineRPM < actualStallRPM && throttleInput > 0f)
        {
            isEngineRunning = false;
        }

        currentEngineRPM = Mathf.Clamp(currentEngineRPM, 0f, maxRPM);
    }

    private void HandleAutomaticTransmission()
    {
        if (manualTransmission) return;
        if (!isEngineRunning || shiftTimer > 0) return;

        bool isStuntButtonPressed = InputStunt;

        if (currentGear == 0)
        {
            if (throttleInput > 0.1f || isStuntButtonPressed)
            {
                currentGear = 1;
                shiftTimer = shiftDelay;
            }
            return;
        }

        float currentAngle = 0f;
        if (frameRb != null)
        {
            currentAngle = frameRb.transform.eulerAngles.z;
            if (currentAngle > 180f) currentAngle -= 360f;
        }

        bool isPhysicallyStunting = currentAngle > 10f || isStuntButtonPressed;

        if (isPhysicallyStunting) return;

        if (currentCooldown > 0) return;

        if (currentEngineRPM > shiftUpRPM && currentGear < gearRatios.Length - 1 && clutchLock > 0.8f)
        {
            currentGear++;
            shiftTimer = shiftDelay;
            currentCooldown = shiftCooldown;
            currentEngineRPM -= 1500f;
        }
        else if (currentEngineRPM < shiftDownRPM && currentGear > 1)
        {
            currentGear--;
            shiftTimer = shiftDelay;
            currentCooldown = shiftCooldown;
        }
    }

    public void ShiftUp()
    {
        if (!isEngineRunning || currentGear >= gearRatios.Length - 1) return;

        currentGear++;
        shiftTimer = shiftDelay;
        currentCooldown = shiftCooldown;
        currentEngineRPM = Mathf.Max(idleRPM, currentEngineRPM - 1500f);
    }

    public void ShiftDown()
    {
        if (!isEngineRunning || currentGear <= 0) return;

        currentGear--;
        shiftTimer = shiftDelay;
        currentCooldown = shiftCooldown;
        if (currentGear > 0)
        {
            currentEngineRPM = Mathf.Min(maxRPM, currentEngineRPM + 1000f);
        }
    }

    private void ApplyEnginePhysics()
    {
        if (!isEngineRunning || shiftTimer > 0 || currentGear == 0) return;

        float wheelRevsPerSecond = Mathf.Abs(rearWheelRb.angularVelocity) / 360f;
        float wheelRPM = wheelRevsPerSecond * 60f;
        float mechanicalRPM = wheelRPM * CurrentTotalRatio;

        float normalizedRPM = currentEngineRPM / maxRPM;
        float powerPercentage = activeEngineCurve.Evaluate(normalizedRPM);
        float currentHP = powerPercentage * maxHP * Mathf.Max(engineCondition, 0.01f);

        float engineTorque = (currentHP * 7120f) / Mathf.Max(currentEngineRPM, 1f);
        float torqueAtWheel = engineTorque * CurrentTotalRatio * clutchLock * arcadeTorqueMultiplier * wheelsCondition;

        if (mechanicalRPM >= maxRPM)
        {
            torqueAtWheel = 0f;
        }

        if (throttleInput > 0.05f)
        {
            float finalTorque = torqueAtWheel * throttleInput;

            float currentAngle = frameRb.transform.eulerAngles.z;
            if (currentAngle > 180f) currentAngle -= 360f;

            float gearMultiplier = 1f;
            if (enableGearBasedStuntScaling && currentGear > 1 && currentGear < gearRatios.Length)
            {
                gearMultiplier = Mathf.Sqrt(gearRatios[currentGear] / gearRatios[1]);
            }

            float actualStuntTorque = stuntPullTorque * gearMultiplier;

            if (InputStunt && frameRb != null)
            {
                if (enableEasyStuntMode)
                {
                    float t = Mathf.InverseLerp(5f, balanceAngle, currentAngle);
                    float currentStuntPull = Mathf.Lerp(actualStuntTorque, actualStuntTorque * 0.05f, t);

                    if (frameRb.angularVelocity > 1.5f && currentAngle > 15f)
                    {
                        frameRb.angularVelocity = Mathf.Lerp(frameRb.angularVelocity, 1.5f, 5f * Time.fixedDeltaTime);
                    }

                    if (currentAngle < balanceAngle) frameRb.AddTorque(currentStuntPull);
                }
                else if (enableWheelieAssist)
                {
                    float currentStuntPull = actualStuntTorque;

                    if (enableSmoothStuntPull)
                    {
                        float smoothStart = balanceAngle - 15f;
                        if (currentAngle > smoothStart && currentAngle < balanceAngle)
                        {
                            float t = Mathf.InverseLerp(smoothStart, balanceAngle, currentAngle);
                            currentStuntPull = Mathf.Lerp(actualStuntTorque, actualStuntTorque * 0.1f, t);
                        }
                    }

                    if (currentAngle < balanceAngle) frameRb.AddTorque(currentStuntPull);
                }
                else
                {
                    frameRb.AddTorque(actualStuntTorque);
                }
            }

            if (!enableEasyStuntMode && enableWheelieAssist && frameRb != null && currentAngle > balanceAngle)
            {
                float reductionFactor = 1f - Mathf.InverseLerp(balanceAngle, criticalAngle, currentAngle);
                finalTorque *= reductionFactor;

                if (InputStunt) frameRb.AddTorque(-actualStuntTorque * 0.5f);
            }

            if (finalTorque > 0f)
            {
                rearWheelRb.AddTorque(-finalTorque);
            }
        }
        else if (clutchLock > 0.5f)
        {
            float brakingForce = engineBrakingFactor * CurrentTotalRatio;
            rearWheelRb.AddTorque(-Mathf.Sign(rearWheelRb.angularVelocity) * brakingForce);
        }
    }
}
