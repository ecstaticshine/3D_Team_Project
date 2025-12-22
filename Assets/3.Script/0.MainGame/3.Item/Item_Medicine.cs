using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Medicine : MonoBehaviour, IItem
{
    public void Use(Player player)
    {
        player.Tranquilizer();
    }
}
