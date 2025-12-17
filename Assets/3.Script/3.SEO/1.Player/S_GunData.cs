using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObejct/GunData", fileName = "GunData")]
public class S_GunData : ScriptableObject
{
    [Header("총 기본 정보")]

    [S_Lavel("총 이름")]
    public string gunName;

    [S_Lavel("총 프리팹")]
    public GameObject gunPrefab;

    [S_Lavel("총 애니메이터")]
    public AnimatorOverrideController gunAnimation;

    [S_Lavel("총알 프리팹")]
    public GameObject bulletPrefab;

    [Header("총 성능 정보")]

    [S_Lavel("총알 속도")]
    public float bulletSpeed;

    [S_Lavel("연사 속도")]
    public float fireDelay;

    [S_Lavel("장전 시간")]
    public float reloadTime;

    [S_Lavel("탄창 용량")]
    public int maxAmmo;

    [Header("총 사운드 & 이펙트")]

    [S_Lavel("발사 소리 이름")]
    public string fireSoundName;

    //[S_Lavel("발사 이펙트")]
    //public ? effect;
}
