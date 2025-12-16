using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Item_Gun : MonoBehaviour, S_IItem
{
    [SerializeField] private int ammoAmount = 30;

    public void Use(S_Player player)
    {
        player.gun.AddAmmo(ammoAmount);
    }
}
