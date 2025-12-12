using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랜덤 사건 선택지
/// </summary>
[System.Serializable]
public class EventOption
{
    public string optionText = "선택지";
    public int goldCost = 0; // 골드 소모 (음수면 획득)
    public int healthCost = 0; // 체력 소모 (음수면 회복)
    public ItemData rewardItem; // 보상 아이템 (선택사항)
}

/// <summary>
/// 랜덤 사건 데이터
/// </summary>
[CreateAssetMenu(fileName = "NewEvent", menuName = "Game/Random Event")]
public class RandomEventData : ScriptableObject
{
    [Header("Event Info")]
    public string eventTitle = "사건";
    [TextArea(3, 6)]
    public string eventDescription = "무언가 일어났습니다...";

    [Header("Options")]
    public EventOption[] options = new EventOption[2];
}
