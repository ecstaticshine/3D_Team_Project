using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Bullet : MonoBehaviour
{
    [SerializeField] private float baseDamage = 50f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Target")) return;

        S_Enemy enemy = other.GetComponentInParent<S_Enemy>();

        if (enemy != null)
        {
            float finalDamage = baseDamage;

            if (other.TryGetComponent(out S_HitBox hitBox))
            {
                finalDamage *= hitBox.damageMultiplier;
            }

            enemy.TakeDamage(finalDamage);
        }
        else
        {
            Debug.Log($"벽 또는 오브젝트 : {other.name}");
        }

        Destroy(gameObject);
    }
}
