//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AlertState : IState
{
    private float timer;
    private float rotationTimer;

    private const float ROTATION_CHANGE_TIME = 2.0f;
    private const float ARRIVAL_MARGIN = 0.5f;

    private Vector3 targetSearchDirection;

    public void Enter(AIController ai)
    {
        timer = 0f;
        rotationTimer = 0f;

        ai.StopMove(); // 잔여 이동 정리

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

    public void Execute(AIController ai)
    {

        timer += Time.deltaTime * ai.timeScaleMultiplier;


        if (ai.CanSeePlayer())
        {
            ai.ChangeState(ai.combatState);
            return;
        }


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

    public void Exit(AIController ai)
    {
        ai.StopMove();
    }

}
