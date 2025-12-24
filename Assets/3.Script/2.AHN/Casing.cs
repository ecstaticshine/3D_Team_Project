using UnityEngine;

public class Casing : MonoBehaviour
{
    private CasingType _myType;

    public void Initialize(CasingType type)
    {
        _myType = type;
    }

    private void OnEnable()
    {
        // [명령어] 2초 뒤에 자동으로 풀에 반납되도록 예약
        Invoke(nameof(BackToPool), 2f);
    }

    private void BackToPool()
    {
        CancelInvoke(); 
        // [기능] 매니저에게 나 자신을 반납
        if (CasingManager.Instance != null)
        {
            CasingManager.Instance.ReturnCasing(_myType, gameObject);
        }
    }
}