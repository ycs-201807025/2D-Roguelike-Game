using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업그레이드 타입 정의
/// </summary>
public enum UpgradeType
{
    MaxHealth,      // 최대 체력
    AttackDamage,   // 공격력
    MoveSpeed,      // 이동 속도
    CritChance,     // 치명타 확률
    CritDamage,     // 치명타 피해
    StartGold       // 시작 골드
}

/// <summary>
/// 영구 강화 데이터
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Basic Info")]
    public string upgradeName = "Upgrade";
    public UpgradeType upgradeType;
    [TextArea(2, 4)]
    public string description = "업그레이드 설명";

    [Header("Stats")]
    public int maxLevel = 5;

    [Tooltip("각 레벨의 비용 (영혼)")]
    public int[] costs = new int[5] { 50, 100, 200, 400, 800 };

    [Tooltip("각 레벨의 증가값")]
    public int[] values = new int[5] { 20, 20, 20, 20, 20 };

    /// <summary>
    /// 특정 레벨의 비용
    /// </summary>
    public int GetCost(int level)
    {
        if (level < 0 || level >= costs.Length)
            return 0;
        return costs[level];
    }

    /// <summary>
    /// 특정 레벨의 증가값
    /// </summary>
    public int GetValue(int level)
    {
        if (level < 0 || level >= values.Length)
            return 0;
        return values[level];
    }

    /// <summary>
    /// 현재 레벨까지의 총 증가값
    /// </summary>
    public int GetTotalValue(int currentLevel)
    {
        int total = 0;
        for (int i = 0; i < currentLevel && i < values.Length; i++)
        { 
            total += values[i];
        }
        return total;
    }
}
