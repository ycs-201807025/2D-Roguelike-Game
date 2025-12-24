using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영구 데이터 관리 (PlayerPrefs 사용)
/// </summary>
public class PersistentDataManager : MonoBehaviour
{
    #region Singleton
    public static PersistentDataManager Instance { get; private set; }
    #endregion

    #region Constants
    private const string SOULS_KEY = "Souls";
    private const string UPGRADE_KEY_PREFIX = "Upgrade_";
    private const int DEBUG_SOULS_AMOUNT = 1000;
    #endregion

    #region Serialized Fields
    [Header("Debug")]
    [SerializeField] private bool resetDataOnStart = false;

    [Header("Upgrade References")]
    [SerializeField] private UpgradeData[] allUpgrades;
    #endregion

    #region State
    public int souls = 0;
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();
    private Dictionary<UpgradeType, UpgradeData> upgradeDataDict = new Dictionary<UpgradeType, UpgradeData>();
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeSingleton();
        InitializeUpgradeData();
        LoadOrResetData();
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
    /// 업그레이드 데이터 초기화
    /// </summary>
    private void InitializeUpgradeData()
    {
        upgradeDataDict.Clear();

        if (!ValidateUpgradeData())
        {
            return;
        }

        BuildUpgradeDictionary();

        Debug.Log($"[PERSISTENT] Initialized {upgradeDataDict.Count} upgrade types");
    }

    /// <summary>
    /// 업그레이드 데이터 유효성 검사
    /// </summary>
    private bool ValidateUpgradeData()
    {
        if (allUpgrades == null || allUpgrades.Length == 0)
        {
            Debug.LogWarning("[PERSISTENT] No upgrade data assigned!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 업그레이드 딕셔너리 구성
    /// </summary>
    private void BuildUpgradeDictionary()
    {
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null)
            {
                upgradeDataDict[upgrade.upgradeType] = upgrade;
            }
        }
    }

    /// <summary>
    /// 데이터 로드 또는 리셋
    /// </summary>
    private void LoadOrResetData()
    {
        if (resetDataOnStart)
        {
            Debug.Log("[PERSISTENT] Resetting all data...");
            ResetAllData();
        }
        else
        {
            LoadData();
        }
    }
    #endregion

    #region Data Loading
    /// <summary>
    /// 데이터 로드
    /// </summary>
    public void LoadData()
    {
        Debug.Log("[PERSISTENT] ═══ Loading Data ═══");

        LoadSouls();
        LoadUpgradeLevels();

        Debug.Log("[PERSISTENT] ═══ Load Complete ═══");
    }

    /// <summary>
    /// 영혼 로드
    /// </summary>
    private void LoadSouls()
    {
        souls = PlayerPrefs.GetInt(SOULS_KEY, 0);
        Debug.Log($"[PERSISTENT] Souls: {souls}");
    }

    /// <summary>
    /// 업그레이드 레벨 로드
    /// </summary>
    private void LoadUpgradeLevels()
    {
        upgradeLevels.Clear();

        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            string key = BuildUpgradeKey(type);
            int level = PlayerPrefs.GetInt(key, 0);
            upgradeLevels[type] = level;

            if (level > 0)
            {
                Debug.Log($"[PERSISTENT] {type}: Level {level}");
            }
        }
    }

    /// <summary>
    /// 업그레이드 키 생성
    /// </summary>
    private string BuildUpgradeKey(UpgradeType type)
    {
        return UPGRADE_KEY_PREFIX + type.ToString();
    }
    #endregion

    #region Data Saving
    /// <summary>
    /// 데이터 저장
    /// </summary>
    public void SaveData()
    {
        Debug.Log("[PERSISTENT] ═══ Saving Data ═══");

        SaveSouls();
        SaveUpgradeLevels();
        CommitSave();

        Debug.Log("[PERSISTENT] ═══ Save Complete ═══");
    }

