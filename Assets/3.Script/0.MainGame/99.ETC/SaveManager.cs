using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class StageResultData
{
    public int kills;
    public int headshots;
    public float playTime;
    public float abilityTime;
    public float accuracy;
    public int totalScore;
    public SceneName nextScene;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance = null;
    public bool IsLoaded { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public SaveData saveData;
    public StageResultData lastStageResult;

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

    public void NewGame() 
    {
        saveData = new SaveData();

        Save(saveData);

        if (GameManager.instance != null)
        {
            GameManager.instance.totalPlayTimeMs = 0;
            GameManager.instance.deathCount = 0;
            GameManager.instance.ResumeGame();
        }
    }


    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"저장 완료: {SavePath}");
    }

    public SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("세이브 파일 없음, 새로 생성합니다.");
            return new SaveData();
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    public void SaveGame()
    {
        if (saveData == null)
        {
            saveData = new SaveData();
        }

        saveData.totalPlayTimeMs = GameManager.instance.totalPlayTimeMs;
        saveData.deathCount = GameManager.instance.deathCount;
        saveData.lastSaveTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Save(saveData);
    }
}
