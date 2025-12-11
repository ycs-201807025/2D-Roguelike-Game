using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 타입 정의
/// </summary>
public enum ItemType
{
    Weapon,     // 무기 (교체형)
    Passive     // 패시브 (중첩형)
}

/// <summary>
/// 패시브 효과 타입
/// </summary>
public enum PassiveEffectType
{
    MaxHealthUp,        // 최대 체력 증가
    AttackDamageUp,     // 공격력 증가
    MoveSpeedUp,        // 이동 속도 증가
    AttackSpeedUp,      // 공격 속도 증가
    CritChanceUp,       // 치명타 확률 증가
    CritDamageUp,       // 치명타 피해 증가
    LifeSteal,          // 생명력 흡수
    Thorns              // 가시 (반사 데미지)
}

/// <summary>
/// 아이템 데이터
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName = "Item";
    public ItemType itemType;
    [TextArea(2, 4)]
    public string description = "아이템 설명";
    public Sprite icon;

    [Header("Weapon (무기인 경우)")]
    public WeaponData weaponData; // 무기 데이터 참조

    [Header("Passive Effect (패시브인 경우)")]
    public PassiveEffectType effectType;
    public int effectValue; // 효과 값 (예: +10 공격력, +5% 치명타)

    [Header("Rarity")]
    [Range(0f, 1f)]
    public float dropChance = 0.5f; // 드롭 확률 (0~1)
}
