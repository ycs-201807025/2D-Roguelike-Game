using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 업그레이드 버튼 UI
/// </summary>
public class UpgradeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button button;

    [Header("Colors")]
    [SerializeField] private Color affordableColor = new Color(0.4f, 0.8f, 0.4f); // 구매 가능 (녹색)
    [SerializeField] private Color notAffordableColor = new Color(0.5f, 0.5f, 0.5f); // 구매 불가 (회색)
    [SerializeField] private Color maxLevelColor = new Color(1f, 0.84f, 0f); // 최대 레벨 (금색)

    private UpgradeData upgradeData;
    private UpgradeManager upgradeManager;

    void Awake()
    {
        // Button 컴포넌트 자동 할당
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        // 버튼 클릭 이벤트
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(UpgradeData data, UpgradeManager manager)
    {
        upgradeData = data;
        upgradeManager = manager;

        if (upgradeData == null)
        {
            Debug.LogError("[UPGRADE BUTTON] UpgradeData is null!");
            return;
        }

        // 기본 정보 설정 (변하지 않는 것들)
        if (nameText != null)
        {
            nameText.text = upgradeData.upgradeName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = upgradeData.description;
        }

        // 동적 정보 업데이트
        UpdateDisplay();

        Debug.Log($"[UPGRADE BUTTON] Initialized: {upgradeData.upgradeName}");
    }

    /// <summary>
    /// 표시 업데이트 (레벨, 비용, 색상)
    /// </summary>
    public void UpdateDisplay()
    {
        if (upgradeData == null)
        {
            Debug.LogWarning("[UPGRADE BUTTON] Cannot update - no upgrade data");
            return;
        }

        // 현재 레벨 가져오기
        int currentLevel = 0;
        int currentSouls = 0;

        if (PersistentDataManager.Instance != null)
        {
            currentLevel = PersistentDataManager.Instance.GetUpgradeLevel(upgradeData.upgradeType);
            currentSouls = PersistentDataManager.Instance.souls;
        }

        // 레벨 텍스트
        if (levelText != null)
        {
            levelText.text = $"Lv. {currentLevel} / {upgradeData.maxLevel}";
        }

        // 최대 레벨 체크
        bool isMaxLevel = currentLevel >= upgradeData.maxLevel;

        if (isMaxLevel)
        {
            // 최대 레벨 도달
            if (costText != null)
            {
                costText.text = "MAX";
                costText.color = maxLevelColor;
            }

            if (button != null)
            {
                button.interactable = false;
                button.GetComponent<Image>().color = maxLevelColor;
            }
        }
        else
        {
            // 아직 업그레이드 가능
            int nextLevelCost = upgradeData.GetCost(currentLevel);

            if (costText != null)
            {
                costText.text = $"{nextLevelCost} 영혼";
            }

            // 구매 가능 여부에 따라 색상 변경
            bool canAfford = currentSouls >= nextLevelCost;

            if (button != null)
            {
                button.interactable = canAfford;

                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = canAfford ? affordableColor : notAffordableColor;
                }
            }

            if (costText != null)
            {
                costText.color = canAfford ? affordableColor : notAffordableColor;
            }
        }
    }

    /// <summary>
    /// 버튼 클릭 시
    /// </summary>
    void OnButtonClick()
    {
        if (upgradeData == null || upgradeManager == null)
        {
            Debug.LogError("[UPGRADE BUTTON] Cannot purchase - missing references");
            return;
        }

        Debug.Log($"[UPGRADE BUTTON] Attempting to purchase: {upgradeData.upgradeName}");

        // 구매 시도
        bool success = upgradeManager.TryPurchaseUpgrade(upgradeData);

        if (success)
        {
            Debug.Log($"[UPGRADE BUTTON] ✓ Purchase successful!");
        }
        else
        {
            Debug.Log($"[UPGRADE BUTTON] ✗ Purchase failed");
        }
    }

    void OnDestroy()
    {
        // 버튼 이벤트 제거
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
