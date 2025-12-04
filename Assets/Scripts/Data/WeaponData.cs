using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기 데이터 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName = "Sword";
    public Sprite weaponIcon;

    [Header("Attack Settings")]
    public int damage = 10;//공격력
    public float attackSpeed = 1f; //공격 속도(쿨타임)
    public float attackRange = 1.5f; //공격 범위

    [Header("Projectile (원거리 무기용)")]
    public GameObject projectilePrefab; //투사체 프리팹 null이면 근거리 무기
    public float projectileSpeed = 10f; //투사체 속도

    [Header("Effects")]
    public GameObject attackEffectPrefab; //공격 이펙트 프리팹

    ///<summary>
    ///근접무기 
    ///</summary>
    public bool IsMelee => projectilePrefab == null;
}
