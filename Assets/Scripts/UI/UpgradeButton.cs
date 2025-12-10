using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 개별 강화 버튼 UI
/// </summary>
public class UpgradeButton : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UpgradeData upgradeData;
     
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private Button button;

    private int currentLevel = 0;

    void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        UpdateUI();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    public void UpdateUI()
    {
        if (upgradeData == null) return;

        // 현재 레벨 가져오기
        currentLevel = PersistentDataManager.Instance.GetUpgradeLevel(upgradeData.upgradeType);

        // 이름
        if (nameText != null)
        {
            nameText.text = upgradeData.upgradeName;
        }

        // 레벨
        if (levelText != null)
        {
            levelText.text = $"Lv. {currentLevel} / {upgradeData.maxLevel}";
        }

        // 최대 레벨이면
        if (currentLevel >= upgradeData.maxLevel)
        {
            if (costText != null) costText.text = "MAX";
            if (effectText != null) effectText.text = "최대 레벨";
            if (button != null) button.interactable = false;
            return;
        }

        // 비용
        int cost = upgradeData.GetCostForLevel(currentLevel);
        if (costText != null)
        {
            costText.text = $"비용: {cost} 영혼";
        }

        // 효과
        int value = upgradeData.GetValueForLevel(currentLevel);
        if (effectText != null)
        {
            effectText.text = $"+{value} {GetEffectSuffix()}";
        }

        // 버튼 활성화 여부 (영혼 부족하면 비활성)
        if (button != null)
        {
            int souls = PersistentDataManager.Instance.GetSouls();
            button.interactable = souls >= cost;
        }
    }

    /// <summary>
    /// 효과 접미사
    /// </summary>
    private string GetEffectSuffix()
    {
        switch (upgradeData.upgradeType)
        {
            case UpgradeType.MaxHealth: return "체력";
            case UpgradeType.AttackDamage: return "공격력";
            case UpgradeType.MoveSpeed: return "속도";
            case UpgradeType.CritChance: return "%";
            case UpgradeType.CritDamage: return "%";
            case UpgradeType.StartGold: return "골드";
            default: return "";
        }
    }

    /// <summary>
    /// 버튼 클릭
    /// </summary>
    private void OnButtonClicked()
    {
        if (upgradeData == null) return;
        if (currentLevel >= upgradeData.maxLevel) return;

        int cost = upgradeData.GetCostForLevel(currentLevel);

        // 영혼 소비
        if (PersistentDataManager.Instance.SpendSouls(cost))
        {
            // 레벨 업
            PersistentDataManager.Instance.SetUpgradeLevel(
                upgradeData.upgradeType,
                currentLevel + 1
            );

            Debug.Log($"[UPGRADE] {upgradeData.upgradeName} upgraded to level {currentLevel + 1}");

            // UI 업데이트
            UpdateUI();

            // 다른 버튼들도 업데이트 (영혼 개수 변경됨)
            //UpgradeMenuManager manager = FindObjectOfType<UpgradeMenuManager>();
            //if (manager != null)
            //{
            //    manager.UpdateAllButtons();
            //}
        }
    }

    // Public API
    public UpgradeData UpgradeData => upgradeData;
}
