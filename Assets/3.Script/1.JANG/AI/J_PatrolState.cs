//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class J_PatrolState : J_IState
{
    private const int MAX_SAMPLE_TRY = 10;
    private const float STUCK_VELOCITY_THRESHOLD = 0.01f;
    private const float ARRIVE_DISTANCE_BUFFER = 0.3f;

    private float waitTimer;
    private float waitDuration;

    private float speed;

    public void Enter(J_AIController controller)
    {
        waitTimer = 0f;
        waitDuration = Random.Range(
            controller.waitTimeRange.x,
            controller.waitTimeRange.y
        );

        controller.Agent.isStopped = false;

        controller.SetMoveSpeed(1.0f);
        controller.animator.SetBool("isMove", true);
        GotoNextPoint(controller);

        Debug.Log($"[Patrol] {controller.name}: Patrol Start");
    }

    public void Execute(J_AIController controller)
    {


        // 1️ 플레이어 발견 → 즉시 전투
        if (controller.CanSeePlayer())
        {
            controller.ChangeState(controller.combatState);
            return;
        }

        // 2️ 경로가 없거나 막힘 → 새 순찰 지점
        if (!controller.Agent.hasPath ||
            controller.Agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            GotoNextPoint(controller);
            return;
        }

        // 3️ 벽/코너에 끼여 멈춘 경우
        if (controller.Agent.velocity.sqrMagnitude < STUCK_VELOCITY_THRESHOLD)
        {
            GotoNextPoint(controller);
            return;
        }

        // 4️ 목적지 도착 처리
        if (!controller.Agent.pathPending &&
            controller.Agent.remainingDistance <=
            controller.Agent.stoppingDistance + ARRIVE_DISTANCE_BUFFER)
        {
            waitTimer += Time.deltaTime * controller.timeScaleMultiplier;

            if (waitTimer >= waitDuration)
            {
                waitTimer = 0f;
                waitDuration = Random.Range(
                    controller.waitTimeRange.x,
                    controller.waitTimeRange.y
                );

                GotoNextPoint(controller);
            }
        }
    }

    public void Exit(J_AIController controller)
    {
        controller.Agent.isStopped = true;
    }

    // ======================================
    // 다음 순찰 지점 설정 (고정 / 랜덤)
    // ======================================
    private void GotoNextPoint(J_AIController controller)
    {

        // -----------------------------
        // 1️ 고정 순찰
        // -----------------------------
        if (controller.PatrolPoints != null &&
            controller.PatrolPoints.Length > 0)
        {
            for (int i = 0; i < controller.PatrolPoints.Length; i++)
            {
                Transform point =
                    controller.PatrolPoints[controller.currentPatrolIndex];

                controller.currentPatrolIndex =
                    (controller.currentPatrolIndex + 1) %
                    controller.PatrolPoints.Length;

                if (TrySetDestination(controller, point.position))
                {
                    Debug.Log($"[Patrol] {controller.name}: Fixed Point -> {point.name}");
                    return;
                }
            }
        }

        // -----------------------------
        // 2️ 랜덤 순찰
        // -----------------------------
        Vector3 origin = controller.transform.position;

        for (int i = 0; i < MAX_SAMPLE_TRY; i++)
        {
            Vector2 rand = Random.insideUnitCircle * controller.patrolRadius;
            Vector3 candidate =
                origin + new Vector3(rand.x, 0f, rand.y);

            if (NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    controller.patrolRadius,
                    NavMesh.AllAreas))
            {
                if (TrySetDestination(controller, hit.position))
                {
                    Debug.Log($"[Patrol] {controller.name}: Random Point -> {hit.position}");
                    return;
                }
            }
        }

        // 모든 시도 실패 → 정지
        controller.Agent.ResetPath();
    }

    // ======================================
    // 목적지 유효성 검사 + SetDestination
    // ======================================
    private bool TrySetDestination(J_AIController controller, Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();

        if (controller.Agent.CalculatePath(target, path) &&
            path.status == NavMeshPathStatus.PathComplete)
        {
            controller.Agent.isStopped = false;
            controller.Agent.SetDestination(target);
            return true;
        }

        return false;
    }

}
