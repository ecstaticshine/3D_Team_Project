//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PatrolState : IState
{
    private Vector3 patrolTarget;
    private const float MIN_ARRIVAL_DISTANCE = 0.5f;
    private const int MAX_SAMPLE_TRY = 5;

    public void Enter(K_AIController ai)
    {
        Debug.Log($"[Patrol] {ai.name}: 순찰 시작");
        ai.Agent.isStopped = false;
        SetNewPatrolPoint(ai);
    }

    public void Execute(K_AIController ai)
    {
        // 1. 플레이어 인식 우선
        if (ai.CanSeePlayer())
        {
            ai.ChangeState(ai.combatState);
            return;
        }

        // 2. 목적지 도착 판정
        if (!ai.Agent.pathPending &&
            ai.Agent.remainingDistance <= ai.Agent.stoppingDistance + MIN_ARRIVAL_DISTANCE)
        {
            SetNewPatrolPoint(ai);
        }
    }

    public void Exit(K_AIController ai)
    {
        ai.StopMove();
    }

    private void SetNewPatrolPoint(K_AIController ai)
    {
        Vector3 origin = ai.transform.position;

        // 여러 번 시도하여 반드시 NavMesh 위 지점 확보
        for (int i = 0; i < MAX_SAMPLE_TRY; i++)
        {
            // XZ 평면 랜덤 위치
            Vector2 rand = Random.insideUnitCircle * ai.patrolRadius;
            Vector3 checkPos = origin + new Vector3(rand.x, 0f, rand.y);

            if (NavMesh.SamplePosition(
                    checkPos,
                    out NavMeshHit hit,
                    ai.patrolRadius,
                    NavMesh.AllAreas))
            {
                patrolTarget = hit.position;
                ai.MoveTo(patrolTarget);

                Debug.Log($"[Patrol] {ai.name}: 새 순찰 지점 {patrolTarget}");
                return;
            }
        }

        // 최후 수단: 이동 중단 (멈춤 방지)
        Debug.LogWarning($"[Patrol] {ai.name}: 순찰 지점 탐색 실패, 현재 위치 유지");
        ai.StopMove();
    }

}
