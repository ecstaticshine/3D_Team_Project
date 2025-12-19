//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// ai가 어 무슨 소리였지?하고 주위를 경계하는 걸 담당하는 코드
public class J_AlertState : J_IState
{
    private float timer;
    private float rotationTimer;

    private const float ROTATION_CHANGE_TIME = 2.0f;
    private const float ARRIVAL_MARGIN = 0.5f;

    private Vector3 targetSearchDirection;

    private float speed;

    public void Enter(J_AIController ai) // AI가 처음 경계 상태에 들어오자마자 하는 행동(방금 저기서 소리가 났는데?)
    {
        timer = 0f;
        rotationTimer = 0f;

        ai.StopMove(); // 가고있던 길을 멈추고 새로운 명령을 받기 전 정지 상태로 만듬

        // 청각 정보 위치를 NavMesh 위로 보정
        if (NavMesh.SamplePosition(ai.lastHeardPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            ai.MoveTo(hit.position);
            Debug.Log($"[Alert] {ai.name}: 청각 위치로 이동 시작 {hit.position}");
        }
        else
        {
            // NavMesh 밖이면 제자리 수색
            Debug.LogWarning($"[Alert] {ai.name}: 청각 위치 NavMesh 실패 → 제자리 수색");
            targetSearchDirection = ai.transform.position + ai.transform.forward;
        }
    }

    public void Execute(J_AIController ai) // AI가 소리가 난 곳에 도착한 이후의 행동(도착했으니 주변을 살펴보자)
    {

        timer += Time.deltaTime * ai.timeScaleMultiplier;


        if (ai.CanSeePlayer())
        {
            ai.ChangeState(ai.combatState);
            return;
        }

        Vector3 soundDir = (ai.lastHeardPosition - ai.transform.position).normalized;

        Quaternion noise = Quaternion.Euler(
            0,
            Random.Range(-25f, 25f),
            0
        );

        Vector3 fakeTarget = ai.transform.position + (noise * soundDir) * 10f;

        ai.LookAt(fakeTarget);

        if (!ai.Agent.pathPending &&
            ai.Agent.remainingDistance <= ai.Agent.stoppingDistance + ARRIVAL_MARGIN)
        {
            rotationTimer += Time.deltaTime * ai.timeScaleMultiplier;


            if (rotationTimer >= ROTATION_CHANGE_TIME)
            {
                Vector2 rand = Random.insideUnitCircle;
                Vector3 randomDir = new Vector3(rand.x, 0f, rand.y);

                targetSearchDirection = ai.transform.position + randomDir.normalized;
                rotationTimer = 0f;
            }

            // 수색 방향으로 회전
            ai.LookAt(targetSearchDirection);
        }


        if (timer >= ai.alertDuration)
        {
            ai.ChangeState(ai.patrolState);
        }
    }

    public void Exit(J_AIController ai) // AI가 일정시간 동안 수색해도 아무것도 없을 때 하는 행동(아무것도 없네 다시 순찰이나 하자)
    {
        ai.StopMove();
    }

}
