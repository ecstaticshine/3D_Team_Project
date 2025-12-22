using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class K_AIController : MonoBehaviour
{



    public enum CombatType
    {
        Melee,
        Ranged
    }

    [Header("전투 설정")]
    public CombatType combatType = CombatType.Melee;

    [Tooltip("근접형 적이 플레이어에게 접근을 멈추는 거리")]
    public float meleeEngageDistance = 3f;
    [Tooltip("원거리형 적이 유지하려는 전투 거리")]
    public float rangedEngageDistance = 12f;
    [Tooltip("원거리형 적이 후퇴할 때 이동하는 거리")]
    public float retreatDistance = 4f;

    // ===============================
    // 참조
    // ===============================
    [Header("참조")]
    public Transform player;
    public Transform eyePoint;

    public NavMeshAgent Agent { get; private set; }

    
    [Header("시간 제어")]
    [Range(0.1f, 3f)]
    [Tooltip("불릿타임 시 AI 개별 시간 배율")]
    public float timeScaleMultiplier = 1f;

    private float baseAgentSpeed;
    private float baseAngularSpeed;
    private const float BASE_LOOK_SPEED = 5f;

    
    [Header("시야설정")]
    [Tooltip("플레이어를 인식할 수 있는 최대 거리")]
    public float viewDistance = 20f;
    [Range(0f, 180f)]
    [Tooltip("플레이어를 인식할 수 있는 시야각")]
    public float viewAngle = 90f;
    [Tooltip("Raycast 시 플레이어를 가리는 장애물 Layer Mask")]
    public LayerMask obstacleMask; 

   
    private IState currentState;

    // 상태 인스턴스 (GC 방지)
    public readonly PatrolState patrolState = new PatrolState();
    public readonly AlertState alertState = new AlertState();
    public readonly CombatState combatState = new CombatState();

    [Header("순찰 설정")]
    public float patrolRadius = 15f;

    public Vector2 waitTimeRange = new Vector2(1f, 3f);

    public Transform[] PatrolPoints;
    [HideInInspector] public int currentPatrolIndex;

    [Header("전투 설정")]
    [Tooltip("플레이어와 전투 상태를 유지할 최소 거리")]
    public float combatEngageDistance = 15f;
    [Tooltip("전투 중 엄폐물 간 이동 시 스나이핑 사거리")]
    public float combatSnipeDistance = 25f;
    [Tooltip("전투 중 무기를 발사할 최소/최대 간격 (초)")]
    public Vector2 fireInterval = new Vector2(0.5f, 1.5f);

   
    [Header("경계 및 수색")]
    public Vector3 lastHeardPosition;
    [Tooltip("경계 상태를 유지하는 시간")]
    public float alertDuration = 5f; 

   
    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();

        baseAgentSpeed = Agent.speed;
        baseAngularSpeed = Agent.angularSpeed;

        ApplyTimeScale();
    }

    void Start()
    {
        ChangeState(patrolState);
    }

    void Update()
    {
        currentState?.Execute(this);
    }

    // ===============================
    // FSM Control
    // ===============================
    public void ChangeState(IState newState)
    {
        if (currentState == newState || newState == null)
            return;

        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);

        Debug.Log($"[AI] {name} -> {newState.GetType().Name}");
    }

    // ===============================
    // Time Scale Control
    // ===============================
    public void SetTimeScale(float multiplier)
    {
        timeScaleMultiplier = Mathf.Clamp(multiplier, 0.1f, 3f);
        ApplyTimeScale();
    }

    private void ApplyTimeScale()
    {
        Agent.speed = baseAgentSpeed * timeScaleMultiplier;
        Agent.angularSpeed = baseAngularSpeed * timeScaleMultiplier;
    }

    // ===============================
    // 시야 판정
    // ===============================
    public bool CanSeePlayer()
    {
        if (player == null || eyePoint == null)
            return false;

        Vector3 toPlayer = player.position - eyePoint.position;
        float distance = toPlayer.magnitude;

        // 1. 거리 체크
        if (distance > viewDistance)
            return false;

        Vector3 dir = toPlayer.normalized;

        // 2. 시야각 체크
        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f)
            return false;

        // 3. Raycast (장애물 체크)
        // ~obstacleMask를 사용하여 장애물 레이어를 제외한 모든 것과 충돌 체크.
        if (Physics.Raycast(eyePoint.position, dir, out RaycastHit hit, distance))
        {
            return hit.collider.CompareTag("Player");
        }

        // Raycast가 아무것도 맞추지 못했거나, 무시된 레이어(obstacleMask)만 맞춘 경우
        return false;
    }

    // ===============================
    // 청각 이벤트
    // ===============================

    public void OnSoundHeard(Vector3 soundPosition)
    {
        lastHeardPosition = soundPosition;

        // 전투 중이 아닐 때만 Alert 진입
        if (currentState is not CombatState)
        {
            LookAt(soundPosition);
            ChangeState(alertState);
        }
    }

    // ===============================
    // 행동
    // ===============================
    public void LookAt(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * BASE_LOOK_SPEED * timeScaleMultiplier
        );
    }

    public void MoveTo(Vector3 destination)
    {
        if (!Agent.enabled) return;

        Agent.isStopped = false;
        Agent.SetDestination(destination);
    }

    public void StopMove()
    {
        if (!Agent.enabled) return;
        Agent.isStopped = true;
    }

    public void Shoot()
    {
        // 무기 시스템 연결 예정
        Debug.DrawRay(eyePoint.position, eyePoint.forward * 10f, Color.red);
    }

    // ===============================
    // Debug
    // ===============================
    void OnDrawGizmosSelected()
    {
        if (eyePoint == null) return;

        DrawVisionCone(eyePoint.position, transform.forward, viewAngle, viewDistance);
    }

    private void DrawVisionCone(Vector3 origin, Vector3 forward, float angle, float range)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, range);

        Quaternion leftRot = Quaternion.Euler(0, -angle * 0.5f, 0);
        Quaternion rightRot = Quaternion.Euler(0, angle * 0.5f, 0);

        Gizmos.DrawRay(origin, leftRot * forward * range);
        Gizmos.DrawRay(origin, rightRot * forward * range);
    }

}
