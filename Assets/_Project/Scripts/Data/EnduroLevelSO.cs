using UnityEngine;

[CreateAssetMenu(fileName = "NewEnduroLevel", menuName = "Enduro/Level")]
public class EnduroLevelSO : ScriptableObject
{
    public string levelName;
    public string sceneName;
    public int totalCheckpoints = 10;
    public int[] checkpointRewards;
    public int completionBonus = 100;
    public int minCrashDurabilityLoss = 5;
    public int maxCrashDurabilityLoss = 15;
    public float startingTemperature = 20f;
    public float maxTemperature = 120f;
    public float ambientCoolingRate = 5f;
    public float checkpointTemperatureReset = 20f;
}
