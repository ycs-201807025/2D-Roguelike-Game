using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방 타입 정의
/// </summary>
public enum RoomType
{
    Normal,      // 일반 전투방
    Start,       // 시작방
    Boss,        // 보스방
    Event,       // 랜덤 사건방
    Shop,        // 상점 (추후)
    Treasure     // 보물방 (추후)
}

/// <summary>
/// 방 데이터
/// </summary>
[CreateAssetMenu(fileName = "NewRoom", menuName = "Game/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Room Info")]
    public string roomName = "Room";
    public RoomType roomType = RoomType.Normal;

    [Header("Layout")]
    public GameObject roomPrefab; // 방 레이아웃 프리팹
    public Vector2Int roomSize = new Vector2Int(20, 15); // 방 크기 (타일 단위)

    [Header("Enemy Spawning")]
    public bool hasEnemies = true;
    public int minEnemies = 3;
    public int maxEnemies = 6;
    public GameObject[] enemyPrefabs; // 생성 가능한 적들

    [Header("Rewards")]
    public int goldReward = 10;
    public bool hasChest = false;

    [Header("Connections")]
    public bool hasNorthDoor = false;
    public bool hasSouthDoor = false;
    public bool hasEastDoor = false;
    public bool hasWestDoor = false;
}
