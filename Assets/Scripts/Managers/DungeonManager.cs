using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 생성 및 관리
/// 4일차: 간단한 선형 던전
/// </summary>
public class DungeonManager : MonoBehaviour
{
    #region Constants
    private const int BOSS_ROOM_INTERVAL = 5;
    private const int EVENT_ROOM_INTERVAL = 3;
    #endregion

    #region Serialized Fields
    [Header("Room Prefabs")]
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private GameObject[] normalRoomPrefabs;
    [SerializeField] private GameObject[] eventRoomPrefabs; 
    [SerializeField] private GameObject[] bossRoomPrefabs;  

    [Header("Dungeon Settings")]
    [SerializeField] private int roomCount = 10; // 총 방 개수
    [SerializeField] private float roomSpacing = 30f; // 방 사이 간격

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Camera")]
    [SerializeField] private CameraRoomBounds cameraRoomBounds;
    #endregion

    #region State
    private List<Room> rooms = new List<Room>();
    private int currentRoomIndex = 0;
    #endregion

    #region Properties
    public Room CurrentRoom => currentRoomIndex >= 0 && currentRoomIndex < rooms.Count ? rooms[currentRoomIndex] : null;
    public int CurrentRoomIndex => currentRoomIndex;
    public int TotalRooms => rooms.Count;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        Debug.Log("=== DUNGEON MANAGER START ===");

