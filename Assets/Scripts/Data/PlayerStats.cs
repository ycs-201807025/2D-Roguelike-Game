using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// MVP 패턴의 Model: 플레이어 스탯 데이터
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private int baseAttackDamage = 10;
    [SerializeField] private float baseMoveSpeed = 5f;

    // 현재 스탯 (기본 + 강화)
    private int maxHealth;
    private int currentHealth;
    private int attackDamage;
    private float moveSpeed;
    private float critChance;
    private float critDamage = 150f;

    // 재화
    private int gold;
    private int souls;

    // 이벤트
    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnGoldChanged;
    public event Action<int> OnSoulsChanged;
    public event Action OnPlayerDied;

    // 접근자
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int AttackDamage => attackDamage;
    public float MoveSpeed => moveSpeed;
    public float CritChance => critChance;
    public float CritDamage => critDamage;
    public int Gold => gold;
    public int Souls => souls;

    private SynergyManager synergyManager;

    void Start()
    {
        synergyManager = SynergyManager.Instance;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeStats();
    }

    void InitializeStats()
    {
        // 영구 강화 데이터 불러오기
        UpdateFromPersistentData();

        // 체력을 최대로
        currentHealth = maxHealth;

        Debug.Log($"[STATS] Initialized - HP: {maxHealth}, ATK: {attackDamage}, SPD: {moveSpeed}");
        Debug.Log($"[STATS] Crit Chance: {critChance}%, Crit Damage: {critDamage}%");
        Debug.Log($"[STATS] Start Gold: {gold}");
    }
    
    /// <summary>
    /// 영구 강화 데이터를 스탯에 적용
    /// </summary>
    public void UpdateFromPersistentData()
    {
        var dataManager = PersistentDataManager.Instance;
        if (dataManager == null)
        {
            Debug.LogWarning("[STATS] PersistentDataManager not found, using base stats");
            maxHealth = baseMaxHealth;
            attackDamage = baseAttackDamage;
            moveSpeed = baseMoveSpeed;
            critChance = 0f;
            critDamage = 150f;
            return;
        }

        // 기본 스탯 + 강화 스탯
        maxHealth = baseMaxHealth + dataManager.GetTotalUpgradeValue(UpgradeType.MaxHealth);
        attackDamage = baseAttackDamage + dataManager.GetTotalUpgradeValue(UpgradeType.AttackDamage);
        moveSpeed = baseMoveSpeed + dataManager.GetTotalUpgradeValue(UpgradeType.MoveSpeed);
        critChance = dataManager.GetTotalUpgradeValue(UpgradeType.CritChance);
        critDamage = 150f + dataManager.GetTotalUpgradeValue(UpgradeType.CritDamage);

        // 시작 골드
        gold = dataManager.GetTotalUpgradeValue(UpgradeType.StartGold);

        Debug.Log($"[STATS] Updated from persistent data:");
        Debug.Log($"  Max Health: {maxHealth} ({baseMaxHealth} base + upgrades)");
        Debug.Log($"  Attack: {attackDamage} ({baseAttackDamage} base + upgrades)");
        Debug.Log($"  Move Speed: {moveSpeed:F1} ({baseMoveSpeed} base + upgrades)");
        Debug.Log($"  Crit Chance: {critChance}%");
        Debug.Log($"  Crit Damage: {critDamage}%");
        Debug.Log($"  Start Gold: {gold}");

        // 이벤트 발생
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnGoldChanged?.Invoke(gold);
    }

    /// <summary>
    /// 체력 직접 설정 (초기화용)
    /// </summary>
    public void SetHealth(int current, int max)
    {
        maxHealth = max;
        currentHealth = Mathf.Clamp(current, 0, max);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[STATS] Health set to {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 데미지 계산 (치명타 + 시너지 효과 포함)
    /// </summary>
    public int CalculateDamage()
    {
        // 시너지 효과 가져오기
        SynergyEffect effects = synergyManager != null ?
            synergyManager.CalculateSynergyEffects() : new SynergyEffect();

        // 기본 공격력에 시너지 배율 적용
        float damage = attackDamage * effects.atkMultiplier;

        // 치명타 판정 (기본 + 시너지)
        float totalCritChance = critChance + effects.critChance;
        if (UnityEngine.Random.Range(0f, 100f) < totalCritChance)
        {
            float totalCritDamage = critDamage + effects.critDamage;
            damage *= (totalCritDamage / 100f);
            Debug.Log($"[COMBAT] CRITICAL HIT! {damage:F0} damage");
        }

        // 2배 대미지 확률 (힘 시너지 4)
        if (UnityEngine.Random.Range(0f, 1f) < effects.doubleDamageChance)
        {
            damage *= 2f;
            Debug.Log($"[COMBAT] DOUBLE DAMAGE! {damage:F0} damage");
        }

        return Mathf.RoundToInt(damage);
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    public void AddSouls(int amount)
    {
        souls += amount;
        OnSoulsChanged?.Invoke(souls);

        // 영구 데이터에도 저장
        if (PersistentDataManager.Instance != null)
        {
            PersistentDataManager.Instance.souls += amount;
            PersistentDataManager.Instance.SaveData();
        }
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            return true;
        }
        return false;
    }
    /// <summary>
    /// 최대 체력 증가 (아이템)
    /// </summary>
    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // 현재 체력도 같이 증가
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[STATS] Max Health increased by {amount} → {maxHealth}");
    }

    /// <summary>
    /// 공격력 증가 (아이템)
    /// </summary>
    public void AddAttackDamage(int amount)
    {
        attackDamage += amount;
        Debug.Log($"[STATS] Attack Damage increased by {amount} → {attackDamage}");
    }

    /// <summary>
    /// 이동 속도 증가 (아이템)
    /// </summary>
    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;
        Debug.Log($"[STATS] Move Speed increased by {amount} → {moveSpeed}");
    }

    /// <summary>
    /// 치명타 확률 증가 (아이템)
    /// </summary>
    public void AddCritChance(float amount)
    {
        critChance += amount;
        Debug.Log($"[STATS] Crit Chance increased by {amount}% → {critChance}%");
    }

    /// <summary>
    /// 최종 이동속도 (시너지 포함)
    /// </summary>
    public float GetFinalMoveSpeed()
    {
        if (synergyManager == null) return moveSpeed;

        SynergyEffect effects = synergyManager.CalculateSynergyEffects();
        return moveSpeed * effects.spdMultiplier;
    }
    /// <summary>
    /// 공격속도 배율 (시너지 포함)
    /// </summary>
    public float GetAttackSpeedMultiplier()
    {
        if (synergyManager == null) return 1f;

        return synergyManager.CalculateSynergyEffects().atkSpdMultiplier;
    }
    /// <summary>
    /// 드롭률 배율 (시너지 포함)
    /// </summary>
    public float GetDropRateMultiplier()
    {
        if (synergyManager == null) return 1f;

        return synergyManager.CalculateSynergyEffects().dropRateMultiplier;
    }
}
