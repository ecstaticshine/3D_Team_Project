using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunFireMode
{
    SemiAuto,
    FullAuto,
    Shotgun
}

[CreateAssetMenu(menuName = "ScriptableObejct/GunData", fileName = "GunData")]
public class GunData : ScriptableObject
{

    [Header("총 기본 정보")]

    [Lavel("총 이름")]
    public string gunName;

    [Lavel("발사 형식")]
    public GunFireMode fireMode;

    [Lavel("총 프리팹")]
    public GameObject gunPrefab;

    [Lavel("아이템 크기")]
    public Vector3 itemScale = Vector3.one;

    [Lavel("총 애니메이터")]
    public AnimatorOverrideController gunAnimation;

    [Lavel("총알 프리팹")]
    public GameObject bulletPrefab;

    [Header("총 성능 정보")]

    [Lavel("총알 속도")]
    public float bulletSpeed;

    [Lavel("연사 속도")]
    public float fireDelay;

    [Lavel("장전 시간")]
    public float reloadTime;

    [Lavel("탄창 용량")]
    public int maxAmmo;

    [Header("샷건 전용 설정")]

    [Lavel("총알 개수")]
    public int pelletCount = 5;

    [Lavel("총알 각도")]
    public float spreadAngle = 15f;

    [Header("총 사운드 & 이펙트")]

    [Lavel("발사 소리 이름")]
    public string fireSoundName;

    //[S_Lavel("발사 이펙트")]
    //public ? effect;
}
