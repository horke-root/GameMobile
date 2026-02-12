using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void LoadByName(string sceneName) => 
        SceneManager.LoadScene(sceneName);
    
    public void LoadById(int sceneId) => 
        SceneManager.LoadScene(sceneId); 
}