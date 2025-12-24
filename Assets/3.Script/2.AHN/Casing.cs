using UnityEngine;

public class Casing : MonoBehaviour
{
    private CasingType _myType;
    // [변수] 이미 풀에 반납되었는지 확인하는 스위치입니다.
    private bool _isReturned = false;

    public void Initialize(CasingType type)
    {
        _myType = type;
    }

    private void OnEnable()
    {
        // [기능] 풀에서 꺼내질 때 반납 상태를 초기화합니다.
        _isReturned = false;
        // [명령어] 1초 뒤에 자동으로 반납 함수를 호출합니다.
        Invoke(nameof(BackToPool), 1f);
    }

    private void BackToPool()
    {
        // [검토] 이미 반납되었다면 아래 로직을 실행하지 않습니다 (에러 방지 핵심).
        if (_isReturned) return;

        if (CasingManager.Instance != null)
        {
            _isReturned = true; // [명령어] 반납 상태로 변경
            CasingManager.Instance.ReturnCasing(_myType, gameObject);
        }
    }

    private void OnDisable()
    {
        // [기능] 오브젝트가 꺼질 때 예약된 Invoke를 취소하여 중복 실행을 원천 차단합니다.
        CancelInvoke();
    }
}