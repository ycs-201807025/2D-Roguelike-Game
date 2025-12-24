using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시너지 효과 계산 매니저
/// </summary>
public class SynergyManager : MonoBehaviour
{
    #region Singleton
    public static SynergyManager Instance { get; private set; }
    #endregion

    #region Constants
    private const int SYNERGY_TIER1_THRESHOLD = 2;
    private const int SYNERGY_TIER2_THRESHOLD = 4;

    private const float STRENGTH_TIER1_ATK_BONUS = 0.05f;
    private const float STRENGTH_TIER2_ATK_BONUS = 0.05f;
    private const float STRENGTH_DOUBLE_DAMAGE_CHANCE = 0.02f;

    private const float AGILITY_SPD_BONUS = 0.05f;
    private const float AGILITY_ATK_SPD_BONUS = 0.05f;

    private const float EXPLORER_TIER1_DROP_MULTIPLIER = 1.5f;
    private const float EXPLORER_TIER2_DROP_MULTIPLIER = 2f;

    private const int MAGE_TIER1_EXTRA_ATTACKS = 1;
    private const int MAGE_TIER2_EXTRA_ATTACKS = 2;
    private const float MAGE_TRIPLE_ATTACK_CHANCE = 0.1f;

    private const float BERSERKER_HP_LOSS_MULTIPLIER = 0.05f;

    private const float ASSASSIN_CRIT_CHANCE_BONUS = 5f;
    private const float ASSASSIN_CRIT_DAMAGE_BONUS = 10f;
    #endregion

