using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영구 강화 타입
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
    public UpgradeType upgradeType = UpgradeType.MaxHealth;

    [TextArea(2, 4)]
    public string description = "Description here";

    [Header("Icon")]
    public Sprite icon;

    [Header("Stats")]
    public int maxLevel = 5;
    public int[] costs = new int[5] { 50, 100, 200, 400, 800 }; // 레벨별 비용
    public int[] values = new int[5] { 20, 40, 60, 80, 100 }; // 레벨별 효과

    /// <summary>
    /// 특정 레벨의 비용
    /// </summary>
    public int GetCostForLevel(int level)
    {
        if (level < 0 || level >= costs.Length) return 0;
        return costs[level];
    }

    /// <summary>
    /// 특정 레벨의 효과값
    /// </summary>
    public int GetValueForLevel(int level)
    {
        if (level < 0 || level >= values.Length) return 0;
        return values[level];
    }
}
