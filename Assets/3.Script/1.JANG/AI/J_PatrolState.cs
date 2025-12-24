using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class J_PatrolState : J_IState
{
    private const int MAX_SAMPLE_TRY = 10;
    private const float STUCK_VELOCITY_THRESHOLD = 0.01f;
    private const float ARRIVE_DISTANCE_BUFFER = 1.0f;

    private float waitTimer;
    private float waitDuration;

    // [유니] 출발 유예 시간을 위한 타이머 추가
    private float stuckTimer = 0f;

    private float speed;

    public void Enter(J_AIController controller)
    {
        controller.hasDetectedPlayer = false;

        if (controller.enemyAlert != null)
        {
            controller.enemyAlert.SetState(EnemyAlert.AlertState.None);
        }

        waitTimer = 0f;
        stuckTimer = 0f; // [유니] 타이머 초기화
        waitDuration = Random.Range(
            controller.waitTimeRange.x,
            controller.waitTimeRange.y
        );

        controller.Agent.isStopped = false;
        controller.SetMoveSpeed(1.0f);
        controller.animator.SetBool("IsMove", true);

        GotoNextPoint(controller);
    }

    public void Execute(J_AIController controller)
    {
        // 1. 플레이어 발견
        if (controller.CanSeePlayer())
        {
            controller.ChangeState(controller.combatState);
            return;
        }

        // 2. 경로 유효성 체크
        // pathPending: 경로 계산 중일 때는 건드리면 안 됨!
        if (!controller.Agent.pathPending &&
            (!controller.Agent.hasPath || controller.Agent.pathStatus != NavMeshPathStatus.PathComplete))
        {
            GotoNextPoint(controller);
            return;
        }

        // 3. [수정됨] 벽/코너 끼임 체크 (타이머 적용)
        // 움직여야 하는데 속도가 0인 경우
        if (controller.Agent.remainingDistance > controller.Agent.stoppingDistance &&
            controller.Agent.velocity.sqrMagnitude < STUCK_VELOCITY_THRESHOLD)
        {
            // 바로 바꾸지 말고 시간을 좀 잰다
            stuckTimer += Time.deltaTime;

            // 0.5초 동안이나 속도가 0이면 진짜 낀 거다!
            if (stuckTimer > 0.5f)
            {
                Debug.LogWarning($"[Patrol] {controller.name} 끼임 감지! 다음 경로로 강제 이동");
                GotoNextPoint(controller);
                stuckTimer = 0f; // 리셋
                return;
            }
        }
        else
        {
            // 잘 움직이고 있으면 타이머 초기화
            stuckTimer = 0f;
        }

        // 4. 목적지 도착 처리
        if (!controller.Agent.pathPending &&
            controller.Agent.remainingDistance <= controller.Agent.stoppingDistance + ARRIVE_DISTANCE_BUFFER)
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

        // [유니] 여기 밑에 있던 중복된 if (waitTimer >= waitDuration) 코드는 지웠어!
        // 위에서 이미 처리하고 있어서 두 번 실행될 위험이 있거든.
    }

    public void Exit(J_AIController controller)
    {
        controller.Agent.isStopped = true;
        controller.animator.SetBool("isMove", false);
    }

    // (GotoNextPoint랑 TrySetDestination은 그대로 두면 돼!)
    private void GotoNextPoint(J_AIController controller)
    {
        // ... (오빠 코드 그대로) ...
        // 코드 길어지니까 생략할게, 기존 거 그대로 써!

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
                    return;
                }
            }
        }

        controller.Agent.ResetPath();
    }

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