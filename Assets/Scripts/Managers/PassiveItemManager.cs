using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패시브 아이템 관리 싱글톤
/// </summary>
public class PassiveItemManager : MonoBehaviour
{
    public static PassiveItemManager Instance { get; private set; }

    [Header("Owned Items")]
    private List<PassiveItemData> ownedItems = new List<PassiveItemData>();

    // 이벤트
    public delegate void OnItemAcquired(PassiveItemData item);
    public event OnItemAcquired onItemAcquired;

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

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(PassiveItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("[PASSIVE MANAGER] Cannot add null item");
            return;
        }

        ownedItems.Add(item);
        onItemAcquired?.Invoke(item);

        Debug.Log($"[PASSIVE MANAGER] Acquired: {item.itemName} ({item.itemType})");
        Debug.Log($"[PASSIVE MANAGER] Total items: {ownedItems.Count}");
    }

    /// <summary>
    /// 보유 아이템 리스트 반환
    /// </summary>
    public List<PassiveItemData> GetOwnedItems()
    {
        return new List<PassiveItemData>(ownedItems);
    }

    /// <summary>
    /// 특정 타입 개수 세기
    /// </summary>
    public int CountItemsByType(PassiveItemType type)
    {
        int count = 0;
        foreach (var item in ownedItems)
        {
            if (item.itemType == type)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 던전 시작 시 초기화 (선택사항)
    /// </summary>
    public void ClearItems()
    {
        ownedItems.Clear();
        Debug.Log("[PASSIVE MANAGER] All items cleared");
    }
}
