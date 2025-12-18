using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 인벤토리 관리
/// </summary>
public class Inventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeapon playerWeapon;

    [Header("Current Items")]
    [SerializeField] private List<ItemData> passiveItems = new List<ItemData>();

    // 패시브 효과 누적값
    private int bonusMaxHealth = 0;
    private int bonusAttackDamage = 0;
    private float bonusMoveSpeed = 0f;
    private float bonusAttackSpeed = 0f;
    private float bonusCritChance = 0f;
    private int bonusLifeSteal = 0;

    // 접근자
    public int BonusMaxHealth => bonusMaxHealth;
    public int BonusAttackDamage => bonusAttackDamage;
    public float BonusMoveSpeed => bonusMoveSpeed;
    public float BonusAttackSpeed => bonusAttackSpeed;
    public float BonusCritChance => bonusCritChance;
    public int BonusLifeSteal => bonusLifeSteal;
    public List<ItemData> PassiveItems => passiveItems;

    void Awake()
    {
        if (playerWeapon == null)
        {
            playerWeapon = GetComponent<PlayerWeapon>();
        }
        if (playerWeapon == null)
        {
            Debug.LogWarning("[INVENTORY] PlayerWeapon not found!");
        }

        // ★★★ 리스트 초기화
        if (passiveItems == null)
        {
            passiveItems = new List<ItemData>();
        }

        Debug.Log("[INVENTORY] Initialized");
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("[INVENTORY] Cannot add null item");
            return;
        }

        Debug.Log($"[INVENTORY] Adding item: {item.itemName}");

        switch (item.itemType)
        {
            case ItemType.Weapon:
                AddWeapon(item);
                break;

            case ItemType.Passive:
                AddPassive(item);
                break;
            default:
                Debug.LogWarning($"[INVENTORY] Unknown item type: {item.itemType}");
                break;
        }
    }

    /// <summary>
    /// 무기 추가 (교체)
    /// </summary>
    void AddWeapon(ItemData item)
    {
        if (item.weaponData == null)
        {
            Debug.LogError($"[INVENTORY] {item.itemName} has no weapon data!");
            return;
        }

        if (playerWeapon != null)
        {
            playerWeapon.EquipWeapon(item.weaponData);
            Debug.Log($"[INVENTORY] Equipped weapon: {item.itemName}");
        }
    }

    /// <summary>
    /// 패시브 아이템 추가 (중첩)
    /// </summary>
    void AddPassive(ItemData item)
    {
        if (passiveItems == null)
        {
            passiveItems = new List<ItemData>();
        }
        passiveItems.Add(item);

        // 효과 적용
        ApplyPassiveEffect(item);

        Debug.Log($"[INVENTORY] Added passive: {item.itemName}");
        Debug.Log($"[INVENTORY] Total passives: {passiveItems.Count}");
    }

    /// <summary>
    /// 패시브 효과 적용
    /// </summary>
    void ApplyPassiveEffect(ItemData item)
    {
        switch (item.effectType)
        {
            case PassiveEffectType.MaxHealthUp:
                bonusMaxHealth += item.effectValue;
                Debug.Log($"[INVENTORY] Bonus Max Health: +{bonusMaxHealth}");

                // PlayerStats에 즉시 적용
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.AddMaxHealth(item.effectValue);
                }
                break;

            case PassiveEffectType.AttackDamageUp:
                bonusAttackDamage += item.effectValue;
                Debug.Log($"[INVENTORY] Bonus Attack: +{bonusAttackDamage}");

                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.AddAttackDamage(item.effectValue);
                }
                break;

            case PassiveEffectType.MoveSpeedUp:
                bonusMoveSpeed += item.effectValue;
                Debug.Log($"[INVENTORY] Bonus Speed: +{bonusMoveSpeed}");

                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.AddMoveSpeed(item.effectValue);
                }
                break;

            case PassiveEffectType.AttackSpeedUp:
                bonusAttackSpeed += item.effectValue;
                Debug.Log($"[INVENTORY] Bonus Attack Speed: +{bonusAttackSpeed}%");
                break;

            case PassiveEffectType.CritChanceUp:
                bonusCritChance += item.effectValue;
                Debug.Log($"[INVENTORY] Bonus Crit Chance: +{bonusCritChance}%");

                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.AddCritChance(item.effectValue);
                }
                break;

            case PassiveEffectType.LifeSteal:
                bonusLifeSteal += item.effectValue;
                Debug.Log($"[INVENTORY] Life Steal: {bonusLifeSteal}%");
                break;
        }
    }

    /// <summary>
    /// 공격 속도 배율 (1.0 = 100%)
    /// </summary>
    public float GetAttackSpeedMultiplier()
    {
        return 1f + (bonusAttackSpeed / 100f);
    }

    /// <summary>
    /// 생명력 흡수 계산
    /// </summary>
    public int CalculateLifeSteal(int damageDealt)
    {
        if (bonusLifeSteal <= 0) return 0;

        int healAmount = Mathf.RoundToInt(damageDealt * bonusLifeSteal / 100f);
        return healAmount;
    }
}
