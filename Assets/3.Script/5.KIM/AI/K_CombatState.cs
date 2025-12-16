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

    public void Enter(AIController ai)
    {
        fireCooldown = Random.Range(MIN_FIRE_INTERVAL, MAX_FIRE_INTERVAL);
        lostSightTimer = 0f;
        ai.Agent.isStopped = false;
    }

    public void Execute(AIController ai)
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
        if (ai.combatType == AIController.CombatType.Melee)
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

    private void HandleMeleeCombat(AIController ai, Vector3 playerPos, float distance)
    {
        if (distance > ai.meleeEngageDistance)
        {
            Move(ai, playerPos);
        }
        else
        {
            ai.StopMove();
        }
    }

    private void HandleRangedCombat(AIController ai, Vector3 playerPos, float distance)
    {
        if (distance < ai.rangedEngageDistance)
        {
            Vector3 retreatDir = (ai.transform.position - playerPos).normalized;
            Vector3 retreatPos = ai.transform.position + retreatDir * ai.retreatDistance;
            Move(ai, retreatPos);
        }
        else
        {
            ai.StopMove();
        }
    }

    private void Move(AIController ai, Vector3 targetPos)
    {
        if (!ai.Agent.pathPending &&
            (ai.Agent.destination - targetPos).sqrMagnitude > 1f)
        {
            ai.MoveTo(targetPos);
        }
    }

    public void Exit(AIController ai)
    {
        ai.StopMove();
    }
}
