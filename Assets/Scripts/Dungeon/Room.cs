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

    [Header("Item Drop")]
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private ItemData[] possibleItems; // 드롭 가능한 아이템들
    [SerializeField] private float itemDropChance = 0.5f; // 50% 확률

    [Header("Random Event")]
    [SerializeField] private bool isEventRoom = false;

    // 이벤트
    public System.Action OnRoomCleared;

    void Update()
    {
        // 방이 활성화되어 있고, 클리어되지 않았다면
        if (isActive && !isCleared && roomData != null && roomData.hasEnemies)
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

        Debug.Log($"[ROOM] ═══ Activating Room ═══");

        // RoomData 확인
        if (roomData == null)
        {
            Debug.LogError($"[ROOM] RoomData is NULL! 프리팹에 RoomData 연결 필요");
            return;
        }

        // 사건방인 경우
        if (isEventRoom)
        {
            Debug.Log($"[ROOM] Event room: {roomData.roomName}");

            // 랜덤 사건 트리거
            RandomEventManager eventManager = FindObjectOfType<RandomEventManager>();
            if (eventManager != null)
            {
                eventManager.TriggerRandomEvent();
            }

            // 사건방은 자동 클리어 (적 없음)
            isCleared = true;
            OpenDoors();
            ActivatePortals();
            return;
        }

        // 시작방이고 적이 없으면
        if (roomData.roomType == RoomType.Start || !roomData.hasEnemies)
        {
            Debug.Log($"[ROOM] Room has no enemies - auto cleared!");
            isCleared = true;
            OpenDoors();
            ActivatePortals();
        }
        // 적 생성
        else if (roomData.hasEnemies && !isCleared)
        {
            SpawnEnemies();
            CloseDoors(); // 전투 중엔 문 닫기
            DeactivatePortals(); // 포탈 비활성화
        }
        else
        {
            OpenDoors(); // 이미 클리어했으면 문 열기
            ActivatePortals(); // 포탈 활성화
        }

    }

    /// <summary>
    /// 방 비활성화
    /// </summary>
    public void DeactivateRoom()
    {
        isActive = false;
        // 방을 완전히 끄지 않고 보이게만 유지
    }

    /// <summary>
    /// 적 생성
    /// </summary>
    private void SpawnEnemies()
    {
        if (roomData.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"{roomData.roomName}: No enemy prefabs assigned!");
            return;
        }

        int enemyCount = Random.Range(roomData.minEnemies, roomData.maxEnemies + 1);

        Debug.Log($"Spawning {enemyCount} enemies in {roomData.roomName}");

        // 스폰 포인트가 있으면 거기에, 없으면 랜덤 위치
        if (enemySpawnPoints != null && enemySpawnPoints.Length > 0)
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

        Debug.Log($"Spawned {spawnedEnemies.Count} enemies");
    }

    /// <summary>
    /// 특정 위치에 적 생성
    /// </summary>
    private void SpawnEnemyAt(Vector3 position)
    {
        GameObject enemyPrefab = roomData.enemyPrefabs[Random.Range(0, roomData.enemyPrefabs.Length)];

        // ★★★ 프리팹 검증
        if (enemyPrefab == null)
        {
            Debug.LogError($"[ROOM] Enemy prefab is null in {roomData.roomName}!");
            return;
        }

        // ★★★ Enemy 컴포넌트 확인
        Enemy enemyComponent = enemyPrefab.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError($"[ROOM] Prefab {enemyPrefab.name} has no Enemy component!");
            return;
        }

        // ★★★ EnemyData 확인
        if (enemyComponent.data == null)
        {
            Debug.LogError($"[ROOM] Prefab {enemyPrefab.name} has no EnemyData assigned!");
            return;
        }
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
        if (isCleared) return;

        isCleared = true;

        Debug.Log($"[ROOM] ✓ Room Cleared: {roomData.roomName}");
        // ★★★ 문 열리는 소리 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDoorOpenSFX();
        }
        // 문 열기
        OpenDoors();

        // 포탈 활성화
        ActivatePortals();

        // 골드 보상
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddGold(roomData.goldReward);
        }

        // ★★★ 아이템 드롭 추가 ★★★
        DropRandomItem();
    }
    /// <summary>
    /// 보상 지급
    /// </summary>
    private void GiveRewards()
    {
        if (roomData == null) return;

        PlayerUIPresenter presenter = FindObjectOfType<PlayerUIPresenter>();
        if (presenter != null)
        {
            PlayerStats stats = presenter.GetPlayerStats();
            if (stats != null && roomData.goldReward > 0)
            {
                stats.AddGold(roomData.goldReward);
                Debug.Log($"[ROOM] Reward: {roomData.goldReward} gold");
            }
        }
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
        Debug.Log($"[ROOM] ActivatePortals called for {roomData.roomName}");
        Debug.Log($"[ROOM] nextPortal is null? {nextPortal == null}");
        Debug.Log($"[ROOM] previousPortal is null? {previousPortal == null}");

        if (nextPortal != null)
        {
            nextPortal.SetActive(true);
            Debug.Log("→ Next portal activated!");
        }
        else
        {
            Debug.LogError($"[ROOM] Next portal is NULL in {roomData.roomName}! Inspector에서 연결 필요");
        }

        if (previousPortal != null)
        {
            previousPortal.SetActive(true);
            Debug.Log("← Previous portal activated!");
        }
        else
        {
            Debug.LogWarning($"[ROOM] Previous portal is NULL (시작방이면 정상)");
        }
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

    /// <summary>
    /// 방 클리어 시 아이템 드롭
    /// </summary>
    void DropRandomItem()
    {
        // 확률 체크
        if (Random.value > itemDropChance)
        {
            Debug.Log("[ROOM] No item dropped");
            return;
        }

        if (possibleItems == null || possibleItems.Length == 0)
        {
            Debug.LogWarning("[ROOM] No possible items configured");
            return;
        }

        if (itemDropPrefab == null)
        {
            Debug.LogError("[ROOM] Item drop prefab not assigned");
            return;
        }

        // 랜덤 아이템 선택 (가중치 고려)
        ItemData selectedItem = SelectRandomItem();

        if (selectedItem == null) return;

        // 아이템 드롭
        Vector3 dropPosition = transform.position; // 방 중앙
        GameObject itemObj = Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);

        ItemDrop drop = itemObj.GetComponent<ItemDrop>();
        if (drop != null)
        {
            drop.Initialize(selectedItem);
        }

        Debug.Log($"[ROOM] Dropped item: {selectedItem.itemName}");
    }

    /// <summary>
    /// 가중치를 고려한 랜덤 아이템 선택
    /// </summary>
    ItemData SelectRandomItem()
    {
        float totalWeight = 0f;
        foreach (var item in possibleItems)
        {
            if (item != null)
            {
                totalWeight += item.dropChance;
            }
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var item in possibleItems)
        {
            if (item != null)
            {
                currentWeight += item.dropChance;
                if (randomValue <= currentWeight)
                {
                    return item;
                }
            }
        }

        return possibleItems[0]; // 폴백
    }
}
