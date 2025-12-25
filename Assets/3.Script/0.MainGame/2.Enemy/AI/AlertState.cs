//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// ai가 어 무슨 소리였지?하고 주위를 경계하는 걸 담당하는 코드
public class AlertState : IState
{
    private float timer;
    private float rotationTimer;

    private const float ROTATION_CHANGE_TIME = 2.0f;
    private const float ARRIVAL_MARGIN = 0.5f;

    private Vector3 targetSearchDirection;

    private float speed;

    public void Enter(AIController ai) // AI가 처음 경계 상태에 들어오자마자 하는 행동(방금 저기서 소리가 났는데?)
    {
        // [핵심 로직 수정]
        // 방금 전까지 전투 중이 아니었을 때만(평화로울 때만) '?'를 띄운다.
        // 전투 중이었다면(hasDetectedPlayer == true), 아이콘을 띄우지 않거나 기존 '!'를 유지한다.
        if (ai.enemyAlert != null && !ai.hasDetectedPlayer)
        {
            ai.enemyAlert.SetState(EnemyAlert.AlertState.Suspicious);
        }

        timer = 0f;
        rotationTimer = 0f;

        ai.StopMove(); // 가고있던 길을 멈추고 새로운 명령을 받기 전 정지 상태로 만듬

        // 청각 정보 위치를 NavMesh 위로 보정
        if (NavMesh.SamplePosition(ai.lastHeardPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            ai.MoveTo(hit.position);
            //Debug.Log($"[Alert] {ai.name}: 청각 위치로 이동 시작 {hit.position}");
        }
        else
        {
            // NavMesh 밖이면 제자리 수색
            //Debug.LogWarning($"[Alert] {ai.name}: 청각 위치 NavMesh 실패 → 제자리 수색");
            targetSearchDirection = ai.transform.position + ai.transform.forward;
        }
    }

    public void Execute(AIController ai) // AI가 소리가 난 곳에 도착한 이후의 행동(도착했으니 주변을 살펴보자)
    {

        timer += Time.deltaTime * ai.timeScaleMultiplier;

        if (ai.CanSeePlayer())
        {
            ai.ChangeState(ai.combatState);
            return;
        }

        // lastHeardPosition: 마지막으로 소리가 들린 위치입니다.
        // soundDir: 내 위치에서 소리 지점까지의 방향을 정규화(길이를 1로 만듬)한 벡터입니다.
        Vector3 soundDir = (ai.lastHeardPosition - ai.transform.position).normalized;

        // noise: 소리 난 방향에서 좌우로 최대 25도 정도 오차를 주기 위한 무작위 회전값입니다.
        Quaternion noise = Quaternion.Euler(0, Random.Range(-25f, 25f), 0);

        // fakeTarget: 소리 난 곳 주변을 의심하며 바라볼 가상의 지점입니다. 
        // y좌표 보정: ai.transform.position.y를 더해줌으로써 바닥이 아닌 자신의 눈높이 정도를 보게 합니다.
        Vector3 fakeTarget = ai.transform.position + (noise * soundDir) * 10f;
        fakeTarget.y = ai.transform.position.y; // [추가] 수평을 바라보도록 고정

        ai.LookAt(fakeTarget);

        if (!ai.Agent.pathPending && ai.Agent.remainingDistance <= ai.Agent.stoppingDistance + ARRIVAL_MARGIN)
        {
            rotationTimer += Time.deltaTime * ai.timeScaleMultiplier;

            if (rotationTimer >= ROTATION_CHANGE_TIME)
            {
                Vector2 rand = Random.insideUnitCircle;
                Vector3 randomDir = new Vector3(rand.x, 0f, rand.y);

                // targetSearchDirection: 주변을 두리번거릴 때 바라볼 랜덤한 방향의 좌표입니다.
                // ai.transform.position.y를 사용하여 수평을 유지합니다.
                targetSearchDirection = ai.transform.position + randomDir.normalized;
                targetSearchDirection.y = ai.transform.position.y;
                rotationTimer = 0f;
            }

            ai.LookAt(targetSearchDirection);
        }


        if (timer >= ai.alertDuration)
        {
            ai.ChangeState(ai.patrolState);
        }
    }

    public void Exit(AIController ai) // AI가 일정시간 동안 수색해도 아무것도 없을 때 하는 행동(아무것도 없네 다시 순찰이나 하자)
    {
        ai.StopMove();
    }

}