        PlayDungeonBGM();
        GenerateDungeon();
        StartDungeon();
    }
    void Update()
    {
        HandleDebugInput();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// BGM 재생
    /// </summary>
    private void PlayDungeonBGM()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDungeonBGM();
        }
    }
    #endregion

    #region Dungeon Generation
    /// <summary>
    /// 던전 생성 (간단한 선형 구조)
    /// </summary>
    private void GenerateDungeon()
    {
        Debug.Log($"Generating dungeon with {roomCount} rooms...");

        rooms.Clear();
        CreateStartRoom();
        CreateNormalRooms();
        DeactivateAllRooms();

        Debug.Log($"=== Dungeon generated: {rooms.Count} rooms ===");
    }

    /// <summary>
    /// 시작방 생성
    /// </summary>
    private void CreateStartRoom()
    {
        CreateRoom(startRoomPrefab, 0, "Start");
    }

    /// <summary>
    /// 일반 방들 생성
    /// </summary>
    private void CreateNormalRooms()
    {
        for (int i = 1; i < roomCount; i++)
        {
            RoomType roomType = DetermineRoomType(i);
            GameObject roomPrefab = SelectRoomPrefab(roomType);

            if (roomPrefab != null)
            {
                CreateRoom(roomPrefab, i, roomType.ToString());
            }
        }
    }

    /// <summary>
    /// 방 타입 결정
    /// </summary>
    private RoomType DetermineRoomType(int index)
    {
        int floor = index + 1;

        if (IsBossFloor(floor))
        {
            return RoomType.Boss;
        }
        else if (IsEventFloor(index))
        {
            return RoomType.Event;
        }
        else
        {
            return RoomType.Normal;
        }
    }

    /// <summary>
    /// 보스 층 확인
    /// </summary>
    private bool IsBossFloor(int floor)
    {
        return floor % BOSS_ROOM_INTERVAL == 0 &&
               bossRoomPrefabs != null &&
               bossRoomPrefabs.Length > 0;
    }

    /// <summary>
    /// 사건방 층 확인
    /// </summary>
    private bool IsEventFloor(int index)
    {
        return index % EVENT_ROOM_INTERVAL == 0 &&
               eventRoomPrefabs != null &&
               eventRoomPrefabs.Length > 0;
    }

    /// <summary>
    /// 방 프리팹 선택
    /// </summary>
    private GameObject SelectRoomPrefab(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Boss:
                return SelectRandomPrefab(bossRoomPrefabs);

            case RoomType.Event:
                return SelectRandomPrefab(eventRoomPrefabs);

            case RoomType.Normal:
                return SelectRandomPrefab(normalRoomPrefabs);

            default:
                Debug.LogError($"[DUNGEON MANAGER] Unknown room type: {roomType}");
                return null;
        }
    }

    /// <summary>
    /// 랜덤 프리팹 선택
    /// </summary>
    private GameObject SelectRandomPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        return prefabs[Random.Range(0, prefabs.Length)];
    }

    /// <summary>
    /// 방 생성
    /// </summary>
    private void CreateRoom(GameObject roomPrefab, int index, string typeLabel)
    {
        if (!ValidateRoomPrefab(roomPrefab, index))
        {
            return;
        }

        Vector3 position = CalculateRoomPosition(index);
        GameObject roomObj = InstantiateRoom(roomPrefab, position);
        ConfigureRoom(roomObj, index, typeLabel);

        Room room = roomObj.GetComponent<Room>();
        if (room != null)
        {
            rooms.Add(room);
            Debug.Log($"[DUNGEON MANAGER] ✓ Room {index} ({typeLabel}) created at {position}");
        }
    }

    /// <summary>
    /// 방 프리팹 유효성 검사
    /// </summary>
    private bool ValidateRoomPrefab(GameObject roomPrefab, int index)
    {
        if (roomPrefab == null)
        {
            Debug.LogError($"[DUNGEON MANAGER] Room prefab is null at index {index}!");
            return false;
        }

        Room room = roomPrefab.GetComponent<Room>();
        if (room == null)
        {
            Debug.LogError($"[DUNGEON MANAGER] Room prefab {roomPrefab.name} doesn't have Room component!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 방 위치 계산
    /// </summary>
    private Vector3 CalculateRoomPosition(int index)
    {
        return new Vector3(index * roomSpacing, 0, 0);
    }

    /// <summary>
    /// 방 인스턴스화
    /// </summary>
    private GameObject InstantiateRoom(GameObject roomPrefab, Vector3 position)
    {
        GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity);
        roomObj.transform.SetParent(transform);
        return roomObj;
    }

    /// <summary>
    /// 방 설정
    /// </summary>
    private void ConfigureRoom(GameObject roomObj, int index, string typeLabel)
    {
        string roomName = index == 0 ? "Room_0_Start" : $"Room_{index}_{typeLabel}";
        roomObj.name = roomName;
    }

    /// <summary>
    /// 모든 방 비활성화
    /// </summary>
    private void DeactivateAllRooms()
    {
        foreach (Room room in rooms)
        {
            room.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Dungeon Progression
    /// <summary>
    /// 던전 시작
    /// </summary>
    private void StartDungeon()
    {
        if (!ValidateDungeon())
        {
            return;
        }

        Debug.Log("Starting dungeon...");

        FindPlayer();
        EnterRoom(0);
    }
    /// <summary>
    /// 던전 유효성 검사
    /// </summary>
    private bool ValidateDungeon()
    {
        if (rooms.Count == 0)
        {
            Debug.LogError("No rooms in dungeon!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 플레이어 찾기
    /// </summary>
    private void FindPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player == null)
        {
            Debug.LogError("Player not found!");
        }
    }

    /// <summary>
    /// 방 입장
    /// </summary>
    public void EnterRoom(int roomIndex)
    {
        if (!ValidateRoomIndex(roomIndex))
        {
            return;
        }

        DeactivateCurrentRoom();
        ActivateNewRoom(roomIndex);
        MovePlayerToRoom();
        UpdateCamera();
        SubscribeToRoomEvents();
        HandleBossBGM();

        Debug.Log($"\n>>> Entering Room {roomIndex} <<<");
        Debug.Log($"Current Room: {CurrentRoom.RoomData.roomName}");
    }
    /// <summary>
    /// 방 인덱스 유효성 검사
    /// </summary>
    private bool ValidateRoomIndex(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= rooms.Count)
        {
            Debug.LogError($"Invalid room index: {roomIndex}");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 현재 방 비활성화
    /// </summary>
    private void DeactivateCurrentRoom()
    {
        if (currentRoomIndex >= 0 && currentRoomIndex < rooms.Count)
        {
            rooms[currentRoomIndex].DeactivateRoom();
        }
    }

    /// <summary>
    /// 새 방 활성화
    /// </summary>
    private void ActivateNewRoom(int roomIndex)
    {
        currentRoomIndex = roomIndex;
        CurrentRoom.ActivateRoom();
    }

    /// <summary>
    /// 플레이어 이동
    /// </summary>
    private void MovePlayerToRoom()
    {
        if (player != null)
        {
            Vector3 spawnPos = CurrentRoom.GetPlayerSpawnPosition();
            player.transform.position = spawnPos;
            Debug.Log($"Player moved to {spawnPos}");
        }
    }

    /// <summary>
    /// 카메라 업데이트
    /// </summary>
    private void UpdateCamera()
    {
        if (cameraRoomBounds != null)
        {
            cameraRoomBounds.SetCurrentRoom(CurrentRoom);
        }
    }

    /// <summary>
    /// 방 이벤트 구독
    /// </summary>
    private void SubscribeToRoomEvents()
    {
        CurrentRoom.OnRoomCleared -= OnCurrentRoomCleared;
        CurrentRoom.OnRoomCleared += OnCurrentRoomCleared;
    }

    /// <summary>
    /// 보스방 BGM 처리
    /// </summary>
    private void HandleBossBGM()
    {
        if (CurrentRoom.RoomData != null &&
            CurrentRoom.RoomData.roomType == RoomType.Boss &&
            SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossBGM();
        }
    }
    /// <summary>
    /// 현재 방 클리어 시
    /// </summary>
    private void OnCurrentRoomCleared()
    {
        Debug.Log(">>> Current room cleared! <<<");

        // 다음 방이 있으면
        if (currentRoomIndex < rooms.Count - 1)
        {
            Debug.Log("→ Next room available. Use portal or press [N]");
        }
        else
        {
            OnDungeonCompleted();
        }
    }
    /// <summary>
    /// 다음 방 존재 여부
    /// </summary>
    private bool HasNextRoom()
    {
        return currentRoomIndex < rooms.Count - 1;
    }
    /// <summary>
    /// 다음 방으로 이동
    /// </summary>
    public void MoveToNextRoom()
    {
        Debug.Log("[MANAGER] MoveToNextRoom called");

        if (!HasNextRoom())
        {
            Debug.Log("마지막 방입니다!");
            return;
        }

        if (CurrentRoom.IsCleared)
        {
            EnterRoom(currentRoomIndex + 1);
        }
        else
        {
            Debug.LogWarning("현재 방을 먼저 클리어하세요!");
        }
    }

    /// <summary>
    /// 이전 방으로 이동
    /// </summary>
    public void MoveToPreviousRoom()
    {
        Debug.Log("[MANAGER] MoveToPreviousRoom called");

        if (currentRoomIndex > 0)
        {
            EnterRoom(currentRoomIndex - 1);
        }
        else
        {
            Debug.Log("첫 번째 방입니다!");
        }
    }

    /// <summary>
    /// 던전 완료
    /// </summary>
    private void OnDungeonCompleted()
    {
        Debug.Log("=== DUNGEON CLEARED! ===");
        Debug.Log("★★★ DUNGEON COMPLETED! ★★★");

        GiveCompletionReward();
    }
    /// <summary>
    /// 완료 보상 지급
    /// </summary>
    private void GiveCompletionReward()
    {
        const int COMPLETION_GOLD = 500;

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddGold(COMPLETION_GOLD);
            Debug.Log($"[DUNGEON MANAGER] Completion reward: {COMPLETION_GOLD} gold");
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// 특정 인덱스의 방 가져오기
    /// </summary>
    private Room GetRoomAtIndex(int index)
    {
        if (index >= 0 && index < rooms.Count)
        {
            return rooms[index];
        }
        return null;
    }
    #endregion

    #region Debug Input
    /// <summary>
    /// 디버그 입력 처리
    /// </summary>
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("[INPUT] N key pressed");
            MoveToNextRoom();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("[INPUT] B key pressed");
            MoveToPreviousRoom();
        }
    }
    #endregion

}