using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Item_Medicine : MonoBehaviour, S_IItem
{
    public void Use(S_Player player)
    {
        Debug.Log("체력 회복");
        player.RestoreAbilityGauge();
    }
}
