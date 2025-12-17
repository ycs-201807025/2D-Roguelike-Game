using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시너지 효과 계산 매니저
/// </summary>
public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }

    private PassiveItemManager itemManager;
    private PlayerStats playerStats;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        itemManager = PassiveItemManager.Instance;
        playerStats = PlayerStats.Instance;

        if (itemManager == null)
        {
            Debug.LogError("[SYNERGY MANAGER] PassiveItemManager not found!");
        }
    }

    /// <summary>
    /// 현재 시너지 효과 계산
    /// </summary>
    public SynergyEffect CalculateSynergyEffects()
    {
        SynergyEffect effects = new SynergyEffect();

        if (itemManager == null) return effects;

        // 각 타입별 개수 세기
        int strengthCount = itemManager.CountItemsByType(PassiveItemType.Strength);
        int agilityCount = itemManager.CountItemsByType(PassiveItemType.Agility);
        int explorerCount = itemManager.CountItemsByType(PassiveItemType.Explorer);
        int mageCount = itemManager.CountItemsByType(PassiveItemType.Mage);
        int berserkerCount = itemManager.CountItemsByType(PassiveItemType.Berserker);
        int assassinCount = itemManager.CountItemsByType(PassiveItemType.Assassin);

        // === 힘 시너지 ===
        if (strengthCount >= 2)
        {
            effects.atkMultiplier += 0.05f; // 5% 증가
        }
        if (strengthCount >= 4)
        {
            effects.atkMultiplier += 0.05f; // 추가 5% (총 10%)
            effects.doubleDamageChance = 0.02f; // 2% 확률 2배 대미지
        }

        // === 민첩 시너지 ===
        if (agilityCount >= 2)
        {
            effects.spdMultiplier += 0.05f;     // 이동속도 5% 증가
            effects.atkSpdMultiplier += 0.05f;  // 공격속도 5% 증가
        }

        // === 탐험가 시너지 ===
        if (explorerCount >= 2)
        {
            effects.dropRateMultiplier = 1.5f; // 1.5배
        }
        if (explorerCount >= 4)
        {
            effects.dropRateMultiplier = 2f; // 2배
        }

        // === 마법사 시너지 ===
        if (mageCount >= 2)
        {
            effects.extraAttacks = 1; // 추가 공격 1발
        }
        if (mageCount >= 4)
        {
            effects.extraAttacks = 2; // 추가 공격 2발
            effects.tripleAttackChance = 0.1f; // 10% 확률로 1발 더
        }

        // === 광전사 시너지 ===
        if (berserkerCount >= 2 && playerStats != null)
        {
            float hpLost = playerStats.MaxHealth - playerStats.CurrentHealth;
            effects.berserkerBonus = hpLost * 0.05f;

            effects.atkMultiplier += effects.berserkerBonus;
            effects.spdMultiplier += effects.berserkerBonus;
            effects.atkSpdMultiplier += effects.berserkerBonus;
        }

        // === 암살자 시너지 ===
        if (assassinCount >= 2)
        {
            effects.critChance = 5f;     // 치명타 확률 5% 증가
            effects.critDamage = 10f;    // 치명타 대미지 10% 증가
        }

        return effects;
    }

    /// <summary>
    /// 활성화된 시너지 목록 반환 (UI용)
    /// </summary>
    public List<string> GetActiveSynergies()
    {
        List<string> synergies = new List<string>();

        if (itemManager == null) return synergies;

        int strengthCount = itemManager.CountItemsByType(PassiveItemType.Strength);
        int agilityCount = itemManager.CountItemsByType(PassiveItemType.Agility);
        int explorerCount = itemManager.CountItemsByType(PassiveItemType.Explorer);
        int mageCount = itemManager.CountItemsByType(PassiveItemType.Mage);
        int berserkerCount = itemManager.CountItemsByType(PassiveItemType.Berserker);
        int assassinCount = itemManager.CountItemsByType(PassiveItemType.Assassin);

        if (strengthCount >= 2) synergies.Add($"💪 힘 ({strengthCount})");
        if (agilityCount >= 2) synergies.Add($"🪶 민첩 ({agilityCount})");
        if (explorerCount >= 2) synergies.Add($"🧭 탐험 ({explorerCount})");
        if (mageCount >= 2) synergies.Add($"🔮 마법 ({mageCount})");
        if (berserkerCount >= 2) synergies.Add($"⚔️ 광전 ({berserkerCount})");
        if (assassinCount >= 2) synergies.Add($"🗡️ 암살 ({assassinCount})");

        return synergies;
    }
}
