using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 인벤토리 관리
/// </summary>
public class Inventory : MonoBehaviour
{
    #region Constants
    private const float LIFE_STEAL_PERCENTAGE_DIVISOR = 100f;
    private const float ATTACK_SPEED_PERCENTAGE_DIVISOR = 100f;
    #endregion

    #region Serialized Fields
    [Header("References")]
    [SerializeField] private PlayerWeapon playerWeapon;

    [Header("Current Items")]
    [SerializeField] private List<ItemData> passiveItems = new List<ItemData>();
    #endregion

    #region Bonus Stats
    // 패시브 효과 누적값
    private int bonusMaxHealth = 0;
    private int bonusAttackDamage = 0;
    private float bonusMoveSpeed = 0f;
    private float bonusAttackSpeed = 0f;
    private float bonusCritChance = 0f;
    private int bonusLifeSteal = 0;
    #endregion

    #region Properties
    // 접근자
    public int BonusMaxHealth => bonusMaxHealth;
    public int BonusAttackDamage => bonusAttackDamage;
    public float BonusMoveSpeed => bonusMoveSpeed;
    public float BonusAttackSpeed => bonusAttackSpeed;
    public float BonusCritChance => bonusCritChance;
    public int BonusLifeSteal => bonusLifeSteal;
    public List<ItemData> PassiveItems => passiveItems;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeComponents();
        InitializePassiveItems();

        Debug.Log("[INVENTORY] Initialized");
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        if (playerWeapon == null)
        {
            playerWeapon = GetComponent<PlayerWeapon>();
        }

        if (playerWeapon == null)
        {
            Debug.LogWarning("[INVENTORY] PlayerWeapon not found!");
        }
    }

    /// <summary>
    /// 패시브 아이템 리스트 초기화
    /// </summary>
    private void InitializePassiveItems()
    {
        if (passiveItems == null)
        {
            passiveItems = new List<ItemData>();
        }
    }
    #endregion

    #region Item Management
    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(ItemData item)
    {
        if (!ValidateItem(item))
        {
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
    /// 아이템 유효성 검사
    /// </summary>
    private bool ValidateItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("[INVENTORY] Cannot add null item");
            return false;
        }
        return true;
    }
    #endregion

    #region Weapon Management
    /// <summary>
    /// 무기 추가 (교체)
    /// </summary>
    void AddWeapon(ItemData item)
    {
        if (!ValidateWeaponData(item))
        {
            return;
        }

        EquipWeapon(item.weaponData);
    }
    /// <summary>
    /// 무기 데이터 유효성 검사
    /// </summary>
    private bool ValidateWeaponData(ItemData item)
    {
        if (item.weaponData == null)
        {
            Debug.LogError($"[INVENTORY] {item.itemName} has no weapon data!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 무기 장착
    /// </summary>
    private void EquipWeapon(WeaponData weapon)
    {
        if (playerWeapon != null)
        {
            playerWeapon.EquipWeapon(weapon);
            Debug.Log($"[INVENTORY] Equipped weapon: {weapon.weaponName}");
        }
    }
    #endregion

    #region Passive Item Management
    /// <summary>
    /// 패시브 아이템 추가 (중첩)
    /// </summary>
    void AddPassive(ItemData item)
    {
        passiveItems.Add(item);
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
                ApplyMaxHealthBonus(item.effectValue);
                break;

            case PassiveEffectType.AttackDamageUp:
                ApplyAttackDamageBonus(item.effectValue);
                break;

            case PassiveEffectType.MoveSpeedUp:
                ApplyMoveSpeedBonus(item.effectValue);
                break;

            case PassiveEffectType.AttackSpeedUp:
                ApplyAttackSpeedBonus(item.effectValue);
                break;

            case PassiveEffectType.CritChanceUp:
                ApplyCritChanceBonus(item.effectValue);
                break;

            case PassiveEffectType.LifeSteal:
                ApplyLifeStealBonus(item.effectValue);
                break;

            default:
                Debug.LogWarning($"[INVENTORY] Unknown effect type: {item.effectType}");
                break;
        }
    }
    /// <summary>
    /// 최대 체력 보너스 적용
    /// </summary>
    private void ApplyMaxHealthBonus(int value)
    {
        bonusMaxHealth += value;
        Debug.Log($"[INVENTORY] Bonus Max Health: +{bonusMaxHealth}");

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddMaxHealth(value);
        }
    }

    /// <summary>
    /// 공격력 보너스 적용
    /// </summary>
    private void ApplyAttackDamageBonus(int value)
    {
        bonusAttackDamage += value;
        Debug.Log($"[INVENTORY] Bonus Attack: +{bonusAttackDamage}");

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddAttackDamage(value);
        }
    }

    /// <summary>
    /// 이동 속도 보너스 적용
    /// </summary>
    private void ApplyMoveSpeedBonus(int value)
    {
        bonusMoveSpeed += value;
        Debug.Log($"[INVENTORY] Bonus Speed: +{bonusMoveSpeed}");

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddMoveSpeed(value);
        }
    }

    /// <summary>
    /// 공격 속도 보너스 적용
    /// </summary>
    private void ApplyAttackSpeedBonus(int value)
    {
        bonusAttackSpeed += value;
        Debug.Log($"[INVENTORY] Bonus Attack Speed: +{bonusAttackSpeed}%");
    }

    /// <summary>
    /// 치명타 확률 보너스 적용
    /// </summary>
    private void ApplyCritChanceBonus(int value)
    {
        bonusCritChance += value;
        Debug.Log($"[INVENTORY] Bonus Crit Chance: +{bonusCritChance}%");

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddCritChance(value);
        }
    }

    /// <summary>
    /// 생명력 흡수 보너스 적용
    /// </summary>
    private void ApplyLifeStealBonus(int value)
    {
        bonusLifeSteal += value;
        Debug.Log($"[INVENTORY] Life Steal: {bonusLifeSteal}%");
    }
    #endregion

    #region Combat Calculations
    /// <summary>
    /// 공격 속도 배율 (1.0 = 100%)
    /// </summary>
    public float GetAttackSpeedMultiplier()
    {
        return 1f + (bonusAttackSpeed / ATTACK_SPEED_PERCENTAGE_DIVISOR);
    }

    /// <summary>
    /// 생명력 흡수 계산
    /// </summary>
    public int CalculateLifeSteal(int damageDealt)
    {
        if (bonusLifeSteal <= 0)
        {
            return 0;
        }

        int healAmount = Mathf.RoundToInt(
            damageDealt * bonusLifeSteal / LIFE_STEAL_PERCENTAGE_DIVISOR
        );

        return healAmount;
    }
    #endregion
}
