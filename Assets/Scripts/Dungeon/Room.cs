using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개별 방 관리
/// </summary>
public class Room : MonoBehaviour
{
    [Header("Room Data")]
    [SerializeField] private RoomData roomData;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Doors")]
    [SerializeField] private GameObject northDoor;
    [SerializeField] private GameObject southDoor;
    [SerializeField] private GameObject eastDoor;
    [SerializeField] private GameObject westDoor;

    [Header("Portals")]
    [SerializeField] private RoomPortal nextPortal;
    [SerializeField] private RoomPortal previousPortal;

    [Header("State")]
    private bool isCleared = false;
    private bool isActive = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    // 이벤트
    public System.Action OnRoomCleared;

    void Update()
    {
        // 방이 활성화되어 있고, 클리어되지 않았다면
        if (isActive && !isCleared && roomData.hasEnemies)
        {
            // 모든 적 제거 확인
            spawnedEnemies.RemoveAll(enemy => enemy == null);

            if (spawnedEnemies.Count == 0)
            {
                ClearRoom();
            }
        }
    }

    /// <summary>
    /// 방 활성화
    /// </summary>
    public void ActivateRoom()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        Debug.Log($"Room Activated: {roomData.roomName}");

        // 시작방이고 적이 없으면
        if (roomData.roomType == RoomType.Start && !roomData.hasEnemies)
        {
            OpenDoors(); // 문 열기 (숨기기)
        }
        else if (roomData.hasEnemies && !isCleared)
        {
            SpawnEnemies();
            CloseDoors(); // 전투 중엔 문 닫기
            DeactivatePortals();
        }
        else
        {
            OpenDoors();
            ActivatePortals();
        }
    }

    /// <summary>
    /// 방 비활성화
    /// </summary>
    public void DeactivateRoom()
    {
        isActive = false;
        // 방을 완전히 끄지 않고 보이게만 유지 (선택사항)
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// 적 생성
    /// </summary>
    private void SpawnEnemies()
    {
        if (roomData.enemyPrefabs.Length == 0) return;

        int enemyCount = Random.Range(roomData.minEnemies, roomData.maxEnemies + 1);

        // 스폰 포인트가 있으면 거기에, 없으면 랜덤 위치
        if (enemySpawnPoints.Length > 0)
        {
            for (int i = 0; i < enemyCount && i < enemySpawnPoints.Length; i++)
            {
                SpawnEnemyAt(enemySpawnPoints[i].position);
            }
        }
        else
        {
            // 랜덤 위치 생성
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 randomPos = GetRandomPositionInRoom();
                SpawnEnemyAt(randomPos);
            }
        }

        Debug.Log($"Spawned {spawnedEnemies.Count} enemies in {roomData.roomName}");
    }

    /// <summary>
    /// 특정 위치에 적 생성
    /// </summary>
    private void SpawnEnemyAt(Vector3 position)
    {
        GameObject enemyPrefab = roomData.enemyPrefabs[Random.Range(0, roomData.enemyPrefabs.Length)];
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.transform.SetParent(transform); // 방의 자식으로
        spawnedEnemies.Add(enemy);
    }

    /// <summary>
    /// 방 내 랜덤 위치
    /// </summary>
    private Vector3 GetRandomPositionInRoom()
    {
        // 방 중심에서 랜덤 오프셋
        float offsetX = Random.Range(-roomData.roomSize.x / 3f, roomData.roomSize.x / 3f);
        float offsetY = Random.Range(-roomData.roomSize.y / 3f, roomData.roomSize.y / 3f);

        return transform.position + new Vector3(offsetX, offsetY, 0);
    }

    /// <summary>
    /// 방 클리어
    /// </summary>
    private void ClearRoom()
    {
        isCleared = true;
        OpenDoors();
        ActivatePortals(); // 포탈 활성화

        Debug.Log($"Room Cleared: {roomData.roomName}");

        // 보상 지급 (추후 구현)
        // GiveRewards();

        OnRoomCleared?.Invoke();
    }

    /// <summary>
    /// 문 열기
    /// </summary>
    private void OpenDoors()
    {
        if (northDoor != null && roomData.hasNorthDoor) northDoor.SetActive(false);
        if (southDoor != null && roomData.hasSouthDoor) southDoor.SetActive(false);
        if (eastDoor != null && roomData.hasEastDoor) eastDoor.SetActive(false);
        if (westDoor != null && roomData.hasWestDoor) westDoor.SetActive(false);
    }

    /// <summary>
    /// 문 닫기
    /// </summary>
    private void CloseDoors()
    {
        if (northDoor != null && roomData.hasNorthDoor) northDoor.SetActive(true);
        if (southDoor != null && roomData.hasSouthDoor) southDoor.SetActive(true);
        if (eastDoor != null && roomData.hasEastDoor) eastDoor.SetActive(true);
        if (westDoor != null && roomData.hasWestDoor) westDoor.SetActive(true);
    }

    /// <summary>
    /// 포탈 활성화
    /// </summary>
    private void ActivatePortals()
    {
        if (nextPortal != null) nextPortal.SetActive(true);
        if (previousPortal != null) previousPortal.SetActive(true);
    }

    /// <summary>
    /// 포탈 비활성화
    /// </summary>
    private void DeactivatePortals()
    {
        if (nextPortal != null) nextPortal.SetActive(false);
        if (previousPortal != null) previousPortal.SetActive(false);
    }

    /// <summary>
    /// 플레이어 스폰 위치
    /// </summary>
    public Vector3 GetPlayerSpawnPosition()
    {
        if (playerSpawnPoint != null)
        {
            return playerSpawnPoint.position;
        }
        return transform.position;
    }

    // Getters
    public RoomData RoomData => roomData;
    public bool IsCleared => isCleared;
    public bool IsActive => isActive;
}
