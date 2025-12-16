using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Passive Item", menuName = "Game/Passive Item")]
public class PassiveItem : ScriptableObject
{
    public string itemName;             // 아이템 이름
    public PassiveItemType itemType;    // 아이템 타입
    public Sprite icon;                 // 아이템 아이콘
    [TextArea]
    public string description;          // 설명
}
