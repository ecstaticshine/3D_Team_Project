using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IState
{
    private const int MAX_SAMPLE_TRY = 10;
    private const float STUCK_VELOCITY_THRESHOLD = 0.01f;
    private const float ARRIVE_DISTANCE_BUFFER = 1.0f; // 도착 인정 여유 거리

    private float waitTimer;      // 현재 대기한 시간
    private float waitDuration;   // 총 대기해야 할 시간
    private float stuckTimer = 0f; // 끼임 감지용 타이머

    public void Enter(AIController controller)
    {
        controller.hasDetectedPlayer = false;

        if (controller.enemyAlert != null)
            controller.enemyAlert.SetState(EnemyAlert.AlertState.None);

        waitTimer = 0f;
        stuckTimer = 0f;

        if (controller.PatrolPoints != null && controller.PatrolPoints.Length > 0)
        {
            float closestDistance = Mathf.Infinity; 
            int closestIndex = 0; // 가장 가까운 지점의 번호를 저장

            for (int i = 0; i < controller.PatrolPoints.Length; i++)
            {
                // 현재 내 위치와 i번째 순찰 지점 사이의 거리를 계산 (Vector3.Distance)
                float dist = Vector3.Distance(controller.transform.position, controller.PatrolPoints[i].position);

                if (dist < closestDistance) 
                {
                    closestDistance = dist; // 최단 거리 갱신
                    closestIndex = i;      // 해당 번호 기억
                }
            }
            // 찾은 가장 가까운 지점부터 순찰을 시작하도록 인덱스 설정
            controller.currentPatrolIndex = closestIndex;
        }

        waitDuration = Random.Range(controller.waitTimeRange.x, controller.waitTimeRange.y);
        controller.Agent.isStopped = false;
        controller.SetMoveSpeed(1.0f);
        controller.animator.SetBool("IsMove", true);

        GotoNextPoint(controller); // 결정된 지점으로 이동 시작
    }

    public void Execute(AIController controller)
    {
        if (controller.CanSeePlayer())
        {
            controller.ChangeState(controller.combatState);
            return;
        }

        // 경로가 끊겼을 때의 예외 처리
        if (!controller.Agent.pathPending &&
            (!controller.Agent.hasPath || controller.Agent.pathStatus != NavMeshPathStatus.PathComplete))
        {
            GotoNextPoint(controller);
            return;
        }

        // 끼임 체크 로직
        if (controller.Agent.remainingDistance > controller.Agent.stoppingDistance &&
            controller.Agent.velocity.sqrMagnitude < STUCK_VELOCITY_THRESHOLD)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 0.5f)
            {
                GotoNextPoint(controller);
                stuckTimer = 0f;
                return;
            }
        }
        else { stuckTimer = 0f; }

        Vector3 agentPos = controller.transform.position; // AI의 현재 위치
        Vector3 destPos = controller.Agent.destination;  // AI의 현재 목적지

        agentPos.y = 0; // 높이 값을 0으로 고정하여 평면 거리만 계산
        destPos.y = 0;  // 목적지의 높이 값도 0으로 고정

        float flatDistance = Vector3.Distance(agentPos, destPos); // 높이를 제외한 거리 측정

        if (!controller.Agent.pathPending &&
            flatDistance <= controller.Agent.stoppingDistance + ARRIVE_DISTANCE_BUFFER)
        {
            waitTimer += Time.deltaTime * controller.timeScaleMultiplier;
            if (waitTimer >= waitDuration)
            {
                waitTimer = 0f;
                waitDuration = Random.Range(controller.waitTimeRange.x, controller.waitTimeRange.y);
                GotoNextPoint(controller);
            }
        }
    }

    public void Exit(AIController controller)
    {
        controller.Agent.isStopped = true;
    }

    private void GotoNextPoint(AIController controller)
    {
        if (controller.PatrolPoints != null && controller.PatrolPoints.Length > 0)
        {
            // 현재 이동해야 할 지점 정보를 가져옵니다.
            Transform targetPoint = controller.PatrolPoints[controller.currentPatrolIndex];

            // [보정] 목적지가 바닥에서 떠 있을 경우 대비 (NavMesh 샘플링)
            
            if (NavMesh.SamplePosition(targetPoint.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                if (TrySetDestination(controller, hit.position)) // 바닥 보정된 위치로 경로 시도
                {
                    // 성공했다면 다음 순서를 위해 인덱스 미리 증가
                    controller.currentPatrolIndex = (controller.currentPatrolIndex + 1) % controller.PatrolPoints.Length;
                    return;
                }
            }
        }

        // 고정 지점 실패 시 랜덤 순찰 시도 (생략된 기존 로직 실행)
    }

    private bool TrySetDestination(AIController controller, Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        // CalculatePath: 실제로 갈 수 있는지 미리 계산해 보는 명령어
        if (controller.Agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            controller.Agent.isStopped = false;
            controller.Agent.SetDestination(target); // 최종 목적지 승인
            return true;
        }
        return false;
    }
}