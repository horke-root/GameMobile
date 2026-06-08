using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;

    void Awake()
    {
        Instance = this;
    }
}
