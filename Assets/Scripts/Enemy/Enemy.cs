using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 AI 및 행동
/// 2025-12-04 : 간단 추격AI
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] public EnemyData data;

    [Header("References")]
    public Rigidbody2D rb;
    public Transform player;
    public SpriteRenderer spriteRenderer;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    //상태
    protected int health;
    protected float attackCooldown;

    // 접근자 (외부에서 체력 확인용)
    public int CurrentHealth => health;
    public int MaxHealth => data != null ? data.maxHealth : 0;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // 자식에서도 못 찾았으면 자기 자신에서 찾기
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        //초기 설정
        if (data != null)
        {
            health = data.maxHealth;
            if(spriteRenderer != null && data.sprite != null)
            {
                spriteRenderer.sprite = data.sprite;
            }
        }
    }

    protected virtual void Start()
    {
        //플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogWarning($"[ENEMY] {data?.enemyName} - Player not found!");
        }
    }

    protected virtual void Update()
    {
        if (player == null) return;

        if (data == null) return;

        //쿨타임 감소
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        //AI 행동
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if(distanceToPlayer <= data.attackRange)
        {
            //공격 범위 내 - 공격
            Attack();
        }
        else if(distanceToPlayer <= data.detectionRange)
        {
            //감지 범위 내 - 추격
            ChasePlayer();
        }
        else
        {
            //범위 밖 - 정지
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 플레이어 추격
    /// </summary>
    protected virtual void ChasePlayer()
    {
        if (player == null || rb == null || data == null) return;
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * data.moveSpeed;
    }

    /// <summary>
    /// 공격
    /// </summary>
    protected virtual void Attack()
    {
        //정지
        rb.velocity = Vector2.zero;

        if(attackCooldown <= 0)
        {
            attackCooldown = data.attackCooldown;

            //플레이어에게 대미지
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(data.damage);
            }

            Debug.Log($"{data.enemyName}이(가) 공격");
        }
    }
    /// <summary>
    /// 피격
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        // ★★★ Null 체크 추가
        if (data == null)
        {
            Debug.LogError($"[ENEMY] {gameObject.name} TakeDamage called but data is null!");
            Destroy(gameObject);
            return;
        }

        health -= damage;
        Debug.Log($"{data.enemyName} 체력 : {health}/{data.maxHealth}");
        // ★★★ 파티클 이펙트 추가 ★★★
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        //피격 이펙트
        StartCoroutine(HitEffect());

        if(health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 피격 이펙트
    /// </summary>
    protected virtual IEnumerator HitEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
        else
        {
            // SpriteRenderer가 없으면 그냥 넘어감
            yield return null;
        }
    }

    /// <summary>
    /// 사망
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{data.enemyName} 사망");
        // ★★★ 적 사망 소리 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyDeathSFX();
        }
        // 골드 드롭
        if (PlayerStats.Instance != null && data != null)
        {
            PlayerStats.Instance.AddGold(data.goldDrop);
            Debug.Log($"[ENEMY] Dropped {data.goldDrop} gold");
        }
        // 패시브 아이템 드롭 (일정 확률)
        TryDropPassiveItem();

        //오브젝트 제거
        Destroy(gameObject);
    }
    /// <summary>
    /// 패시브 아이템 드롭 시도
    /// </summary>
    void TryDropPassiveItem()
    {
        // 탐험가 시너지 효과 적용
        float dropRateMultiplier = PlayerStats.Instance != null ?
            PlayerStats.Instance.GetDropRateMultiplier() : 1f;

        float baseDropChance = 0.15f; // 기본 15% 확률
        float finalDropChance = baseDropChance * dropRateMultiplier;

        if (Random.value < finalDropChance)
        {
            DropRandomPassiveItem();
        }
    }

    /// <summary>
    /// 랜덤 패시브 아이템 드롭
    /// </summary>
    void DropRandomPassiveItem()
    {
        // Resources 폴더에서 모든 PassiveItemData 로드
        PassiveItemData[] allItems = Resources.LoadAll<PassiveItemData>("PassiveItems");

        if (allItems.Length == 0)
        {
            Debug.LogWarning("[ENEMY] No passive items found in Resources/PassiveItems");
            return;
        }

        // 랜덤 선택
        PassiveItemData randomItem = allItems[Random.Range(0, allItems.Length)];

        // 드롭 오브젝트 생성
        GameObject dropObj = new GameObject($"Drop_{randomItem.itemName}");
        dropObj.transform.position = transform.position;
        dropObj.layer = LayerMask.NameToLayer("Default");

        // PickupPassiveItem 컴포넌트 추가
        PickupPassiveItem pickup = dropObj.AddComponent<PickupPassiveItem>();
        pickup.passiveItem = randomItem;

        // 시각적 표현 (SpriteRenderer)
        SpriteRenderer sr = dropObj.AddComponent<SpriteRenderer>();
        sr.sprite = randomItem.icon;
        sr.sortingOrder = 10;
        sr.color = new Color(0.8f, 0.5f, 1f); // 보라색 톤

        // Collider2D 추가
        CircleCollider2D col = dropObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Debug.Log($"[ENEMY] Dropped passive item: {randomItem.itemName} ({randomItem.itemType})");
    }
    //디버그
    protected virtual void OnDrawGizmosSelected()
    {
        if (data == null) return;
        
        //감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);

        //공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
        
    }
}

