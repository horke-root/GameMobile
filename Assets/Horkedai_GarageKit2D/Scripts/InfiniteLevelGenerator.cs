using System.Collections.Generic;
using UnityEngine;





public class InfiniteLevelGenerator : MonoBehaviour

{
    [Header("Ссылки")]
    [Tooltip("Префаб твоего фона с гаражами/дорогой и светом")]
    public GameObject midgroundPrefab;

    [Header("Настройки генерации")]
    [Tooltip("Точная ширина одного префаба (в метрах/юнитах).")]
    public float segmentWidth = 30f;
    public float yOffset = 0;

    [Tooltip("Сколько кусков держать на сцене одновременно.")]
    public int poolSize = 4;

    [Tooltip("За сколько метров до края камеры переносить следующий кусок.")]
    public float spawnAheadDistance = 40f;


    private Transform camTransform;

    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private float nextSpawnX = 0f;

    private void Start()
    {

        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("LevelGenerator: На сцене нет камеры с тегом MainCamera!");
            return;
        }

        if (midgroundPrefab == null)
        {
            Debug.LogError("LevelGenerator: Не назначен префаб!");
            return;
        }


        for (int i = 0; i < poolSize; i++)
        {
            SpawnInitialSegment();
        }
    }

    private void Update()
    {

        if (camTransform.position.x + spawnAheadDistance > nextSpawnX - segmentWidth)
        {
            RecycleSegment();
        }
    }

    private void SpawnInitialSegment()
    {
        GameObject segment = Instantiate(midgroundPrefab, new Vector3(nextSpawnX, yOffset, 0f), Quaternion.identity);
        segment.transform.SetParent(transform);
        activeSegments.Enqueue(segment);

        nextSpawnX += segmentWidth;
    }

    private void RecycleSegment()
    {
        GameObject oldestSegment = activeSegments.Dequeue();


        oldestSegment.transform.position = new Vector3(nextSpawnX, yOffset, 0f);

        activeSegments.Enqueue(oldestSegment);

        nextSpawnX += segmentWidth;
    }
}
