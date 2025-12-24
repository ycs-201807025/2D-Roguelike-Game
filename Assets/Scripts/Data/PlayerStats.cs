using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// MVP 패턴의 Model: 플레이어 스탯 데이터
/// </summary>
public class PlayerStats : MonoBehaviour
{
    #region Singleton
    public static PlayerStats Instance { get; private set; }
    #endregion

    #region Constants
    private const float GOLD_TO_SOULS_RATIO = 0.1f;
    #endregion

    #region Base Stats
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private int baseAttackDamage = 10;
    [SerializeField] private float baseMoveSpeed = 5f;
    #endregion

    #region Current Stats
    // 현재 스탯 (기본 + 강화)
    private int maxHealth;
    private int currentHealth;
    private int attackDamage;
    private float moveSpeed;
    private float critChance;
    private float critDamage = 150f;
    #endregion

    #region Currency
    // 재화
    private int gold;
    private int souls;
    #endregion

    #region Events
    // 이벤트
    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnGoldChanged;
    public event Action<int> OnSoulsChanged;
    public event Action OnPlayerDied;
    #endregion

    #region Properties
    // 접근자
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int AttackDamage => attackDamage;
    public float MoveSpeed => moveSpeed;
    public float CritChance => critChance;
    public float CritDamage => critDamage;
    public int Gold => gold;
    public int Souls => souls;
    #endregion

