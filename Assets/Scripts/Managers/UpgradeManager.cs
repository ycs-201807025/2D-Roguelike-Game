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
    #region Constants
    private const float PAUSED_TIME_SCALE = 0f;
    private const float NORMAL_TIME_SCALE = 1f;
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Transform buttonContainer; // ScrollView의 Content
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private TextMeshProUGUI soulsText;
    [SerializeField] private Button closeButton;

    [Header("Upgrade Data")]
    [SerializeField] private UpgradeData[] allUpgrades;
    #endregion

    #region Components
    private PersistentDataManager dataManager;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeComponents();
        SetupUI();
        HidePanel();
    }

    void Update()
    {
        HandleToggleInput();
    }
    void OnDestroy()
    {
        EnsureTimeScaleRestored();
    }
    void OnDisable()
    {
        EnsureTimeScaleRestored();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        dataManager = PersistentDataManager.Instance;

        if (dataManager == null)
        {
            Debug.LogError("[UPGRADE MANAGER] PersistentDataManager not found!");
        }
    }

    /// <summary>
    /// UI 설정
    /// </summary>
    private void SetupUI()
    {
        ClearExistingButtons();
        CreateUpgradeButtons();
        SetupCloseButton();
        UpdateSoulsDisplay();
    }

    /// <summary>
    /// 기존 버튼 삭제
    /// </summary>
    private void ClearExistingButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 업그레이드 버튼 생성
    /// </summary>
    private void CreateUpgradeButtons()
    {
        foreach (var upgradeData in allUpgrades)
        {
            CreateUpgradeButton(upgradeData);
        }
    }

    /// <summary>
    /// 개별 업그레이드 버튼 생성
    /// </summary>
    private void CreateUpgradeButton(UpgradeData data)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, buttonContainer);

        InitializeUpgradeButton(buttonObj, data);
    }

    /// <summary>
    /// 업그레이드 버튼 초기화
    /// </summary>
    private void InitializeUpgradeButton(GameObject buttonObj, UpgradeData data)
    {
        UpgradeButton button = buttonObj.GetComponent<UpgradeButton>();

        if (button != null)
        {
            button.Initialize(data, this);
        }
    }

    /// <summary>
    /// 닫기 버튼 설정
    /// </summary>
    private void SetupCloseButton()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    /// <summary>
    /// 패널 초기 숨김
    /// </summary>
    private void HidePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
    #endregion

    #region Purchase System
    /// <summary>
    /// 업그레이드 구매 시도
    /// </summary>
    public bool TryPurchaseUpgrade(UpgradeData data)
    {
        if (!CanPurchase(data, out int cost, out int currentLevel))
        {
            return false;
        }

        ExecutePurchase(data, cost, currentLevel);
        UpdateUI();
        ApplyUpgradeToCurrentGame();

        return true;
    }

    /// <summary>
    /// 구매 가능 여부 확인
    /// </summary>
    private bool CanPurchase(UpgradeData data, out int cost, out int currentLevel)
    {
        currentLevel = dataManager.GetUpgradeLevel(data.upgradeType);
        cost = 0;

        if (!CheckMaxLevel(data, currentLevel))
        {
            return false;
        }

        cost = data.costs[currentLevel];

        if (!CheckSoulBalance(cost))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 최대 레벨 확인
    /// </summary>
    private bool CheckMaxLevel(UpgradeData data, int currentLevel)
    {
        if (currentLevel >= data.maxLevel)
        {
            Debug.Log($"[UPGRADE] {data.upgradeName} is already max level!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 영혼 잔액 확인
    /// </summary>
    private bool CheckSoulBalance(int cost)
    {
        if (dataManager.souls < cost)
        {
            Debug.Log($"[UPGRADE] Not enough souls! Need: {cost}, Have: {dataManager.souls}");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 구매 실행
    /// </summary>
    private void ExecutePurchase(UpgradeData data, int cost, int currentLevel)
    {
        DeductSouls(cost);
        IncrementUpgradeLevel(data, currentLevel);
        SavePurchase();

        Debug.Log($"[UPGRADE] ✓ Purchased {data.upgradeName} Lv.{currentLevel + 1} for {cost} souls");
    }

    /// <summary>
    /// 영혼 차감
    /// </summary>
    private void DeductSouls(int cost)
    {
        dataManager.souls -= cost;
    }

    /// <summary>
    /// 업그레이드 레벨 증가
    /// </summary>
    private void IncrementUpgradeLevel(UpgradeData data, int currentLevel)
    {
        dataManager.SetUpgradeLevel(data.upgradeType, currentLevel + 1);
    }

    /// <summary>
    /// 구매 저장
    /// </summary>
    private void SavePurchase()
    {
        dataManager.SaveData();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        UpdateAllButtons();
        UpdateSoulsDisplay();
    }

    /// <summary>
    /// 모든 버튼 업데이트
    /// </summary>
    private void UpdateAllButtons()
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

    /// <summary>
    /// 현재 게임에 업그레이드 적용
    /// </summary>
    private void ApplyUpgradeToCurrentGame()
    {
        if (PlayerStats.Instance == null)
        {
            return;
        }

        Debug.Log("[UPGRADE] Applying upgrade to current game...");

        PlayerStats.Instance.UpdateFromPersistentData();
        AdjustCurrentHealth();
    }

    /// <summary>
    /// 현재 체력 조정 (체력 비율 유지)
    /// </summary>
    private void AdjustCurrentHealth()
    {
        float healthRatio = CalculateHealthRatio();
        int newCurrentHealth = CalculateNewHealth(healthRatio);

        PlayerStats.Instance.SetHealth(newCurrentHealth, PlayerStats.Instance.MaxHealth);
    }

    /// <summary>
    /// 체력 비율 계산
    /// </summary>
    private float CalculateHealthRatio()
    {
        return (float)PlayerStats.Instance.CurrentHealth / PlayerStats.Instance.MaxHealth;
    }

    /// <summary>
    /// 새로운 체력 계산
    /// </summary>
    private int CalculateNewHealth(float healthRatio)
    {
        return Mathf.RoundToInt(PlayerStats.Instance.MaxHealth * healthRatio);
    }
    #endregion

    #region UI Management
    /// <summary>
    /// 영혼 표시 업데이트
    /// </summary>
    private void UpdateSoulsDisplay()
    {
        if (!ValidateSoulsText())
        {
            return;
        }

        if (dataManager == null)
        {
            soulsText.text = "보유 영혼: 0";
            return;
        }

        soulsText.text = $"보유 영혼: {dataManager.souls}";
    }

    /// <summary>
    /// 영혼 텍스트 유효성 검사
    /// </summary>
    private bool ValidateSoulsText()
    {
        if (soulsText == null)
        {
            Debug.LogWarning("[UPGRADE MANAGER] soulsText is null! Please assign in Inspector.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 패널 열기
    /// </summary>
    public void OpenPanel()
    {
        if (!ValidateUpgradePanel())
        {
            return;
        }

        upgradePanel.SetActive(true);
        UpdateAllButtons();
        UpdateSoulsDisplay();
        PauseGame();
    }

    /// <summary>
    /// 업그레이드 패널 유효성 검사
    /// </summary>
    private bool ValidateUpgradePanel()
    {
        if (upgradePanel == null)
        {
            Debug.LogError("[UPGRADE MANAGER] upgradePanel is null!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    private void PauseGame()
    {
        Time.timeScale = PAUSED_TIME_SCALE;
    }

    /// <summary>
    /// 패널 닫기
    /// </summary>
    public void ClosePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        ResumeGame();
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    private void ResumeGame()
    {
        Time.timeScale = NORMAL_TIME_SCALE;
    }
    /// <summary>
    /// timeScale이 정상으로 복원되었는지 확인
    /// </summary>
    private void EnsureTimeScaleRestored()
    {
        if (Time.timeScale != NORMAL_TIME_SCALE)
        {
            Time.timeScale = NORMAL_TIME_SCALE;
            Debug.Log("[UPGRADE MANAGER] Restored Time.timeScale to 1 on cleanup");
        }
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// 토글 입력 처리
    /// </summary>
    private void HandleToggleInput()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            TogglePanel();
        }
    }

    /// <summary>
    /// 패널 토글
    /// </summary>
    private void TogglePanel()
    {
        if (upgradePanel == null)
        {
            return;
        }

        if (upgradePanel.activeSelf)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }
    #endregion
}
