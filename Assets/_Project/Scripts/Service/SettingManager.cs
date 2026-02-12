using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetSavings()
    {
        PlayerPrefs.DeleteAll();
        /*new OfflineSaveSystem().DeleteSave();
        new OfflineSaveSystem("bikesdata.json").DeleteSave();
        */SceneManager.LoadScene(0);
    }
}
