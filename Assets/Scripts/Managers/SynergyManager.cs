using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }

    private PassiveItemManager itemManager;
    private PlayerStats playerStats; // 플레이어 스탯 참조

    private void Awake()
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

    private void Start()
    {
        itemManager = PassiveItemManager.Instance;
        playerStats = FindObjectOfType<PlayerStats>(); // 또는 다른 방법으로 참조
    }

    // 현재 시너지 효과 계산
    public SynergyEffect CalculateSynergyEffects()
    {
        SynergyEffect effects = new SynergyEffect();

        // 각 타입별 개수 세기
        int strengthCount = itemManager.CountItemsByType(PassiveItemType.Strength);
        int agilityCount = itemManager.CountItemsByType(PassiveItemType.Agility);
        int explorerCount = itemManager.CountItemsByType(PassiveItemType.Explorer);
        int mageCount = itemManager.CountItemsByType(PassiveItemType.Mage);
        int berserkerCount = itemManager.CountItemsByType(PassiveItemType.Berserker);
        int assassinCount = itemManager.CountItemsByType(PassiveItemType.Assassin);

        // 힘 시너지
        if (strengthCount >= 2)
        {
            effects.atkMultiplier += 0.05f;
        }
        if (strengthCount >= 4)
        {
            effects.atkMultiplier += 0.05f; // 총 10%
            effects.doubleDamageChance = 0.02f;
        }

        // 민첩 시너지
        if (agilityCount >= 2)
        {
            effects.spdMultiplier += 0.05f;
            effects.atkSpdMultiplier += 0.05f;
        }

        // 탐험가 시너지
        if (explorerCount >= 2)
        {
            effects.dropRateMultiplier = 1.5f;
        }
        if (explorerCount >= 4)
        {
            effects.dropRateMultiplier = 2f;
        }

        // 마법사 시너지
        if (mageCount >= 2)
        {
            effects.extraAttacks = 1;
        }
        if (mageCount >= 4)
        {
            effects.extraAttacks = 2;
            effects.tripleAttackChance = 0.1f;
        }

        // 광전사 시너지
        if (berserkerCount >= 2 && playerStats != null)
        {
            float hpLost = playerStats.MaxHealth - playerStats.CurrentHealth;
            effects.berserkerBonus = hpLost * 0.05f;
            effects.atkMultiplier += effects.berserkerBonus;
            effects.spdMultiplier += effects.berserkerBonus;
            effects.atkSpdMultiplier += effects.berserkerBonus;
        }

        // 암살자 시너지
        if (assassinCount >= 2)
        {
            effects.critChance = 0.05f;
            effects.critDamage = 1.1f;
        }

        return effects;
    }

    // 활성화된 시너지 목록 반환 (UI용)
    public List<string> GetActiveSynergies()
    {
        List<string> synergies = new List<string>();

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
