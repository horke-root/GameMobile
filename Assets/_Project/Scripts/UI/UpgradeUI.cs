using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public TextMeshProUGUI coinsText;

    [Header("Engine Upgrade")]
    public TextMeshProUGUI engineLevelText;
    public TextMeshProUGUI engineCostText;
    public Button engineUpgradeButton;

    [Header("Primary Drive Upgrade")]
    public TextMeshProUGUI primaryDriveLevelText;
    public TextMeshProUGUI primaryDriveCostText;
    public Button primaryDriveUpgradeButton;

    private void Start()
    {
        if (engineUpgradeButton != null)
        {
            engineUpgradeButton.onClick.AddListener(UpgradeEngine);
        }
        if (primaryDriveUpgradeButton != null)
        {
            primaryDriveUpgradeButton.onClick.AddListener(UpgradePrimaryDrive);
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        int engineLevel = PlayerPrefs.GetInt("EngineLevel", 1);
        int primaryDriveLevel = PlayerPrefs.GetInt("PrimaryDriveLevel", 1);

        int engineCost = GetUpgradeCost(engineLevel);
        int primaryDriveCost = GetUpgradeCost(primaryDriveLevel);

        if (coinsText != null)
        {
            coinsText.text = FormatValue(coins);
        }

        if (engineLevelText != null)
        {
            engineLevelText.text = "Lvl " + engineLevel;
        }

        if (engineCostText != null)
        {
            engineCostText.text = FormatValue(engineCost);
        }

        if (primaryDriveLevelText != null)
        {
            primaryDriveLevelText.text = "Lvl " + primaryDriveLevel;
        }

        if (primaryDriveCostText != null)
        {
            primaryDriveCostText.text = FormatValue(primaryDriveCost);
        }

        if (engineUpgradeButton != null)
        {
            engineUpgradeButton.interactable = coins >= engineCost;
        }

        if (primaryDriveUpgradeButton != null)
        {
            primaryDriveUpgradeButton.interactable = coins >= primaryDriveCost;
        }
    }

    private int GetUpgradeCost(int level)
    {
        return 2 * level * level + 16 * level + 32;
    }

    private void UpgradeEngine()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        int engineLevel = PlayerPrefs.GetInt("EngineLevel", 1);
        int cost = GetUpgradeCost(engineLevel);

        if (coins >= cost)
        {
            PlayerPrefs.SetInt("Coins", coins - cost);
            PlayerPrefs.SetInt("EngineLevel", engineLevel + 1);
            PlayerPrefs.Save();

            ApplyUpgradesToBike();
            UpdateUI();

            if (VirtualBalanceUI.Instance != null)
            {
                VirtualBalanceUI.Instance.RefreshBalance();
            }
        }
    }

    private void UpgradePrimaryDrive()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        int primaryDriveLevel = PlayerPrefs.GetInt("PrimaryDriveLevel", 1);
        int cost = GetUpgradeCost(primaryDriveLevel);

        if (coins >= cost)
        {
            PlayerPrefs.SetInt("Coins", coins - cost);
            PlayerPrefs.SetInt("PrimaryDriveLevel", primaryDriveLevel + 1);
            PlayerPrefs.Save();

            ApplyUpgradesToBike();
            UpdateUI();

            if (VirtualBalanceUI.Instance != null)
            {
                VirtualBalanceUI.Instance.RefreshBalance();
            }
        }
    }

    private void ApplyUpgradesToBike()
    {
        var raceService = FindFirstObjectByType<RaceService>();
        if (raceService != null)
        {
            var bikeController = FindFirstObjectByType<BikeController>();
            if (bikeController != null)
            {
                raceService.ApplyUpgrades(bikeController);
            }
        }
    }

    private string FormatValue(float value)
    {
        if (value >= 1000000)
        {
            return (value / 1000000f).ToString("0.##") + "M";
        }
        if (value >= 1000)
        {
            return (value / 1000f).ToString("0.##") + "k";
        }
        return value.ToString();
    }
}
