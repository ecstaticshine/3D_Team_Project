using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance = null;
    public bool IsLoaded { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public SaveData saveData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        saveData = Load();
        IsLoaded = true;
    }


    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        //Debug.Log($"저장 완료: {SavePath}");
    }

    public SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            //Debug.Log("세이브 파일 없음, 새로 생성합니다.");
            return new SaveData();
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        data.totalPlayTimeMs = GameManager.instance.totalPlayTimeMs;
        data.deathCount = GameManager.instance.deathCount;
        data.lastSaveTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Save(data);
    }
}
