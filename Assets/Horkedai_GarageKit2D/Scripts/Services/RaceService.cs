using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class RaceService : MonoBehaviour
{
    public GameObject bikePrefab;
    private GameObject bike;
    public Vector3 bikeSpawnPosition;
    public float maxDirtness = 0.05f;
    public string racesFolderPath = "Races";

    public AudioSource oneShot;
    public AudioClip fallSound;

    public StatsService statsTracker;
    public EndRaceManagerUI ermUI;

    public EnduroLevelSO enduroLevel;
    private int checkpointsReached = 0;
    private Vector3 lastCheckpointPosition;
    private int accumulatedEnduroMoney = 0;
    public bool enableStuntScoring = true;

    public float virtualBikeHealth = 100f;
    public float maxVirtualBikeHealth = 100f;
    public UnityEngine.UI.Slider healthSlider;

    public UnityEngine.UI.Text checkpointRewardText;
    
    public MobileButton gas;
    public MobileButton stunt;
    public MobileButton frontBrake;
    public MobileButton rearBrake;
    
    public MobileButton shiftUpButton;
    public MobileButton shiftDownButton;

    public ScoreManager scoreManager;

    private Tween crashDelayTween;
    private bool isCrashed = false;

    public bool enableImpactDamage = true;
    public float impactDamageMultiplier = 1.0f;

    void Awake()
    {
        Debug.Log($"[RaceService] Awake() called. Subscribers before reset: {EventManager.GetSubscriberCount()}");
        DOTween.KillAll();
        EventManager.ActivePlayer = null;
        Debug.Log($"[RaceService] Awake() done. Subscribers after reset: {EventManager.GetSubscriberCount()}");
    }

    void Start()
    {
        Debug.Log($"[RaceService] Start() called. Subscribers: {EventManager.GetSubscriberCount()}");

        var mm = MusicManager.Instance;
        if (mm != null)
        {
            mm.StopMusic();
        }

        SpawnBikeInstance();

        if (checkpointRewardText != null)
        {
            checkpointRewardText.gameObject.SetActive(false);
            checkpointRewardText.enabled = false;
        }
    }

    private void SpawnBikeInstance()
    {
        bike = Instantiate(bikePrefab, bikeSpawnPosition, Quaternion.identity);
        lastCheckpointPosition = bike.transform.position;

        var controller = bike.GetComponent<BikeController>();
        if (controller != null)
        {
            controller.gasButton = gas;
            controller.rearBrakeButton = rearBrake;
            controller.stuntButton = stunt;
            controller.frontBrakeButton = frontBrake;
            controller.shiftUpButton = shiftUpButton;
            controller.shiftDownButton = shiftDownButton;

            ApplyUpgrades(controller);

            if (enduroLevel != null)
            {
                controller.enableTemperature = true;
                controller.engineTemperature = enduroLevel.startingTemperature;
                controller.maxEngineTemperature = enduroLevel.maxTemperature;
                controller.ambientCoolingRate = enduroLevel.ambientCoolingRate;
                controller.onOverheat = OnOverheat;
            }
        }

        var human = bike.GetComponent<Human>();
        if (human != null && human.crashDetector != null)
        {
            human.crashDetector.onCrash = OnCrash;
        }

        var impactDetector = bike.GetComponent<ImpactDetector>() ?? bike.AddComponent<ImpactDetector>();
        impactDetector.onHeavyImpact = OnHeavyImpact;

        if (statsTracker != null && controller != null)
        {
            int minAngle = scoreManager != null ? (int)scoreManager.minWheelieAngle : 15;
            statsTracker.StartNewRound(minAngle, controller);
        }

        if (!enableStuntScoring && scoreManager != null)
        {
            scoreManager.gameObject.SetActive(false);
        }

        EventManager.TriggerPlayerSpawned(bike.transform);
    }

    public void ApplyUpgrades(BikeController controller)
    {
        if (controller == null) return;
        int engineLevel = PlayerPrefs.GetInt("EngineLevel", 1);
        int transLevel = PlayerPrefs.GetInt("PrimaryDriveLevel", 1);

        controller.maxHP = 4f + engineLevel;
        controller.primaryDriveRatio = 3.72f - (transLevel - 1) * 0.1f;
    }

    public void OnCrash()
    {
        if (isCrashed) return;
        isCrashed = true;

        var human = bike.GetComponent<Human>();
        if (human != null)
        {
            human.Kill();
            if (Camera.main != null)
            {
                var follow = Camera.main.GetComponent<CameraFollow2D>();
                if (follow != null && human.bodyHingeJoint != null)
                {
                    follow.target = human.bodyHingeJoint.transform;
                }
            }
        }

        if (oneShot != null && fallSound != null)
        {
            oneShot.PlayOneShot(fallSound);
        }

        if (enduroLevel != null)
        {
            int durabilityLoss = Random.Range(enduroLevel.minCrashDurabilityLoss, enduroLevel.maxCrashDurabilityLoss + 1);
            virtualBikeHealth = Mathf.Max(0, virtualBikeHealth - durabilityLoss);

            if (virtualBikeHealth <= 0)
            {
                crashDelayTween = DOVirtual.DelayedCall(3f, () =>
                {
                    if (ermUI != null) ermUI.ShowCrash();
                });
            }
            else
            {
                StartCoroutine(RespawnAtLastCheckpoint());
            }
        }
        else
        {
            crashDelayTween = DOVirtual.DelayedCall(3f, () =>
            {
                if (ermUI != null) ermUI.ShowCrash();
            });
        }
    }

    private void OnHeavyImpact(float impactVelocity)
    {
        if (!enableImpactDamage || isCrashed || enduroLevel == null) return;

        int durabilityLoss = Mathf.FloorToInt(impactVelocity * impactDamageMultiplier);
        if (durabilityLoss <= 0) return;

        virtualBikeHealth = Mathf.Max(0, virtualBikeHealth - durabilityLoss);
        if (virtualBikeHealth <= 0)
        {
            isCrashed = true;
            crashDelayTween = DOVirtual.DelayedCall(3f, () =>
            {
                if (ermUI != null) ermUI.ShowCrash();
            });
        }
    }

    private IEnumerator RespawnAtLastCheckpoint()
    {
        yield return new WaitForSeconds(1.5f);

        if (bike != null)
        {
            Destroy(bike);
        }

        bike = Instantiate(bikePrefab, lastCheckpointPosition, Quaternion.identity);
        isCrashed = false;

        var controller = bike.GetComponent<BikeController>();
        if (controller != null)
        {
            controller.gasButton = gas;
            controller.rearBrakeButton = rearBrake;
            controller.stuntButton = stunt;
            controller.frontBrakeButton = frontBrake;
            controller.shiftUpButton = shiftUpButton;
            controller.shiftDownButton = shiftDownButton;

            ApplyUpgrades(controller);

            if (enduroLevel != null)
            {
                controller.enableTemperature = true;
                controller.engineTemperature = enduroLevel.startingTemperature;
                controller.maxEngineTemperature = enduroLevel.maxTemperature;
                controller.ambientCoolingRate = enduroLevel.ambientCoolingRate;
                controller.onOverheat = OnOverheat;
            }
        }

        var human = bike.GetComponent<Human>();
        if (human != null && human.crashDetector != null)
        {
            human.crashDetector.onCrash = OnCrash;
        }

        var impactDetector = bike.GetComponent<ImpactDetector>() ?? bike.AddComponent<ImpactDetector>();
        impactDetector.onHeavyImpact = OnHeavyImpact;

        if (statsTracker != null && controller != null)
        {
            statsTracker.UpdateBikeReference(controller);
        }

        if (Camera.main != null)
        {
            var follow = Camera.main.GetComponent<CameraFollow2D>();
            if (follow != null)
            {
                follow.target = bike.transform;
            }
        }

        EventManager.TriggerPlayerSpawned(bike.transform);
    }

    private void OnDestroy()
    {
        crashDelayTween?.Kill();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SaveToGarage();
        }

        if (healthSlider != null)
        {
            healthSlider.value = virtualBikeHealth / maxVirtualBikeHealth;
        }
    }

    public void SaveToGarage()
    {
        bool canExit = false;

        if (scoreManager != null && scoreManager.gameObject.activeInHierarchy)
        {
            canExit = scoreManager.IsOnGround();
        }
        else if (bike != null)
        {
            var controller = bike.GetComponent<BikeController>();
            if (controller != null)
            {
                float angle = controller.currentPitchAngle;
                canExit = angle >= -35f && angle < 15f;
            }
        }

        if (canExit)
        {
            ToGarageTrigger();
        }
    }

    private bool isToGarageTriggered = false;

    public void ToGarageTrigger()
    {
        if (isToGarageTriggered) return;
        isToGarageTriggered = true;

        int earned = 0;
        if (enduroLevel == null)
        {
            if (scoreManager != null)
            {
                earned = Mathf.FloorToInt(scoreManager.currentTotalScore);
            }
        }
        else
        {
            earned = accumulatedEnduroMoney;
            if (ermUI != null)
            {
                ermUI.wintext = "Кінець!";
            }
        }

        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        PlayerPrefs.SetInt("Coins", currentCoins + earned);
        PlayerPrefs.Save();

        if (scoreManager != null)
        {
            if (scoreManager.scoreText != null)
            {
                scoreManager.scoreText.enabled = false;
            }
            scoreManager.enabled = false;
        }

        if (ermUI != null)
        {
            ermUI.ShowSucceful(earned, 0);
        }
    }

    public void ToGarage()
    {
        crashDelayTween?.Kill();
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (statsTracker != null)
        {
            statsTracker.StopTracking();
        }

        SceneManager.LoadScene("Login 1");
    }

    public void OnCheckpointReached()
    {
        if (enduroLevel == null || isCrashed) return;

        int rewardIndex = checkpointsReached;
        int rewardAmount = 0;

        if (enduroLevel.checkpointRewards != null && enduroLevel.checkpointRewards.Length > 0)
        {
            if (rewardIndex < enduroLevel.checkpointRewards.Length)
            {
                rewardAmount = enduroLevel.checkpointRewards[rewardIndex];
            }
            else
            {
                rewardAmount = enduroLevel.checkpointRewards[enduroLevel.checkpointRewards.Length - 1];
            }
        }

        checkpointsReached++;

        if (rewardAmount > 0)
        {
            accumulatedEnduroMoney += rewardAmount;
            if (VirtualBalanceUI.Instance != null)
            {
                VirtualBalanceUI.Instance.AddMoneyAnimated(rewardAmount);
            }
        }

        var controller = bike.GetComponent<BikeController>();
        if (controller != null)
        {
            controller.engineTemperature = enduroLevel.checkpointTemperatureReset;
            controller.isOverheated = false;
            controller.isEngineRunning = true;

            controller.throttleInput = 0f;
            if (controller.frameRb != null) controller.frameRb.linearVelocity = Vector2.zero;
            if (controller.rearWheelRb != null) controller.rearWheelRb.angularVelocity = 0f;
            if (controller.frontWheelRb != null) controller.frontWheelRb.angularVelocity = 0f;
            controller.currentEngineRPM = controller.idleRPM;
            controller.currentGear = 0;
        }

        lastCheckpointPosition = bike.transform.position;

        if (checkpointRewardText != null && rewardAmount > 0)
        {
            checkpointRewardText.text = $"+{rewardAmount}$";
            checkpointRewardText.gameObject.SetActive(true);
            checkpointRewardText.enabled = true;

            checkpointRewardText.transform.DOKill();
            checkpointRewardText.DOKill();

            checkpointRewardText.transform.localScale = Vector3.zero;
            Color originalColor = checkpointRewardText.color;
            checkpointRewardText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            Sequence seq = DOTween.Sequence();
            seq.Append(checkpointRewardText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
            seq.Join(checkpointRewardText.DOFade(1f, 0.3f));
            seq.AppendInterval(1.5f);
            seq.Append(checkpointRewardText.transform.DOScale(0.8f, 0.4f).SetEase(Ease.InBack));
            seq.Join(checkpointRewardText.DOFade(0f, 0.4f));
            seq.OnComplete(() =>
            {
                if (checkpointRewardText != null)
                {
                    checkpointRewardText.gameObject.SetActive(false);
                    checkpointRewardText.enabled = false;
                    checkpointRewardText.color = originalColor;
                    checkpointRewardText.transform.localScale = Vector3.one;
                }
            });
        }
    }

    public void OnOverheat()
    {
        if (isCrashed) return;
        isCrashed = true;

        var controller = bike.GetComponent<BikeController>();
        if (controller != null)
        {
            controller.throttleInput = 0f;
            if (controller.frameRb != null) controller.frameRb.linearVelocity = Vector2.zero;
            if (controller.rearWheelRb != null) controller.rearWheelRb.angularVelocity = 0f;
            if (controller.frontWheelRb != null) controller.frontWheelRb.angularVelocity = 0f;
            controller.currentGear = 0;
        }

        if (ermUI != null)
        {
            ermUI.ShowCrash();
        }
    }
}
