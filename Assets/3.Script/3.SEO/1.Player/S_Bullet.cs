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
        if (!other.CompareTag("Target")) return;

        S_Enemy enemy = other.GetComponentInParent<S_Enemy>();

        if (enemy != null)
        {
            if (other.name == "Head")
            {
                Debug.Log("Çìµå");
                enemy.TakeDamage(headDamage);
            }
            else
            {
                Debug.Log("¸öÅë");
                enemy.TakeDamage(bodyDamage);
            }
        }
        else
        {
            Debug.Log($"º® : {other.name}");
        }

        Destroy(gameObject);
    }
}
