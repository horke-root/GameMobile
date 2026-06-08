using System.Collections;
using UnityEngine;





public class EndOfRoad : MonoBehaviour

{
    public RaceService raceService;

    [Header("Настройки времени")]
    [Range(0f, 1f)]
    [SerializeField] private float slowMoScale = 0.3f;
    [SerializeField] private float waitBeforeMenu = 2f;


    private bool isFinished = false;

    private IEnumerator FinishSequence()
    {

        Time.timeScale = slowMoScale;

        Time.fixedDeltaTime = 0.02f * Time.timeScale;






        yield return new WaitForSecondsRealtime(waitBeforeMenu);


        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;


        raceService.ToGarageTrigger();
    }

    public void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bike") && !isFinished)
        {
            isFinished = true;
            StartCoroutine(FinishSequence());
        }
    }
}
