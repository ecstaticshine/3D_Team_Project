//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class CombatState : IState
{
    private float fireCooldown;
    private float lostSightTimer;

    private const float MIN_FIRE_INTERVAL = 0.4f;
    private const float MAX_FIRE_INTERVAL = 0.6f;
    private const float LOST_SIGHT_GRACE_TIME = 0.5f;

    public void Enter(K_AIController ai)
    {
        fireCooldown = Random.Range(MIN_FIRE_INTERVAL, MAX_FIRE_INTERVAL);
        lostSightTimer = 0f;
        ai.Agent.isStopped = false;
    }

    public void Execute(K_AIController ai)
    {
        Vector3 playerPos = ai.player.position;
        float distance = Vector3.Distance(ai.transform.position, playerPos);

        // 1. Vision check (with grace time)
        if (!ai.CanSeePlayer())
        {
            lostSightTimer += Time.deltaTime * ai.timeScaleMultiplier;
            if (lostSightTimer >= LOST_SIGHT_GRACE_TIME)
            {
                ai.ChangeState(ai.alertState);
            }
            return;
        }
        else
        {
            lostSightTimer = 0f;
        }

        // 2. Combat behavior by type
        if (ai.combatType == K_AIController.CombatType.Melee)
        {
            HandleMeleeCombat(ai, playerPos, distance);
        }
        else // Ranged
        {
            HandleRangedCombat(ai, playerPos, distance);
        }

        // 3. Aim
        ai.LookAt(playerPos);

        // 4. Fire
        fireCooldown -= Time.deltaTime * ai.timeScaleMultiplier;
        if (fireCooldown <= 0f)
        {
            ai.Shoot();
            fireCooldown = Random.Range(MIN_FIRE_INTERVAL, MAX_FIRE_INTERVAL);
        }
    }

    private void HandleMeleeCombat(K_AIController ai, Vector3 playerPos, float distance)
    {
        if (distance > ai.meleeEngageDistance)
        {
            Move(ai, playerPos); // 플레이어에게 붙음
        }
        else
        {
            ai.StopMove(); // 멈추고
            //공격
        }
        ai.LookAt(playerPos);
    }

    private void HandleRangedCombat(K_AIController ai, Vector3 playerPos, float distance)
    {
        // 플레이어랑 너무 가까울 시 후퇴
        if (distance < ai.rangedEngageDistance*0.8)
        {
            Vector3 retreatDir = (ai.transform.position - playerPos).normalized;
            Vector3 retreatPos = ai.transform.position + retreatDir * ai.retreatDistance;
            Move(ai, retreatPos);
        }
        // 거리가 너무 멀면 접근을 함
        else if(distance> ai.rangedEngageDistance)
        {
            Move(ai, playerPos);
        }
        // 적정한 거리면 사격하는 곳
        else
        {
            ai.StopMove(); //멈추고 공격

        }
       
    }

    private void Move(K_AIController ai, Vector3 targetPos)
    {
        if (!ai.Agent.pathPending &&
            (ai.Agent.destination - targetPos).sqrMagnitude > 1f)
        {
            ai.MoveTo(targetPos);
        }
    }

    public void Exit(K_AIController ai)
    {
        ai.StopMove();
    }
}
