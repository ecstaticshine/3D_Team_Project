using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Item_Medicine : MonoBehaviour, S_IItem
{
    public void Use(S_Player player)
    {
        player.RestoreAbilityGauge();
    }
}
