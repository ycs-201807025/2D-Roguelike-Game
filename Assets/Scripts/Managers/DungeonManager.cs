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
    [SerializeField] private GameObject bossRoomPrefab; // 추후

    [Header("Dungeon Settings")]
    [SerializeField] private int roomCount = 5; // 총 방 개수
    [SerializeField] private float roomSpacing = 30f; // 방 사이 간격

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Camera")]
    [SerializeField] private CameraRoomBounds cameraRoomBounds;

    private List<Room> rooms = new List<Room>();
    private int currentRoomIndex = 0;

    void Start()
    {
        GenerateDungeon();
        StartDungeon();
    }

    /// <summary>
    /// 던전 생성 (간단한 선형 구조)
    /// </summary>
    private void GenerateDungeon()
    {
        // 시작방
        GameObject startRoomObj = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        startRoomObj.transform.SetParent(transform);
        Room startRoom = startRoomObj.GetComponent<Room>();
        rooms.Add(startRoom);

        Debug.Log("Start room created");

        // 일반 방들 (선형으로 배치)
        for (int i = 0; i < roomCount - 1; i++)
        {
            // 랜덤 방 선택
            GameObject roomPrefab = normalRoomPrefabs[Random.Range(0, normalRoomPrefabs.Length)];

            // 위치 계산 (오른쪽으로 배치)
            Vector3 position = new Vector3((i + 1) * roomSpacing, 0, 0);

            GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity);
            roomObj.transform.SetParent(transform);
            roomObj.name = $"Room_{i + 1}";

            Room room = roomObj.GetComponent<Room>();
            rooms.Add(room);

            Debug.Log($"Room {i + 1} created at {position}");
        }

        // 모든 방 비활성화
        foreach (Room room in rooms)
        {
            room.DeactivateRoom();
        }

        Debug.Log($"Dungeon generated with {rooms.Count} rooms");
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

        // 첫 방 활성화
        currentRoomIndex = 0;
        EnterRoom(0);
    }

    /// <summary>
    /// 방 입장
    /// </summary>
    public void EnterRoom(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= rooms.Count) return;

        // 이전 방 비활성화
        if (currentRoomIndex >= 0 && currentRoomIndex < rooms.Count)
        {
            rooms[currentRoomIndex].DeactivateRoom();
        }

        // 새 방 활성화
        currentRoomIndex = roomIndex;
        Room currentRoom = rooms[currentRoomIndex];
        currentRoom.ActivateRoom();

        // 플레이어 이동
        if (player != null)
        {
            player.transform.position = currentRoom.GetPlayerSpawnPosition();
        }

        // 카메라 경계 설정
        if (cameraRoomBounds != null)
        {
            cameraRoomBounds.SetCurrentRoom(currentRoom);
        }

        // 방 클리어 이벤트 구독
        currentRoom.OnRoomCleared += OnCurrentRoomCleared;

        Debug.Log($"Entered room {currentRoomIndex}: {currentRoom.RoomData.roomName}");
    }

    /// <summary>
    /// 현재 방 클리어 시
    /// </summary>
    private void OnCurrentRoomCleared()
    {
        Debug.Log("Current room cleared!");

        // 다음 방이 있으면 포탈 생성 (또는 자동 이동)
        if (currentRoomIndex < rooms.Count - 1)
        {
            Debug.Log("Next room available. Press 'N' to continue (임시)");
        }
        else
        {
            Debug.Log("Dungeon completed!");
            OnDungeonCompleted();
        }
    }

    /// <summary>
    /// 다음 방으로 이동
    /// </summary>
    public void MoveToNextRoom()
    {
        if (currentRoomIndex < rooms.Count - 1)
        {
            // 현재 방이 클리어되었는지 확인
            if (rooms[currentRoomIndex].IsCleared)
            {
                EnterRoom(currentRoomIndex + 1);
            }
            else
            {
                Debug.Log("현재 방을 클리어하세요!");
            }
        }
    }

    /// <summary>
    /// 이전 방으로 이동 (백트래킹)
    /// </summary>
    public void MoveToPreviousRoom()
    {
        if (currentRoomIndex > 0)
        {
            EnterRoom(currentRoomIndex - 1);
        }
    }

    /// <summary>
    /// 던전 완료
    /// </summary>
    private void OnDungeonCompleted()
    {
        Debug.Log("=== DUNGEON CLEARED! ===");
        // 보상 지급, 다음 층으로 이동 등 (추후 구현)
    }

    // 임시: 키보드로 방 이동 테스트
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            MoveToNextRoom();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            MoveToPreviousRoom();
        }
    }

    // Getters
    public Room CurrentRoom => currentRoomIndex >= 0 && currentRoomIndex < rooms.Count ? rooms[currentRoomIndex] : null;
    public int CurrentRoomIndex => currentRoomIndex;
    public int TotalRooms => rooms.Count;
}