using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 생성 및 관리
/// 4일차: 간단한 선형 던전
/// </summary>
public class DungeonManager : MonoBehaviour
{
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

    private List<Room> rooms = new List<Room>();
    private int currentRoomIndex = 0;

    void Start()
    {
        Debug.Log("=== DUNGEON MANAGER START ===");
        // ★★★ 던전 BGM 재생 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDungeonBGM();
        }
        GenerateDungeon();
        StartDungeon();
    }

    /// <summary>
    /// 던전 생성 (간단한 선형 구조)
    /// </summary>
    private void GenerateDungeon()
    {
        Debug.Log($"Generating dungeon with {roomCount} rooms...");

        rooms.Clear();

        // 시작방
        CreateRoom(startRoomPrefab, 0, "Start");

        // 일반 방들 (선형으로 배치)
        for (int i = 1; i < roomCount; i++)
        {
            GameObject roomPrefab = null;
            string roomTypeLabel = "";

            // 5층마다 보스방
            if ((i + 1) % 5 == 0 && bossRoomPrefabs != null && bossRoomPrefabs.Length > 0)
            {
                roomPrefab = bossRoomPrefabs[Random.Range(0, bossRoomPrefabs.Length)];
                roomTypeLabel = "Boss";
                Debug.Log($"  → Floor {i + 1}: Boss Room");
            }
            // 3층마다 사건방 (보스방 아닌 경우)
            else if (i % 3 == 0 && eventRoomPrefabs != null && eventRoomPrefabs.Length > 0)
            {
                roomPrefab = eventRoomPrefabs[Random.Range(0, eventRoomPrefabs.Length)];
                roomTypeLabel = "Event";
                Debug.Log($"  → Floor {i + 1}: Event Room");
            }
            // 나머지는 일반 전투방
            else
            {
                if (normalRoomPrefabs == null || normalRoomPrefabs.Length == 0)
                {
                    Debug.LogError("[DUNGEON MANAGER] No normal room prefabs assigned!");
                    continue;
                }
                roomPrefab = normalRoomPrefabs[Random.Range(0, normalRoomPrefabs.Length)];
                roomTypeLabel = "Normal";
            }

            if (roomPrefab != null)
            {
                CreateRoom(roomPrefab, i, roomTypeLabel);
            }
        }

        // 모든 방 비활성화
        foreach (Room room in rooms)
        {
            room.gameObject.SetActive(false);
        }

        Debug.Log($"=== Dungeon generated: {rooms.Count} rooms ===");
    }

    
    /// <summary>
    /// 방 생성 헬퍼 메서드
    /// </summary>
    private void CreateRoom(GameObject roomPrefab, int index, string typeLabel)
    {
        if (roomPrefab == null)
        {
            Debug.LogError($"[DUNGEON MANAGER] Room prefab is null at index {index}!");
            return;
        }

        // 위치 계산 (오른쪽으로 배치)
        Vector3 position = new Vector3(index * roomSpacing, 0, 0);

        // 방 생성
        GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity);
        roomObj.transform.SetParent(transform);

        // 이름 설정
        string roomName = index == 0 ? "Room_0_Start" : $"Room_{index}_{typeLabel}";
        roomObj.name = roomName;

        // Room 컴포넌트 확인
        Room room = roomObj.GetComponent<Room>();
        if (room == null)
        {
            Debug.LogError($"[DUNGEON MANAGER] Room prefab {roomPrefab.name} doesn't have Room component!");
            Destroy(roomObj);
            return;
        }

        rooms.Add(room);
        Debug.Log($"[DUNGEON MANAGER] ✓ Room {index} ({typeLabel}) created at {position}");
    }
    /// <summary>
    /// 던전 시작
    /// </summary>
    private void StartDungeon()
    {
        if (rooms.Count == 0)
        {
            Debug.LogError("No rooms in dungeon!");
            return;
        }

        Debug.Log("Starting dungeon...");

        // 플레이어 찾기
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player == null)
        {
            Debug.LogError("Player not found!");
        }

        // 첫 방 활성화
        currentRoomIndex = 0;
        EnterRoom(0);
    }
    /// <summary>
    /// 방 입장
    /// </summary>
    public void EnterRoom(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= rooms.Count)
        {
            Debug.LogError($"Invalid room index: {roomIndex}");
            return;
        }

        Debug.Log($"\n>>> Entering Room {roomIndex} <<<");
        // ★★★ 보스방이면 BGM 변경 ★★★
        Room currentRoom = rooms[currentRoomIndex];
        if (currentRoom.RoomData != null &&
            currentRoom.RoomData.roomType == RoomType.Boss &&
            SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossBGM();
        }
        // 이전 방 비활성화
        if (currentRoomIndex >= 0 && currentRoomIndex < rooms.Count)
        {
            rooms[currentRoomIndex].DeactivateRoom();
        }

        // 새 방 활성화
        currentRoomIndex = roomIndex;
        
        currentRoom.ActivateRoom();

        // 플레이어 이동
        if (player != null)
        {
            Vector3 spawnPos = currentRoom.GetPlayerSpawnPosition();
            player.transform.position = spawnPos;
            Debug.Log($"Player moved to {spawnPos}");
        }

        // 카메라 경계 설정
        if (cameraRoomBounds != null)
        {
            cameraRoomBounds.SetCurrentRoom(currentRoom);
        }

        // 방 클리어 이벤트 구독
        currentRoom.OnRoomCleared -= OnCurrentRoomCleared; // 기존 구독 제거
        currentRoom.OnRoomCleared += OnCurrentRoomCleared; // 새로 구독

        Debug.Log($"Current Room: {currentRoom.RoomData.roomName}");
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
            Debug.Log("★★★ DUNGEON COMPLETED! ★★★");
            OnDungeonCompleted();
        }
    }

    /// <summary>
    /// 다음 방으로 이동
    /// </summary>
    public void MoveToNextRoom()
    {
        Debug.Log("[MANAGER] MoveToNextRoom called");

        if (currentRoomIndex < rooms.Count - 1)
        {
            // 현재 방이 클리어되었는지 확인
            if (rooms[currentRoomIndex].IsCleared)
            {
                EnterRoom(currentRoomIndex + 1);
            }
            else
            {
                Debug.LogWarning("현재 방을 먼저 클리어하세요!");
            }
        }
        else
        {
            Debug.Log("마지막 방입니다!");
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
        // 보상 지급
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddGold(500); // 던전 클리어 보상
            Debug.Log("[DUNGEON MANAGER] Completion reward: 500 gold");
        }
    }

    // 임시: 키보드로 방 이동 테스트
    void Update()
    {
        // N키: 다음 방
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("[INPUT] N key pressed");
            MoveToNextRoom();
        }

        // B키: 이전 방
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("[INPUT] B key pressed");
            MoveToPreviousRoom();
        }
    }

    // Getters
    public Room CurrentRoom => currentRoomIndex >= 0 && currentRoomIndex < rooms.Count ? rooms[currentRoomIndex] : null;
    public int CurrentRoomIndex => currentRoomIndex;
    public int TotalRooms => rooms.Count;
}