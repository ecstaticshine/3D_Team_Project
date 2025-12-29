using UnityEngine;

public class SystemSetup : MonoBehaviour
{
    private static SystemSetup instance;

    private void Awake()
    {
        // 부모가 중복인지 검사!
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}