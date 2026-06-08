using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Ссылки")]
    public Text scoreText;           
    public Text multiplierText;      

    [Header("Настройки Очков")]
    public float basePointsPerSecond = 5f;   
    public float minWheelieAngle = 15f;      
    public float dangerAngleStart = 40f;     
    public float dangerAngleMax = 65f;       
    public float maxDangerMultiplier = 5.0f; 
    
    [Header("Тайминги")]
    public float dangerDelaySeconds = 3.0f;  

    [Header("Дебаг")]
    public float currentTotalScore = 0f;
    private float dangerTimer = 0f;          
    private BikeController activeBike;

    private void OnEnable()
    {
        EventManager.OnPlayerSpawned += InitializeBike;
        if (EventManager.ActivePlayer != null)
        {
            InitializeBike(EventManager.ActivePlayer);
        }
    }

    private void OnDisable()
    {
        EventManager.OnPlayerSpawned -= InitializeBike;
    }

    public void InitializeBike(Transform bikeTransform)
    {
        activeBike = bikeTransform.GetComponent<BikeController>();
        currentTotalScore = 0f;
        dangerTimer = 0f;
        UpdateUI(1f, false);
    }

    private void Update()
    {
        if (activeBike == null) return;

        CalculateStuntScore();
    }

    public bool IsOnGround()
    {
        float angle = activeBike.currentPitchAngle;
        return angle >= -35f && angle < minWheelieAngle;
    }

    private void CalculateStuntScore()
    {
        float angle = activeBike.currentPitchAngle;

        if (angle > minWheelieAngle)
        {
            float currentMultiplier = 1f;
            bool isDangerous = false;

            if (angle > dangerAngleStart)
            {
                dangerTimer += Time.deltaTime;

                if (dangerTimer >= dangerDelaySeconds)
                {
                    isDangerous = true;
                    float dangerFactor = Mathf.InverseLerp(dangerAngleStart, dangerAngleMax, angle);
                    currentMultiplier = Mathf.Lerp(1f, maxDangerMultiplier, dangerFactor);
                }
            }
            else
            {
                dangerTimer = 0f;
            }

            float pointsEarned = basePointsPerSecond * currentMultiplier * Time.deltaTime;
            currentTotalScore += pointsEarned;

            UpdateUI(currentMultiplier, isDangerous);
        }
        else
        {
            dangerTimer = 0f;
            UpdateUI(1f, false); 
        }
    }

    private void UpdateUI(float multiplier, bool isDangerous)
    {
        float angle = activeBike.currentPitchAngle;

        if (scoreText != null)
        {
            if (angle > minWheelieAngle)
            {
                scoreText.enabled = true;
                
                if (angle > dangerAngleStart) scoreText.color = Color.red;
                else scoreText.color = Color.white;
                
                scoreText.text = Mathf.FloorToInt(currentTotalScore).ToString("N0") + " PTS";
            }
            else
            {
                scoreText.enabled = false;
            }
        } 

        if (multiplierText != null)
        {
            if (isDangerous)
            {
                multiplierText.gameObject.SetActive(true);
                multiplierText.text = "x" + multiplier.ToString("F1") + " DANGER!";
                multiplierText.color = Color.Lerp(Color.yellow, Color.red, (multiplier - 1f) / (maxDangerMultiplier - 1f));
            }
            else
            {
                if (multiplierText.gameObject.activeSelf)
                {
                    multiplierText.gameObject.SetActive(false);
                }
            }
        }
    }
}
