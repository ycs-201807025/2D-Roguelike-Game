using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveItemManager : MonoBehaviour
{
    public static PassiveItemManager Instance { get; private set; }

    // 보유한 패시브 아이템 리스트
    private List<PassiveItem> ownedItems = new List<PassiveItem>();

    // 이벤트: 아이템 획득 시
    public delegate void OnItemAcquired(PassiveItem item);
    public event OnItemAcquired onItemAcquired;

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

    // 아이템 추가
    public void AddItem(PassiveItem item)
    {
        ownedItems.Add(item);
        onItemAcquired?.Invoke(item);
        Debug.Log($"획득: {item.itemName}");
    }

    // 보유 아이템 리스트 반환
    public List<PassiveItem> GetOwnedItems()
    {
        return new List<PassiveItem>(ownedItems);
    }

    // 특정 타입 개수 세기
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
}
