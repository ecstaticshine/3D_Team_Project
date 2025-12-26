using System;

[Serializable]
public class SaveData
{
    public int clearStage;
    public int deathCount;
    public long totalPlayTimeMs;
    public long lastSaveTime;

    public string sceneToLoad = "TrainingScene 1";

    public SaveData()
    {
        clearStage = 0;
        deathCount = 0;
        totalPlayTimeMs = 0;
        //UTC
        lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        sceneToLoad = "TrainingScene 1";
    }

}
