using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CombatState : IState
{
    private float fireCooldown; // 사격 간격을 관리하기 위한 변수입니다.
    private float lostSightTimer; // 플레이어를 놓친 후의 경과 시간을 저장하는 변수입니다.

    private const float MIN_FIRE_INTERVAL = 0.4f; // 최소 사격 대기 시간입니다.
    private const float MAX_FIRE_INTERVAL = 0.6f; // 최대 사격 대기 시간입니다.
    private const float LOST_SIGHT_GRACE_TIME = 5.0f; // 시야에서 사라진 후 상태를 유지할 유예 시간입니다.

    public void Enter(AIController ai)
    {
        ai.hasDetectedPlayer = true; // AI가 플레이어를 발견했음을 기억하는 변수를 참으로 설정합니다.

        if (ai.enemyAlert != null)
        {
            // enemyAlert: 경고 UI 등을 관리하는 객체입니다.
            // SetState: UI 상태를 '발견됨(Detected)' 상태인 '!'로 변경합니다.
            ai.enemyAlert.SetState(EnemyAlert.AlertState.Detected);
        }

        // Random.Range: 최소값과 최대값 사이의 무작위 숫자를 반환하여 사격 쿨타임을 초기화합니다.
        fireCooldown = Random.Range(MIN_FIRE_INTERVAL, MAX_FIRE_INTERVAL);
        lostSightTimer = 0f; // 유예 시간 타이머를 0으로 초기화합니다.
        ai.Agent.isStopped = false; // NavMeshAgent가 이동할 수 있도록 정지 상태를 해제합니다.

        ai.SetMoveSpeed(3.0f); // AI의 이동 속도를 3.0으로 설정합니다.
        //ai.animator.SetBool("isRun", true); // 애니메이터의 'isRun' 파라미터를 true로 바꿔 달리기 애니메이션을 재생합니다.
    }

    public void Execute(AIController ai)
    {
        // 1. 플레이어 가슴 위치 참조 (가슴 위치가 없을 경우를 대비해 예외 처리)
        // targetPos: 플레이어의 발바닥 대신 가슴(Chest)의 월드 좌표를 가져와 저장합니다.
        Vector3 targetPos = (ai.EnemyChasePosition != null) ? ai.EnemyChasePosition.position : ai.player.transform.position;

        // distance: AI 자기 자신의 위치와 targetPos(가슴) 사이의 직선 거리를 계산합니다.
        float distance = Vector3.Distance(ai.transform.position, targetPos);

        // 2. 시야 체크 (유예 시간 포함)
        if (!ai.CanSeePlayer())
        {
            ai.LookAround(); // 플레이어가 안 보이면 주변을 둘러보는 함수를 실행합니다.
            // Time.deltaTime: 프레임 간의 경과 시간으로, 성능에 상관없이 일정한 속도로 타이머를 올립니다.
            lostSightTimer += Time.deltaTime * ai.timeScaleMultiplier;

            if (lostSightTimer >= LOST_SIGHT_GRACE_TIME)
            {
                // ChangeState: 일정 시간이 지나면 경계(Alert) 상태로 상태를 전이합니다.
                ai.ChangeState(ai.alertState);
            }
            return;
        }
        else
        {
            lostSightTimer = 0f; // 플레이어가 보이면 타이머를 즉시 초기화합니다.
        }

        // 3. 조준 (가슴 위치를 바라보게 함)
        // LookAt: AI의 몸 방향을 플레이어의 가슴 좌표로 회전시킵니다.
        ai.LookAt(targetPos);

        // 4. 타입별 전투 동작
        if (ai.combatType == AIController.CombatType.Melee)
        {
            HandleMeleeCombat(ai, targetPos, distance);
        }
        else
        {
            HandleRangedCombat(ai, targetPos, distance);    
        }
    }

    private void HandleMeleeCombat(AIController ai, Vector3 targetPos, float distance)
    {
        // meleeEngageDistance: 근접 공격을 시작할 거리 기준값입니다.
        if (distance > ai.meleeEngageDistance)
        {
            Move(ai, targetPos); // 거리가 멀면 가슴 위치(targetPos)를 향해 이동합니다.
        }
        else
        {
            ai.StopMove(); // 공격 범위 안이면 NavMeshAgent를 정지시킵니다.
            ai.LookAt(targetPos); // 정지 상태에서도 계속 가슴을 응시합니다.
        }

        // isDie: 플레이어 스크립트의 사망 여부 변수를 체크합니다.
        if (!ai.player.GetComponent<Player>().isDie)
        {
            ai.Attack(); // 플레이어가 살아있다면 공격 애니메이션/로직을 실행합니다.
        }
    }

    private void HandleRangedCombat(AIController ai, Vector3 targetPos, float distance)
    {
        // 0.8: 사격 유지 거리보다 조금 더 가까워지면(80% 거리) 뒤로 물러납니다.
        if (distance < ai.rangedEngageDistance * 0.8)
        {
            // retreatDir: AI가 플레이어 반대 방향으로 가기 위한 벡터 방향을 계산합니다.
            Vector3 retreatDir = (ai.transform.position - targetPos).normalized;
            // retreatPos: 현재 위치에서 반대 방향으로 퇴각 거리만큼 떨어진 지점을 계산합니다.
            Vector3 retreatPos = ai.transform.position + retreatDir * ai.retreatDistance;
            Move(ai, retreatPos);
        }
        else if (distance > ai.rangedEngageDistance)
        {
            Move(ai, targetPos); // 사격 거리보다 멀면 플레이어에게 다가갑니다.
        }
        else
        {
            ai.StopMove(); // 적정 거리면 멈추고 사격 준비를 합니다.
            ai.LookAt(targetPos);
        }

        if (!ai.player.GetComponent<Player>().isDie)
        {
            // TryFire: 원거리 공격 컴포넌트(j_AIShooter)에서 사격을 시도합니다.
            ai.aIShooter.TryFire();
        }
    }

    private void Move(AIController ai, Vector3 targetPos)
    {
        // pathPending: NavMesh가 경로를 계산 중인지 확인합니다.
        // sqrMagnitude: 두 지점 간의 거리 제곱값으로, 연산 속도가 빨라 거리 비교 시 자주 사용됩니다.
        if (!ai.Agent.pathPending && (ai.Agent.destination - targetPos).sqrMagnitude > 1f)
        {
            // MoveTo: NavMeshAgent를 목적지로 이동시키는 J_AIController의 함수입니다.
            ai.MoveTo(targetPos);
        }
    }

    public void Exit(AIController ai)
    {
        ai.StopMove(); // 상태를 나갈 때 이동을 멈춥니다.
    }
}