    #region Components
    private SynergyManager synergyManager;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeSingleton();
    }
    void Start()
    {
        synergyManager = SynergyManager.Instance;
        InitializeStats();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 싱글톤 초기화
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 스탯 초기화
    /// </summary>
    void InitializeStats()
    {
        // 영구 강화 데이터 불러오기
        UpdateFromPersistentData();

        // 체력을 최대로
        currentHealth = maxHealth;

        LogInitialStats();
    }
    /// <summary>
    /// 초기 스탯 로그
    /// </summary>
    private void LogInitialStats()
    {
        Debug.Log($"[STATS] Initialized - HP: {maxHealth}, ATK: {attackDamage}, SPD: {moveSpeed}");
        Debug.Log($"[STATS] Crit Chance: {critChance}%, Crit Damage: {critDamage}%");
        Debug.Log($"[STATS] Start Gold: {gold}");
    }
    #endregion

    #region Persistent Data Integration
    /// <summary>
    /// 영구 강화 데이터를 스탯에 적용
    /// </summary>
    public void UpdateFromPersistentData()
    {
        var dataManager = PersistentDataManager.Instance;

        if (dataManager == null)
        {
            UseBaseStats();
            return;
        }

        ApplyPersistentUpgrades(dataManager);
        LogUpdatedStats();
        NotifyStatsChanged();
    }
    /// <summary>
    /// 기본 스탯 사용 (강화 없음)
    /// </summary>
    private void UseBaseStats()
    {
        Debug.LogWarning("[STATS] PersistentDataManager not found, using base stats");

        maxHealth = baseMaxHealth;
        attackDamage = baseAttackDamage;
        moveSpeed = baseMoveSpeed;
        critChance = 0f;
        critDamage = 150f;
        gold = 0;
    }

    /// <summary>
    /// 영구 강화 적용
    /// </summary>
    private void ApplyPersistentUpgrades(PersistentDataManager dataManager)
    {
        maxHealth = baseMaxHealth + dataManager.GetTotalUpgradeValue(UpgradeType.MaxHealth);
        attackDamage = baseAttackDamage + dataManager.GetTotalUpgradeValue(UpgradeType.AttackDamage);
        moveSpeed = baseMoveSpeed + dataManager.GetTotalUpgradeValue(UpgradeType.MoveSpeed);
        critChance = dataManager.GetTotalUpgradeValue(UpgradeType.CritChance);
        critDamage = 150f + dataManager.GetTotalUpgradeValue(UpgradeType.CritDamage);
        gold = dataManager.GetTotalUpgradeValue(UpgradeType.StartGold);
    }

    /// <summary>
    /// 업데이트된 스탯 로그
    /// </summary>
    private void LogUpdatedStats()
    {
        Debug.Log($"[STATS] Updated from persistent data:");
        Debug.Log($"  Max Health: {maxHealth} ({baseMaxHealth} base + upgrades)");
        Debug.Log($"  Attack: {attackDamage} ({baseAttackDamage} base + upgrades)");
        Debug.Log($"  Move Speed: {moveSpeed:F1} ({baseMoveSpeed} base + upgrades)");
        Debug.Log($"  Crit Chance: {critChance}%");
        Debug.Log($"  Crit Damage: {critDamage}%");
        Debug.Log($"  Start Gold: {gold}");
    }

    /// <summary>
    /// 스탯 변경 알림
    /// </summary>
    private void NotifyStatsChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnGoldChanged?.Invoke(gold);
    }
    #endregion

    #region Health Management
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
    #endregion

    #region Combat Stats
    /// <summary>
    /// 데미지 계산 (치명타 + 시너지 효과 포함)
    /// </summary>
    public int CalculateDamage()
    {
        SynergyEffect effects = GetSynergyEffects();
        float damage = CalculateBaseDamage(effects);

        damage = ApplyCriticalHit(damage, effects);
        damage = ApplyDoubleDamage(damage, effects);

        return Mathf.RoundToInt(damage);
    }
    /// <summary>
    /// 시너지 효과 가져오기
    /// </summary>
    private SynergyEffect GetSynergyEffects()
    {
        if (synergyManager != null)
        {
            return synergyManager.CalculateSynergyEffects();
        }
        return new SynergyEffect();
    }

    /// <summary>
    /// 기본 데미지 계산
    /// </summary>
    private float CalculateBaseDamage(SynergyEffect effects)
    {
        return attackDamage * effects.atkMultiplier;
    }

    /// <summary>
    /// 치명타 적용
    /// </summary>
    private float ApplyCriticalHit(float damage, SynergyEffect effects)
    {
        float totalCritChance = critChance + effects.critChance;

        if (UnityEngine.Random.Range(0f, 100f) < totalCritChance)
        {
            float totalCritDamage = critDamage + effects.critDamage;
            damage *= (totalCritDamage / 100f);
            Debug.Log($"[COMBAT] CRITICAL HIT! {damage:F0} damage");
        }

        return damage;
    }

    /// <summary>
    /// 2배 대미지 적용 (힘 시너지)
    /// </summary>
    private float ApplyDoubleDamage(float damage, SynergyEffect effects)
    {
        if (UnityEngine.Random.Range(0f, 1f) < effects.doubleDamageChance)
        {
            damage *= 2f;
            Debug.Log($"[COMBAT] DOUBLE DAMAGE! {damage:F0} damage");
        }

        return damage;
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
    /// 치명타 확률 증가 (아이템)
    /// </summary>
    public void AddCritChance(float amount)
    {
        critChance += amount;
        Debug.Log($"[STATS] Crit Chance increased by {amount}% → {critChance}%");
    }
    #endregion

    #region Movement Stats
    /// <summary>
    /// 이동 속도 증가 (아이템)
    /// </summary>
    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;
        Debug.Log($"[STATS] Move Speed increased by {amount} → {moveSpeed}");
    }

    /// <summary>
    /// 최종 이동속도 (시너지 포함)
    /// </summary>
    public float GetFinalMoveSpeed()
    {
        if (synergyManager == null)
        {
            return moveSpeed;
        }

        SynergyEffect effects = synergyManager.CalculateSynergyEffects();
        return moveSpeed * effects.spdMultiplier;
    }

    /// <summary>
    /// 공격속도 배율 (시너지 포함)
    /// </summary>
    public float GetAttackSpeedMultiplier()
    {
        if (synergyManager == null)
        {
            return 1f;
        }

        return synergyManager.CalculateSynergyEffects().atkSpdMultiplier;
    }
    #endregion

    #region Currency Management
    /// <summary>
    /// 골드 추가
    /// </summary>
    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    /// <summary>
    /// 골드 사용
    /// </summary>
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
    /// 영혼 추가
    /// </summary>
    public void AddSouls(int amount)
    {
        souls += amount;
        OnSoulsChanged?.Invoke(souls);

        SaveSoulsToPersistentData(amount);
    }

    /// <summary>
    /// 영혼을 영구 데이터에 저장
    /// </summary>
    private void SaveSoulsToPersistentData(int amount)
    {
        if (PersistentDataManager.Instance != null)
        {
            PersistentDataManager.Instance.souls += amount;
            PersistentDataManager.Instance.SaveData();
        }
    }
    #endregion

    #region Drop Rate
    /// <summary>
    /// 드롭률 배율 (시너지 포함)
    /// </summary>
    public float GetDropRateMultiplier()
    {
        if (synergyManager == null)
        {
            return 1f;
        }

        return synergyManager.CalculateSynergyEffects().dropRateMultiplier;
    }
    #endregion
}
