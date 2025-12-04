using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 생성 및 관리
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    //현재 상태 정보
    public int ActiveEnemyCount => activeEnemies.Count;
    public int CurrentWave => currentWave;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;//생성할 적 프리팹 배열
    [SerializeField] private int maxEnemies = 10;//최대 적 수
    [SerializeField] private float spawnInterval = 3f;//적 생성 간격

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-10, -10);//적 스폰 범위 최소 값
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(10, 10);//적 스폰 범위 최대 값
    [SerializeField] private float minDistanceFromPlayer = 3f;//플레이어로부터 최소 거리

    [Header("Wave System")]
    [SerializeField] private bool useWaveSystem = false;
    [SerializeField] private int enemiesPerWave = 5;

    private Transform player;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float spawnTimer = 0f;
    private int currentWave = 0;
    private int enemiesSpawnedThisWave = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player object not found in the scene. Make sure it has the 'Player' tag.");
        }
        // 초기 적 생성
        if (useWaveSystem)
        {
            StartWave();
        }

    }

    void Update()
    {
        //죽은 적 제거
        activeEnemies.RemoveAll(enemy => enemy == null);

        if (useWaveSystem)
        {
            UpdateWaveSystem();
        }
        else
        {
            UpdateContinuousSpawn();
        }
    }

    /// <summary>
    /// 연속 생성 모드
    /// </summary>
    private void UpdateContinuousSpawn()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval && activeEnemies.Count < maxEnemies)
        {
            SpawnRandomEnemy();
            spawnTimer = 0f;
        }
    }

    /// <summary>
    /// 웨이브 시스템
    /// </summary>
    private void UpdateWaveSystem()
    {
        // 현재 웨이브의 적을 모두 처치하면 다음 웨이브
        if (activeEnemies.Count == 0 && enemiesSpawnedThisWave >= enemiesPerWave)
        {
            StartWave();
        }
    }

    /// <summary>
    /// 웨이브 시작
    /// </summary>
    private void StartWave()
    {
        currentWave++;
        enemiesSpawnedThisWave = 0;

        Debug.Log($"=== Wave {currentWave} Start ===");

        // 웨이브에 맞춰 적 수 증가
        int enemiesToSpawn = enemiesPerWave + (currentWave - 1);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnRandomEnemy();
            enemiesSpawnedThisWave++;
        }
    }

    /// <summary>
    /// 랜덤 위치에 적 생성
    /// </summary>
    private void SpawnRandomEnemy()
    {
        if(enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned!");
            return;
        }

        //랜덤 적 선택
        GameObject Prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        //유효한 스폰 위치 찾기
        Vector2 spawnPos = GetValidSpawnPosition();

        //적 생성
        GameObject enemy = Instantiate(Prefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);

        Debug.Log($"적이 스폰되었습니다 {spawnPos}");
    }

    /// <summary>
    /// 특정 위치에 특정 적 생성(외부 호출 가능)
    /// </summary>
    public void SpawnEnemyAt(GameObject prefab, Vector2 position)
    {
        GameObject enemy = Instantiate(prefab,position, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    ///<summary>
    /// 유효한 스폰 위치 찾기(플레이어로부터 일정거리)
    /// </summary>
    
    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPos;
        int attempts = 0;
        int maxAttempts = 20;

        do
        {
            //랜던 위치 생성
            spawnPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            attempts++;

            //여러번 시도시 강제 생성
            if (attempts > maxAttempts)
            {
                break;
            }

        } while (player != null && Vector2.Distance(spawnPos, player.position) < minDistanceFromPlayer);
        return spawnPos;
    }

    ///<summary>
    /// 모든 적 제거
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if(enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    ///<summary>
    /// 생성 일시정지/재시작
    /// </summary>
    public void SetSpawningEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    ///<summary>
    /// 디버그
    /// </summary>

    private void OnDrawGizmosSelected()
    {
        //스폰 영역
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (spawnAreaMin.x + spawnAreaMax.x) / 2f,
            (spawnAreaMin.y + spawnAreaMax.y) / 2f,
            0
        );
        Vector3 size = new Vector3(
            spawnAreaMax.x - spawnAreaMin.x,
            spawnAreaMax.y - spawnAreaMin.y,
            0
        );
        Gizmos.DrawWireCube( center, size );

        //플레이어 주변 최소 거리
        if(player != null)
        {
            Gizmos.color= Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        }
    }
    void OnGUI()
    {
        // 화면 좌측 상단에 정보 표시
        GUI.color = Color.white;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;

        string info = $"Enemies: {activeEnemies.Count} / {maxEnemies}";
        if (useWaveSystem)
        {
            info += $"\nWave: {currentWave}";
        }

        GUI.Label(new Rect(10, 10, 200, 50), info, style);
    }
}
