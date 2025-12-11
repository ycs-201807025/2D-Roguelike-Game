using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 영구 강화 시스템 관리
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Transform buttonContainer; // ScrollView의 Content
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private TextMeshProUGUI soulsText;
    [SerializeField] private Button closeButton;

    [Header("Upgrade Data")]
    [SerializeField] private UpgradeData[] allUpgrades;

    private PersistentDataManager dataManager;

    void Start()
    {
        dataManager = PersistentDataManager.Instance;

        if (dataManager == null)
        {
            Debug.LogError("PersistentDataManager not found!");
            return;
        }

        SetupUI();

        // 처음엔 패널 비활성화
        upgradePanel.SetActive(false);
    }

    void SetupUI()
    {
        // 기존 버튼들 모두 삭제 (재생성)
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 모든 업그레이드에 대한 버튼 생성
        foreach (var upgradeData in allUpgrades)
        {
            CreateUpgradeButton(upgradeData);
        }

        // 닫기 버튼 이벤트
        closeButton.onClick.AddListener(ClosePanel);

        // 초기 영혼 개수 업데이트
        UpdateSoulsDisplay();
    }

    void CreateUpgradeButton(UpgradeData data)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, buttonContainer);
        UpgradeButton button = buttonObj.GetComponent<UpgradeButton>();

        if (button != null)
        {
            button.Initialize(data, this);
        }
    }

    /// <summary>
    /// 업그레이드 구매 시도
    /// </summary>
    public bool TryPurchaseUpgrade(UpgradeData data)
    {
        int currentLevel = dataManager.GetUpgradeLevel(data.upgradeType);

        // 최대 레벨 체크
        if (currentLevel >= data.maxLevel)
        {
            Debug.Log($"[UPGRADE] {data.upgradeName} is already max level!");
            return false;
        }

        int cost = data.costs[currentLevel];

        // 영혼 부족 체크
        if (dataManager.souls < cost)
        {
            Debug.Log($"[UPGRADE] Not enough souls! Need: {cost}, Have: {dataManager.souls}");
            return false;
        }

        // 구매 성공
        dataManager.souls -= cost;
        dataManager.SetUpgradeLevel(data.upgradeType, currentLevel + 1);
        dataManager.SaveData();

        Debug.Log($"[UPGRADE] Purchased {data.upgradeName} Lv.{currentLevel + 1} for {cost} souls");

        // UI 업데이트
        UpdateAllButtons();
        UpdateSoulsDisplay();

        // MVP UI 업데이트 (게임 중이라면)
        //if (PlayerStats.Instance != null)
        //{
        //    PlayerStats.Instance.UpdateFromPersistentData();
        //}
         
        return true;
    }

    void UpdateAllButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            UpgradeButton button = child.GetComponent<UpgradeButton>();
            if (button != null)
            {
                button.UpdateDisplay();
            }
        }
    }

    void UpdateSoulsDisplay()
    {
        soulsText.text = $"보유 영혼: {dataManager.souls}";
    }

    /// <summary>
    /// 패널 열기
    /// </summary>
    public void OpenPanel()
    {
        upgradePanel.SetActive(true);
        UpdateAllButtons();
        UpdateSoulsDisplay();
        Time.timeScale = 0f; // 게임 일시정지
    }

    /// <summary>
    /// 패널 닫기
    /// </summary>
    public void ClosePanel()
    {
        upgradePanel.SetActive(false);
        Time.timeScale = 1f; // 게임 재개
    }

    void Update()
    {
        // U키로 강화 패널 토글 (테스트용)
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (upgradePanel.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }
    }
}
