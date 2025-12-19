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
    public WeaponType weaponType = WeaponType.Sword; 
    public WeaponRarity rarity = WeaponRarity.Common; 

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

    /// <summary>
    /// 등급에 따른 색상 반환
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case WeaponRarity.Common:
                return new Color(0.7f, 0.7f, 0.7f); // 회색
            case WeaponRarity.Uncommon:
                return new Color(0.2f, 0.8f, 0.2f); // 초록
            case WeaponRarity.Rare:
                return new Color(0.2f, 0.5f, 1f);   // 파랑
            case WeaponRarity.Epic:
                return new Color(0.6f, 0.2f, 0.8f); // 보라
            case WeaponRarity.Legendary:
                return new Color(1f, 0.6f, 0f);     // 주황
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// 등급명 반환
    /// </summary>
    public string GetRarityName()
    {
        switch (rarity)
        {
            case WeaponRarity.Common: return "일반";
            case WeaponRarity.Uncommon: return "고급";
            case WeaponRarity.Rare: return "희귀";
            case WeaponRarity.Epic: return "영웅";
            case WeaponRarity.Legendary: return "전설";
            default: return "알 수 없음";
        }
    }
}
