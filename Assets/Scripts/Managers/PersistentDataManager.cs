using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영구 데이터 저장/로드 관리
/// PlayerPrefs 사용
/// </summary>
public class PersistentDataManager : MonoBehaviour
{
    private static PersistentDataManager instance;
    public static PersistentDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("PersistentDataManager");
                instance = go.AddComponent<PersistentDataManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // 강화 레벨 저장용
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();

    // 영혼 개수
    private int totalSouls = 0;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllData();
    }

    /// <summary>
    /// 모든 데이터 로드
    /// </summary>
    private void LoadAllData()
    {
        Debug.Log("[PERSISTENT] Loading all data...");

        // 강화 레벨 로드
        upgradeLevels.Clear();
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            string key = $"Upgrade_{type}";
            int level = PlayerPrefs.GetInt(key, 0);
            upgradeLevels[type] = level;

            Debug.Log($"[PERSISTENT] {type}: Level {level}");
        }

        // 영혼 로드
        totalSouls = PlayerPrefs.GetInt("TotalSouls", 0);
        Debug.Log($"[PERSISTENT] Total Souls: {totalSouls}");
    }

    /// <summary>
    /// 모든 데이터 저장
    /// </summary>
    public void SaveAllData()
    {
        Debug.Log("[PERSISTENT] Saving all data...");

        // 강화 레벨 저장
        foreach (var kvp in upgradeLevels)
        {
            string key = $"Upgrade_{kvp.Key}";
            PlayerPrefs.SetInt(key, kvp.Value);
        }

        // 영혼 저장
        PlayerPrefs.SetInt("TotalSouls", totalSouls);

        PlayerPrefs.Save();
        Debug.Log("[PERSISTENT] Data saved!");
    }

    /// <summary>
    /// 강화 레벨 가져오기
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
    /// 강화 레벨 설정
    /// </summary>
    public void SetUpgradeLevel(UpgradeType type, int level)
    {
        upgradeLevels[type] = level;
        SaveAllData();

        Debug.Log($"[PERSISTENT] {type} upgraded to level {level}");
    }

    /// <summary>
    /// 영혼 가져오기
    /// </summary>
    public int GetSouls()
    {
        return totalSouls;
    }

    /// <summary>
    /// 영혼 추가
    /// </summary>
    public void AddSouls(int amount)
    {
        totalSouls += amount;
        SaveAllData();

        Debug.Log($"[PERSISTENT] Added {amount} souls. Total: {totalSouls}");
    }

    /// <summary>
    /// 영혼 소비
    /// </summary>
    public bool SpendSouls(int amount)
    {
        if (totalSouls >= amount)
        {
            totalSouls -= amount;
            SaveAllData();

            Debug.Log($"[PERSISTENT] Spent {amount} souls. Remaining: {totalSouls}");
            return true;
        }

        Debug.LogWarning($"[PERSISTENT] Not enough souls! Have: {totalSouls}, Need: {amount}");
        return false;
    }

    /// <summary>
    /// 모든 데이터 초기화 (테스트용)
    /// </summary>
    public void ResetAllData()
    {
        Debug.Log("[PERSISTENT] Resetting all data!");

        PlayerPrefs.DeleteAll();
        upgradeLevels.Clear();
        totalSouls = 0;

        LoadAllData();
    }

    /// <summary>
    /// 특정 강화의 총 효과값 계산
    /// </summary>
    public int GetTotalUpgradeValue(UpgradeData upgradeData)
    {
        if (upgradeData == null) return 0;

        int level = GetUpgradeLevel(upgradeData.upgradeType);
        int totalValue = 0;

        for (int i = 0; i < level; i++)
        {
            totalValue += upgradeData.GetValueForLevel(i);
        }

        return totalValue;
    }
}
