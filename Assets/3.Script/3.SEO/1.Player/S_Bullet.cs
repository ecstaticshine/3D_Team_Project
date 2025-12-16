using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Bullet : MonoBehaviour
{
    [SerializeField] private float bodyDamage = 50f;
    [SerializeField] private float headDamage = 100f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;

        S_Enemy enemy = other.GetComponentInParent<S_Enemy>();

        if (enemy != null)
        {
            if (other.name == "Head")
            {
                Debug.Log("헤드");
                enemy.TakeDamage(headDamage);
            }
            else
            {
                Debug.Log("몸통");
                enemy.TakeDamage(bodyDamage);
            }
        }
        else
        {
            Debug.Log($"벽 명중: {other.name}");
        }

        Destroy(gameObject);
    }
}
