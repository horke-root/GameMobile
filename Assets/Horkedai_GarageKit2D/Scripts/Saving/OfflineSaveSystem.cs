using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class OfflineSaveSystem : ISaveRepository
{
    private readonly string filePath;
    public OfflineSaveSystem(string fileName = "playerdata.json")
    {
        this.filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
    }


    public void Save<T>(T data)
    {
        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log(json);
    }

    public T Load<T>() where T : new()
    {
        if (!File.Exists(filePath))
                return new T();
        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<T>(json);
    }

    public void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }





}
