using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영구 데이터 관리 (PlayerPrefs 사용)
/// </summary>
public class PersistentDataManager : MonoBehaviour
{
    public static PersistentDataManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool resetDataOnStart = false;

    // 재화
    public int souls = 0;
     
    // 업그레이드 레벨 (UpgradeType을 키로 사용)
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();

    // 업그레이드 데이터 (Inspector에서 할당)
    [Header("Upgrade References")]
    [SerializeField] private UpgradeData[] allUpgrades;
    private Dictionary<UpgradeType, UpgradeData> upgradeDataDict = new Dictionary<UpgradeType, UpgradeData>();

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 업그레이드 데이터 딕셔너리 초기화
            InitializeUpgradeData();

            // 데이터 로드
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
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeUpgradeData()
    {
        upgradeDataDict.Clear();

        if (allUpgrades == null || allUpgrades.Length == 0)
        {
            Debug.LogWarning("[PERSISTENT] No upgrade data assigned!");
            return;
        }

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null)
            {
                upgradeDataDict[upgrade.upgradeType] = upgrade;
            }
        }

        Debug.Log($"[PERSISTENT] Initialized {upgradeDataDict.Count} upgrade types");
    }

    /// <summary>
    /// 데이터 로드
    /// </summary>
    public void LoadData()
    {
        Debug.Log("[PERSISTENT] ═══ Loading Data ═══");

        // 영혼
        souls = PlayerPrefs.GetInt("Souls", 0);
        Debug.Log($"[PERSISTENT] Souls: {souls}");

        // 업그레이드 레벨
        upgradeLevels.Clear();
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            string key = "Upgrade_" + type.ToString();
            int level = PlayerPrefs.GetInt(key, 0);
            upgradeLevels[type] = level;

            if (level > 0)
            {
                Debug.Log($"[PERSISTENT] {type}: Level {level}");
            }
        }

        Debug.Log("[PERSISTENT] ═══ Load Complete ═══");
    }

    /// <summary>
    /// 데이터 저장
    /// </summary>
    public void SaveData()
    {
        Debug.Log("[PERSISTENT] ═══ Saving Data ═══");

        // 영혼 저장
        PlayerPrefs.SetInt("Souls", souls);
        Debug.Log($"[PERSISTENT] Saved Souls: {souls}");

        // 업그레이드 레벨 저장
        foreach (var kvp in upgradeLevels)
        {
            string key = "Upgrade_" + kvp.Key.ToString();
            PlayerPrefs.SetInt(key, kvp.Value);

            if (kvp.Value > 0)
            {
                Debug.Log($"[PERSISTENT] Saved {kvp.Key}: Level {kvp.Value}");
            }
        }

        PlayerPrefs.Save();
        Debug.Log("[PERSISTENT] ═══ Save Complete ═══");
    }

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

    /// <summary>
    /// 모든 데이터 초기화
    /// </summary>
    public void ResetAllData()
    {
        Debug.Log("[PERSISTENT] ═══ RESETTING ALL DATA ═══");

        souls = 0;
        upgradeLevels.Clear();

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("[PERSISTENT] All data has been reset");
    }

    /// <summary>
    /// 디버그용: 영혼 추가
    /// </summary>
    [ContextMenu("Add 1000 Souls")]
    public void AddSoulsDebug()
    {
        souls += 1000;
        SaveData();
        Debug.Log($"[DEBUG] Added 1000 souls. Total: {souls}");
    }

    /// <summary>
    /// 디버그용: 데이터 출력
    /// </summary>
    [ContextMenu("Print All Data")]
    public void PrintAllData()
    {
        Debug.Log("═══ PERSISTENT DATA ═══");
        Debug.Log($"Souls: {souls}");

        foreach (var kvp in upgradeLevels)
        {
            if (kvp.Value > 0)
            {
                int totalValue = GetTotalUpgradeValue(kvp.Key);
                Debug.Log($"{kvp.Key}: Level {kvp.Value} (Total: +{totalValue})");
            }
        }
    }
}