    #region Components
    private PassiveItemManager itemManager;
    private PlayerStats playerStats;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeSingleton();
    }

    void Start()
    {
        InitializeComponents();
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        itemManager = PassiveItemManager.Instance;
        playerStats = PlayerStats.Instance;

        if (itemManager == null)
        {
            Debug.LogError("[SYNERGY MANAGER] PassiveItemManager not found!");
        }
    }
    #endregion

    #region Synergy Calculation
    /// <summary>
    /// 현재 시너지 효과 계산
    /// </summary>
    public SynergyEffect CalculateSynergyEffects()
    {
        if (itemManager == null)
        {
            return new SynergyEffect();
        }

        SynergyEffect effects = new SynergyEffect();
        ItemCounts counts = GetItemCounts();

        ApplyStrengthSynergy(effects, counts.strengthCount);
        ApplyAgilitySynergy(effects, counts.agilityCount);
        ApplyExplorerSynergy(effects, counts.explorerCount);
        ApplyMageSynergy(effects, counts.mageCount);
        ApplyBerserkerSynergy(effects, counts.berserkerCount);
        ApplyAssassinSynergy(effects, counts.assassinCount);

        return effects;
    }
    /// <summary>
    /// 아이템 개수 집계
    /// </summary>
    private ItemCounts GetItemCounts()
    {
        return new ItemCounts
        {
            strengthCount = itemManager.CountItemsByType(PassiveItemType.Strength),
            agilityCount = itemManager.CountItemsByType(PassiveItemType.Agility),
            explorerCount = itemManager.CountItemsByType(PassiveItemType.Explorer),
            mageCount = itemManager.CountItemsByType(PassiveItemType.Mage),
            berserkerCount = itemManager.CountItemsByType(PassiveItemType.Berserker),
            assassinCount = itemManager.CountItemsByType(PassiveItemType.Assassin)
        };
    }
    #endregion

    #region Individual Synergies
    /// <summary>
    /// 힘 시너지 적용
    /// </summary>
    private void ApplyStrengthSynergy(SynergyEffect effects, int count)
    {
        if (count >= SYNERGY_TIER1_THRESHOLD)
        {
            effects.atkMultiplier += STRENGTH_TIER1_ATK_BONUS;
        }

        if (count >= SYNERGY_TIER2_THRESHOLD)
        {
            effects.atkMultiplier += STRENGTH_TIER2_ATK_BONUS;
            effects.doubleDamageChance = STRENGTH_DOUBLE_DAMAGE_CHANCE;
        }
    }

    /// <summary>
    /// 민첩 시너지 적용
    /// </summary>
    private void ApplyAgilitySynergy(SynergyEffect effects, int count)
    {
        if (count >= SYNERGY_TIER1_THRESHOLD)
        {
            effects.spdMultiplier += AGILITY_SPD_BONUS;
            effects.atkSpdMultiplier += AGILITY_ATK_SPD_BONUS;
        }
    }

    /// <summary>
    /// 탐험가 시너지 적용
    /// </summary>
    private void ApplyExplorerSynergy(SynergyEffect effects, int count)
    {
        if (count >= SYNERGY_TIER1_THRESHOLD)
        {
            effects.dropRateMultiplier = EXPLORER_TIER1_DROP_MULTIPLIER;
        }

        if (count >= SYNERGY_TIER2_THRESHOLD)
        {
            effects.dropRateMultiplier = EXPLORER_TIER2_DROP_MULTIPLIER;
        }
    }

    /// <summary>
    /// 마법사 시너지 적용
    /// </summary>
    private void ApplyMageSynergy(SynergyEffect effects, int count)
    {
        if (count >= SYNERGY_TIER1_THRESHOLD)
        {
            effects.extraAttacks = MAGE_TIER1_EXTRA_ATTACKS;
        }

        if (count >= SYNERGY_TIER2_THRESHOLD)
        {
            effects.extraAttacks = MAGE_TIER2_EXTRA_ATTACKS;
            effects.tripleAttackChance = MAGE_TRIPLE_ATTACK_CHANCE;
        }
    }

    /// <summary>
    /// 광전사 시너지 적용
    /// </summary>
    private void ApplyBerserkerSynergy(SynergyEffect effects, int count)
    {
        if (count >= SYNERGY_TIER1_THRESHOLD && playerStats != null)
        {
            float hpLost = CalculateHealthLost();
            effects.berserkerBonus = hpLost * BERSERKER_HP_LOSS_MULTIPLIER;

            effects.atkMultiplier += effects.berserkerBonus;
            effects.spdMultiplier += effects.berserkerBonus;
            effects.atkSpdMultiplier += effects.berserkerBonus;
        }
    }

    /// <summary>
    /// 잃은 체력 계산
    /// </summary>
    private float CalculateHealthLost()
    {
        return playerStats.MaxHealth - playerStats.CurrentHealth;
    }

    /// <summary>
    /// 암살자 시너지 적용
    /// </summary>
    private void ApplyAssassinSynergy(SynergyEffect effects, int count)
    {
        if (count >= SYNERGY_TIER1_THRESHOLD)
        {
            effects.critChance = ASSASSIN_CRIT_CHANCE_BONUS;
            effects.critDamage = ASSASSIN_CRIT_DAMAGE_BONUS;
        }
    }
    #endregion

    #region Active Synergies
    /// <summary>
    /// 활성화된 시너지 목록 반환 (UI용)
    /// </summary>
    public List<string> GetActiveSynergies()
    {
        if (itemManager == null)
        {
            return new List<string>();
        }

        List<string> synergies = new List<string>();
        ItemCounts counts = GetItemCounts();

        AddActiveSynergy(synergies, "💪 힘", counts.strengthCount);
        AddActiveSynergy(synergies, "🪶 민첩", counts.agilityCount);
        AddActiveSynergy(synergies, "🧭 탐험", counts.explorerCount);
        AddActiveSynergy(synergies, "🔮 마법", counts.mageCount);
        AddActiveSynergy(synergies, "⚔️ 광전", counts.berserkerCount);
        AddActiveSynergy(synergies, "🗡️ 암살", counts.assassinCount);

        return synergies;
    }

    /// <summary>
    /// 활성 시너지 추가
    /// </summary>
    private void AddActiveSynergy(List<string> synergies, string name, int count)
    {
        if (count >= SYNERGY_TIER1_THRESHOLD)
        {
            synergies.Add($"{name} ({count})");
        }
    }
    #endregion

    #region Helper Classes
    /// <summary>
    /// 아이템 개수 저장 구조체
    /// </summary>
    private struct ItemCounts
    {
        public int strengthCount;
        public int agilityCount;
        public int explorerCount;
        public int mageCount;
        public int berserkerCount;
        public int assassinCount;
    }
    #endregion
}
