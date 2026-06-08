using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndRaceManagerUI : MonoBehaviour
{
    public Canvas EndRaceCanvas;
    public GameObject EndRacePanel;
    public GameObject RewardsPanel;
    public TextMeshProUGUI totalReward;
    public TextMeshProUGUI startReward;
    public TextMeshProUGUI bikeReward;
    public TextMeshProUGUI PanelTitle;
    public AudioSource audioSource;
    public AudioClip tickSound;
    public Button Restart;
    public Button ExitToMenu;
    [Header("Settings")]
    public float animDuration = 0.8f;
    public string wintext = "Отличные трюки!";
    public string losetext = "Разбился!";

    private int currentTotal = 0;

    private UISwipeAnimate endraceA;




    void Start()
    {
        EndRaceCanvas.enabled = false;
        endraceA = EndRacePanel.GetComponent<UISwipeAnimate>();
        totalReward.enabled = false;
        startReward.gameObject.SetActive(false);
        bikeReward.gameObject.SetActive(false);
        Restart.onClick.AddListener(RestartRace);

        totalReward.text = "0";
    }


    void Update()
    {

    }

    public void ShowSucceful(int overallReward, int otherReward)
    {
        EndRaceCanvas.enabled = true;
        totalReward.enabled = true;
        PanelTitle.text = wintext;

        endraceA.OpenMenu();
        PlayWinAnimation(overallReward-otherReward, otherReward);
    }
    public void ShowCrash()
    {
        EndRaceCanvas.enabled = true;
        totalReward.text = "Lose";
        totalReward.enabled = true;
        PanelTitle.text = losetext;
        endraceA.OpenMenu();

    }

    public void PlayWinAnimation(int startAmount, int extraAmount)
    {

        Sequence winSequence = DOTween.Sequence();

        int lastValueForSound = currentTotal;
        extraAmount = startAmount+extraAmount;


        winSequence.AppendCallback(() => {
            startReward.gameObject.SetActive(true);
            startReward.transform.localScale = Vector3.zero;
            startReward.text = $"+{startAmount}";
        });
        winSequence.Append(startReward.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));


        winSequence.Append(DOTween.To(() => currentTotal, x => {
            currentTotal = x;
            totalReward.text = currentTotal.ToString();

            if (currentTotal != lastValueForSound) {
                PlayTick();
                lastValueForSound = currentTotal;
            }
        }, startAmount, animDuration));


        winSequence.AppendInterval(0.3f);


        winSequence.AppendCallback(() => {
            bikeReward.gameObject.SetActive(true);
            bikeReward.text = $"+{extraAmount-startAmount}";
            bikeReward.transform.localScale = Vector3.zero;

        });


        winSequence.Append(bikeReward.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));


        int finalTotal = currentTotal + extraAmount;
        winSequence.Join(DOTween.To(() => currentTotal, x => {
            currentTotal = x;
            totalReward.text = currentTotal.ToString();

            if (currentTotal != lastValueForSound) {
                PlayTick();
                lastValueForSound = currentTotal;
            }
        }, finalTotal, animDuration));


        winSequence.OnComplete(() => {
            totalReward.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f);
        });
    }

    public void RestartRace()
    {

        endraceA.CloseMenu();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void PlayTick()
    {
        if (audioSource != null && tickSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(tickSound);
        }
    }
}
