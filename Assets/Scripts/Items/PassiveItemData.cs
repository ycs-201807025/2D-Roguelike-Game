using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패시브 아이템 데이터 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewPassiveItem", menuName = "Game/Passive Item")]
public class PassiveItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName = "패시브 아이템";
    public PassiveItemType itemType;
    public Sprite icon;

    [TextArea(2, 4)]
    public string description = "아이템 설명";

    [Header("Drop Settings")]
    [Range(0f, 1f)]
    public float dropChance = 0.15f; // 15% 드롭 확률
}
