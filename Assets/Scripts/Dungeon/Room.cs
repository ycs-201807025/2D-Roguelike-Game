using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개별 방 관리
/// </summary>
public class Room : MonoBehaviour
{
    #region Constants
    private const string PLAYER_TAG = "Player";
    private const string ENEMY_LAYER = "Enemy";
    #endregion

    #region Serialized Fields
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

    [Header("Item Drop")]
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private ItemData[] possibleItems; // 드롭 가능한 아이템들
    [SerializeField] private float itemDropChance = 0.5f; // 50% 확률

    [Header("Random Event")]
    [SerializeField] private bool isEventRoom = false;
    #endregion

    #region State
    [Header("State")]
    private bool isCleared = false;
    private bool isActive = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    #endregion

    #region Events
    // 이벤트
    public System.Action OnRoomCleared;
    #endregion

    #region Properties
    public RoomData RoomData => roomData;
    public bool IsCleared => isCleared;
    public bool IsActive => isActive;
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        if (ShouldCheckForClear())
        {
            CheckForClear();
        }
    }
    #endregion

    #region Activation
    /// <summary>
    /// 방 활성화
    /// </summary>
    public void ActivateRoom()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        Debug.Log($"[ROOM] ═══ Activating Room ═══");

        if (!ValidateRoomData())
        {
            return;
        }

        if (isEventRoom)
        {
            HandleEventRoom();
            return;
        }

        if (ShouldAutoClear())
        {
            AutoClearRoom();
        }
        else if (roomData.hasEnemies && !isCleared)
        {
            PrepareForCombat();
        }
        else
        {
            OpenDoors();
            ActivatePortals();
        }
    }
    /// <summary>
    /// RoomData 유효성 검사
    /// </summary>
    private bool ValidateRoomData()
    {
        if (roomData == null)
        {
            Debug.LogError($"[ROOM] RoomData is NULL! 프리팹에 RoomData 연결 필요");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 사건방 처리
    /// </summary>
    private void HandleEventRoom()
    {
        Debug.Log($"[ROOM] Event room: {roomData.roomName}");

        TriggerRandomEvent();

        isCleared = true;
        OpenDoors();
        ActivatePortals();
    }

    /// <summary>
    /// 랜덤 사건 트리거
    /// </summary>
    private void TriggerRandomEvent()
    {
        RandomEventManager eventManager = FindObjectOfType<RandomEventManager>();
        if (eventManager != null)
        {
            eventManager.TriggerRandomEvent();
        }
    }

    /// <summary>
    /// 자동 클리어 여부 확인
    /// </summary>
    private bool ShouldAutoClear()
    {
        return roomData.roomType == RoomType.Start || !roomData.hasEnemies;
    }

    /// <summary>
    /// 자동 클리어
    /// </summary>
    private void AutoClearRoom()
    {
        Debug.Log($"[ROOM] Room has no enemies - auto cleared!");
        isCleared = true;
        OpenDoors();
        ActivatePortals();
    }

    /// <summary>
    /// 전투 준비
    /// </summary>
    private void PrepareForCombat()
    {
        SpawnEnemies();
        CloseDoors();
        DeactivatePortals();
    }

    /// <summary>
    /// 방 비활성화
    /// </summary>
    public void DeactivateRoom()
    {
        isActive = false;
    }
    #endregion

    #region Enemy Spawning
    /// <summary>
    /// 적 생성
    /// </summary>
    private void SpawnEnemies()
    {
        if (!ValidateEnemyPrefabs())
        {
            return;
        }

        int enemyCount = CalculateEnemyCount();
        Debug.Log($"Spawning {enemyCount} enemies in {roomData.roomName}");

        SpawnEnemiesAtPoints(enemyCount);

        Debug.Log($"Spawned {spawnedEnemies.Count} enemies");
    }
    /// <summary>
    /// 적 프리팹 유효성 검사
    /// </summary>
    private bool ValidateEnemyPrefabs()
    {
        if (roomData.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"{roomData.roomName}: No enemy prefabs assigned!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 생성할 적 수 계산
    /// </summary>
    private int CalculateEnemyCount()
    {
        return Random.Range(roomData.minEnemies, roomData.maxEnemies + 1);
    }
    /// <summary>
    /// 스폰 포인트에 적 생성
    /// </summary>
    private void SpawnEnemiesAtPoints(int enemyCount)
    {
        if (HasSpawnPoints())
        {
            SpawnAtDesignatedPoints(enemyCount);
        }
        else
        {
            SpawnAtRandomPositions(enemyCount);
        }
    }

    /// <summary>
    /// 스폰 포인트 존재 여부
    /// </summary>
    private bool HasSpawnPoints()
    {
        return enemySpawnPoints != null && enemySpawnPoints.Length > 0;
    }

    /// <summary>
    /// 지정된 포인트에 생성
    /// </summary>
    private void SpawnAtDesignatedPoints(int enemyCount)
    {
        for (int i = 0; i < enemyCount && i < enemySpawnPoints.Length; i++)
        {
            SpawnEnemyAt(enemySpawnPoints[i].position);
        }
    }

    /// <summary>
    /// 랜덤 위치에 생성
    /// </summary>
    private void SpawnAtRandomPositions(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 randomPos = GetRandomPositionInRoom();
            SpawnEnemyAt(randomPos);
        }
    }
    /// <summary>
    /// 특정 위치에 적 생성
    /// </summary>
    private void SpawnEnemyAt(Vector3 position)
    {
        GameObject enemyPrefab = SelectRandomEnemyPrefab();

        if (!ValidateEnemyPrefab(enemyPrefab))
        {
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.transform.SetParent(transform);
        spawnedEnemies.Add(enemy);
    }
    /// <summary>
    /// 랜덤 적 프리팹 선택
    /// </summary>
    private GameObject SelectRandomEnemyPrefab()
    {
        return roomData.enemyPrefabs[Random.Range(0, roomData.enemyPrefabs.Length)];
    }

    /// <summary>
    /// 적 프리팹 유효성 검사
    /// </summary>
    private bool ValidateEnemyPrefab(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"[ROOM] Enemy prefab is null in {roomData.roomName}!");
            return false;
        }

        Enemy enemyComponent = enemyPrefab.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError($"[ROOM] Prefab {enemyPrefab.name} has no Enemy component!");
            return false;
        }

        if (enemyComponent.data == null)
        {
            Debug.LogError($"[ROOM] Prefab {enemyPrefab.name} has no EnemyData assigned!");
            return false;
        }

        return true;
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
    #endregion

    #region Room Clearing
    /// <summary>
    /// 클리어 확인 필요 여부
    /// </summary>
    private bool ShouldCheckForClear()
    {
        return isActive && !isCleared && roomData != null && roomData.hasEnemies;
    }

    /// <summary>
    /// 클리어 확인
    /// </summary>
    private void CheckForClear()
    {
        CleanupDeadEnemies();

        if (AllEnemiesDefeated())
        {
            ClearRoom();
        }
    }

    /// <summary>
    /// 죽은 적 정리
    /// </summary>
    private void CleanupDeadEnemies()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
    }

    /// <summary>
    /// 모든 적 처치 여부
    /// </summary>
    private bool AllEnemiesDefeated()
    {
        return spawnedEnemies.Count == 0;
    }

    /// <summary>
    /// 방 클리어
    /// </summary>
    private void ClearRoom()
    {
        if (isCleared) return;

        isCleared = true;

        Debug.Log($"[ROOM] ✓ Room Cleared: {roomData.roomName}");

        PlayDoorOpenSound();
        OpenDoors();
        ActivatePortals();
        GiveRewards();
        DropRandomItem();

        OnRoomCleared?.Invoke();
    }

    /// <summary>
    /// 문 열리는 소리 재생
    /// </summary>
    private void PlayDoorOpenSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDoorOpenSFX();
        }
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
    #endregion

    #region Door Management
    /// <summary>
    /// 문 열기
    /// </summary>
    private void OpenDoors()
    {
        SetDoorState(northDoor, roomData.hasNorthDoor, false);
        SetDoorState(southDoor, roomData.hasSouthDoor, false);
        SetDoorState(eastDoor, roomData.hasEastDoor, false);
        SetDoorState(westDoor, roomData.hasWestDoor, false);
    }

    /// <summary>
    /// 문 닫기
    /// </summary>
    private void CloseDoors()
    {
        SetDoorState(northDoor, roomData.hasNorthDoor, true);
        SetDoorState(southDoor, roomData.hasSouthDoor, true);
        SetDoorState(eastDoor, roomData.hasEastDoor, true);
        SetDoorState(westDoor, roomData.hasWestDoor, true);
    }

    /// <summary>
    /// 문 상태 설정
    /// </summary>
    private void SetDoorState(GameObject door, bool hasDoor, bool active)
    {
        if (door != null && hasDoor)
        {
            door.SetActive(active);
        }
    }
    #endregion

    #region Portal Management
    /// <summary>
    /// 포탈 활성화
    /// </summary>
    private void ActivatePortals()
    {
        Debug.Log($"[ROOM] ActivatePortals called for {roomData.roomName}");

        SetPortalState(nextPortal, "Next", true);
        SetPortalState(previousPortal, "Previous", true);
    }

    /// <summary>
    /// 포탈 비활성화
    /// </summary>
    private void DeactivatePortals()
    {
        SetPortalState(nextPortal, "Next", false);
        SetPortalState(previousPortal, "Previous", false);
    }
    /// <summary>
    /// 포탈 상태 설정
    /// </summary>
    private void SetPortalState(RoomPortal portal, string portalName, bool active)
    {
        if (portal != null)
        {
            portal.SetActive(active);
            Debug.Log($"→ {portalName} portal {(active ? "activated" : "deactivated")}!");
        }
        else
        {
            if (active)
            {
                Debug.LogWarning($"[ROOM] {portalName} portal is NULL");
            }
        }
    }
    #endregion

    #region Item Dropping
    /// <summary>
    /// 랜덤 아이템 드롭
    /// </summary>
    void DropRandomItem()
    {
        if (!ShouldDropItem())
        {
            Debug.Log("[ROOM] No item dropped");
            return;
        }

        if (!ValidateItemDropSettings())
        {
            return;
        }

        ItemData selectedItem = SelectRandomItem();
        if (selectedItem != null)
        {
            CreateItemDrop(selectedItem);
        }
    }

    /// <summary>
    /// 아이템 드롭 여부 결정
    /// </summary>
    private bool ShouldDropItem()
    {
        return Random.value <= itemDropChance;
    }

    /// <summary>
    /// 아이템 드롭 설정 유효성 검사
    /// </summary>
    private bool ValidateItemDropSettings()
    {
        if (possibleItems == null || possibleItems.Length == 0)
        {
            Debug.LogWarning("[ROOM] No possible items configured");
            return false;
        }

        if (itemDropPrefab == null)
        {
            Debug.LogError("[ROOM] Item drop prefab not assigned");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 가중치 기반 랜덤 아이템 선택
    /// </summary>
    ItemData SelectRandomItem()
    {
        float totalWeight = CalculateTotalWeight();
        float randomValue = Random.Range(0f, totalWeight);

        return SelectItemByWeight(randomValue);
    }

    /// <summary>
    /// 총 가중치 계산
    /// </summary>
    private float CalculateTotalWeight()
    {
        float totalWeight = 0f;
        foreach (var item in possibleItems)
        {
            if (item != null)
            {
                totalWeight += item.dropChance;
            }
        }
        return totalWeight;
    }

    /// <summary>
    /// 가중치로 아이템 선택
    /// </summary>
    private ItemData SelectItemByWeight(float randomValue)
    {
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

        return possibleItems[0];
    }

    /// <summary>
    /// 아이템 드롭 생성
    /// </summary>
    private void CreateItemDrop(ItemData item)
    {
        Vector3 dropPosition = transform.position;
        GameObject itemObj = Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);

        InitializeItemDrop(itemObj, item);

        Debug.Log($"[ROOM] Dropped item: {item.itemName}");
    }

    /// <summary>
    /// 아이템 드롭 초기화
    /// </summary>
    private void InitializeItemDrop(GameObject itemObj, ItemData item)
    {
        ItemDrop drop = itemObj.GetComponent<ItemDrop>();
        if (drop != null)
        {
            drop.Initialize(item);
        }
    }
    #endregion

    #region Helper Methods
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
    #endregion
}
