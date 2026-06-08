using UnityEngine;
using TMPro;
using DG.Tweening;

public class VirtualBalanceUI : MonoBehaviour
{
    public static VirtualBalanceUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;

    private int virtualBalance = 0;
    private Tween countTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (moneyText == null)
        {
            moneyText = GetComponent<TextMeshProUGUI>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        virtualBalance = PlayerPrefs.GetInt("Coins", 0);
        UpdateText(virtualBalance);
    }

    public void RefreshBalance()
    {
        virtualBalance = PlayerPrefs.GetInt("Coins", 0);
        UpdateText(virtualBalance);
    }

    public void AddMoneyAnimated(int amount)
    {
        if (amount <= 0) return;

        int targetBalance = virtualBalance + amount;

        if (countTween != null)
        {
            countTween.Kill();
        }

        if (moneyText != null)
        {
            moneyText.transform.DOKill();
            moneyText.transform.localScale = Vector3.one;
            moneyText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.4f, 8, 1f)
                .OnComplete(() => moneyText.transform.localScale = Vector3.one);
        }

        if (audioSource != null && collectSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(collectSound);
        }

        countTween = DOTween.To(() => virtualBalance, x =>
        {
            virtualBalance = x;
            UpdateText(virtualBalance);
        }, targetBalance, 1.2f).SetEase(Ease.OutQuad);
    }

    private void UpdateText(int balance)
    {
        if (moneyText != null)
        {
            moneyText.text = balance.ToString();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        if (countTween != null)
        {
            countTween.Kill();
        }
    }
}