    /// <summary>
    /// 영혼 저장
    /// </summary>
    private void SaveSouls()
    {
        PlayerPrefs.SetInt(SOULS_KEY, souls);
        Debug.Log($"[PERSISTENT] Saved Souls: {souls}");
    }

    /// <summary>
    /// 업그레이드 레벨 저장
    /// </summary>
    private void SaveUpgradeLevels()
    {
        foreach (var kvp in upgradeLevels)
        {
            string key = BuildUpgradeKey(kvp.Key);
            PlayerPrefs.SetInt(key, kvp.Value);

            if (kvp.Value > 0)
            {
                Debug.Log($"[PERSISTENT] Saved {kvp.Key}: Level {kvp.Value}");
            }
        }
    }

    /// <summary>
    /// 저장 커밋
    /// </summary>
    private void CommitSave()
    {
        PlayerPrefs.Save();
    }
    #endregion

    #region Upgrade Management
    /// <summary>
    /// 특정 업그레이드의 현재 레벨
    /// </summary>
    public int GetUpgradeLevel(UpgradeType type)
    {
        if (upgradeLevels.ContainsKey(type))
        {
            return upgradeLevels[type];
        }
        return 0;
    }

    /// <summary>
    /// 업그레이드 레벨 설정
    /// </summary>
    public void SetUpgradeLevel(UpgradeType type, int level)
    {
        upgradeLevels[type] = level;
        Debug.Log($"[PERSISTENT] Set {type} to level {level}");
    }

    /// <summary>
    /// 특정 업그레이드의 총 증가값
    /// </summary>
    public int GetTotalUpgradeValue(UpgradeType type)
    {
        if (!upgradeDataDict.ContainsKey(type))
        {
            Debug.LogWarning($"[PERSISTENT] No upgrade data for {type}");
            return 0;
        }

        int currentLevel = GetUpgradeLevel(type);
        return upgradeDataDict[type].GetTotalValue(currentLevel);
    }

    /// <summary>
    /// 업그레이드 데이터 가져오기
    /// </summary>
    public UpgradeData GetUpgradeData(UpgradeType type)
    {
        if (upgradeDataDict.ContainsKey(type))
        {
            return upgradeDataDict[type];
        }
        return null;
    }
    #endregion

    #region Data Reset
    /// <summary>
    /// 모든 데이터 초기화
    /// </summary>
    public void ResetAllData()
    {
        Debug.Log("[PERSISTENT] ═══ RESETTING ALL DATA ═══");

        ClearRuntimeData();
        ClearPlayerPrefs();

        Debug.Log("[PERSISTENT] All data has been reset");
    }

    /// <summary>
    /// 런타임 데이터 클리어
    /// </summary>
    private void ClearRuntimeData()
    {
        souls = 0;
        upgradeLevels.Clear();
    }

    /// <summary>
    /// PlayerPrefs 클리어
    /// </summary>
    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
    #endregion

    #region Debug Methods
    /// <summary>
    /// 디버그: 영혼 추가
    /// </summary>
    [ContextMenu("Add 1000 Souls")]
    public void AddSoulsDebug()
    {
        souls += DEBUG_SOULS_AMOUNT;
        SaveData();
        Debug.Log($"[DEBUG] Added {DEBUG_SOULS_AMOUNT} souls. Total: {souls}");
    }

    /// <summary>
    /// 디버그: 데이터 출력
    /// </summary>
    [ContextMenu("Print All Data")]
    public void PrintAllData()
    {
        Debug.Log("═══ PERSISTENT DATA ═══");
        Debug.Log($"Souls: {souls}");

        PrintUpgradeLevels();
    }

    /// <summary>
    /// 업그레이드 레벨 출력
    /// </summary>
    private void PrintUpgradeLevels()
    {
        foreach (var kvp in upgradeLevels)
        {
            if (kvp.Value > 0)
            {
                int totalValue = GetTotalUpgradeValue(kvp.Key);
                Debug.Log($"{kvp.Key}: Level {kvp.Value} (Total: +{totalValue})");
            }
        }
    }
    #endregion
}

