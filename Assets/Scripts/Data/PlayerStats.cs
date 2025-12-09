using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 플레이어 스탯 데이터 (Model)
/// UI와 독립적인 순수 데이터
/// </summary>
public class PlayerStats
{
    // 체력
    private int currentHealth;
    private int maxHealth;

    // 재화
    private int gold;
    private int souls;

    // 이벤트 (데이터 변경 시 Presenter에게 알림)
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action<int> OnGoldChanged;
    public event Action<int> OnSoulsChanged;
    public event Action OnPlayerDied;

    /// <summary>
    /// 생성자
    /// </summary>
    public PlayerStats(int maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
        this.gold = 0;
        this.souls = 0;
    }

    /// <summary>
    /// 체력 설정
    /// </summary>
    public void SetHealth(int current, int max)
    {
        this.maxHealth = max;
        this.currentHealth = UnityEngine.Mathf.Clamp(current, 0, max);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// 골드 추가
    /// </summary>
    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    /// <summary>
    /// 골드 소비
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
    }

    /// <summary>
    /// 영혼 소비
    /// </summary>
    public bool SpendSouls(int amount)
    {
        if (souls >= amount)
        {
            souls -= amount;
            OnSoulsChanged?.Invoke(souls);
            return true;
        }
        return false;
    }

    // Getters
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int Gold => gold;
    public int Souls => souls;
    public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
}
