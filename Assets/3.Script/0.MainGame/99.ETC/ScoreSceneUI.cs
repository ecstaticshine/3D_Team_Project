using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreSceneUI : MonoBehaviour
{
    [Header("Clock Settings")]
    [SerializeField] private Transform clockHand; // [유니] 시계 바늘 (qksmf)
    [SerializeField] private float startAngle = 45f; // [유니] 시작 각도 (45도)
    [SerializeField] private float stepAngle = 15f;  // [유니] 한 칸당 이동 각도
    [SerializeField] private float finalAngle = 120f; // [유니] 최종 Total Score 각도
    [SerializeField] private float rotateSpeed = 2f; // [유니] 바늘 이동 속도 (낮을수록 천천히)

    [Header("Score Items")]
    // [유니] 45도부터 시작해서 순서대로 뜰 텍스트들 (Kills, Time, Ability, Accuracy...)
    [SerializeField] private List<GameObject> scoreTexts;

    [Header("Final Result")]
    [SerializeField] private GameObject totalScoreObject; // [유니] 120도에서 뜰 Total Score
    [SerializeField] private Button continueButton;       // [유니] 마지막 버튼

    public string nextSceneName;
    private WaitForSeconds _waitTick = new WaitForSeconds(0.3f); // [유니] 글씨 뜨고 잠깐 대기하는 시간

    private void Start()
    {
        InitializeUI();
        StartCoroutine(SequenceScoreAnimation());
    }

    private void InitializeUI()
    {
        // [유니] 시작할 때 바늘은 45도보다 조금 전(예: 30도)에 두거나, 45도에 둡니다.
        // 오빠 의도상 45도부터 시작이면 45도로 세팅!
        clockHand.localRotation = Quaternion.Euler(0, startAngle - stepAngle, 0);

        foreach (var textObj in scoreTexts)
        {
            if (textObj != null) textObj.SetActive(false);
        }

        if (totalScoreObject != null) totalScoreObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
    }

    private IEnumerator SequenceScoreAnimation()
    {
        // [유니] 연출 시작 전 아주 잠깐 대기 (로딩 뚝뚝 끊김 방지)
        yield return new WaitForSeconds(0.5f);

        // ------------------------------------------------
        // 1단계: 리스트에 있는 점수들 (45도 ~ 105도 구간)
        // ------------------------------------------------
        for (int i = 0; i < scoreTexts.Count; i++)
        {
            // 목표 각도 계산: 45 + (0*15) = 45, 60, 75...
            float targetY = startAngle + (i * stepAngle);

            // [중요] 바늘이 목표까지 갈 때까지 여기서 코드 멈춤! (Wait)
            yield return StartCoroutine(MoveNeedleToAngle(targetY));

            // 바늘이 도착했으니 텍스트 켜기!
            if (scoreTexts[i] != null)
            {
                scoreTexts[i].SetActive(true);
                // 여기에 사운드 넣으면 좋아요! AudioSource.PlayOneShot(tickSound);
            }

            // 텍스트 보여주고 잠깐 감상 시간
            yield return _waitTick;
        }

        // ------------------------------------------------
        // 2단계: 대망의 Total Score (120도 구간)
        // ------------------------------------------------
        // 리스트 다 돌았으니 이제 120도로 이동!
        yield return StartCoroutine(MoveNeedleToAngle(finalAngle));

        // 도착 후 Total Score 짠!
        if (totalScoreObject != null) totalScoreObject.SetActive(true);

        // 버튼 활성화
        if (continueButton != null) continueButton.gameObject.SetActive(true);
    }

    // [유니] 바늘을 특정 각도로 부드럽게 돌리는 함수 (재사용 가능!)
    private IEnumerator MoveNeedleToAngle(float targetY)
    {
        Quaternion targetRotation = Quaternion.Euler(0, targetY, 0);

        // 현재 각도와 목표 각도 차이가 클 때만 회전 (0.1도 이내면 멈춤)
        while (Quaternion.Angle(clockHand.localRotation, targetRotation) > 0.1f)
        {
            // Slerp로 부드럽게
            clockHand.localRotation = Quaternion.Slerp(clockHand.localRotation, targetRotation, Time.deltaTime * rotateSpeed);
            yield return null; // 한 프레임 대기
        }

        // 루프 끝나면 각도 정확하게 딱! 고정 (오차 제거)
        clockHand.localRotation = targetRotation;
    }

    public void OnClickContinue()
    {
        if (ScoreManager.instance != null)
        {
            // 1. 매니저가 기억하고 있는 다음 씬 이름을 가져와
            string nextScene = ScoreManager.instance.nextSceneName;

            // 2. 오빠가 원한대로 매니저 파괴 (점수 초기화를 위해)
            Destroy(ScoreManager.instance.gameObject);

            // 3. 다음 씬으로 출발!
            // 만약 이름이 비어있다면 메인화면으로 보내는 예외처리도 센스!
            if (string.IsNullOrEmpty(nextScene))
            {
                SceneManager.LoadScene("Title");
            }
            else
            {
                SceneManager.LoadScene(nextScene);
            }
        }
        else
        {
            // 혹시라도 매니저가 없으면 메인으로
            SceneManager.LoadScene("Title");
        }
    }
}