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
    #region Constants
    private const string PLAYER_TAG = "Player";
    private const float HIT_EFFECT_DURATION = 0.1f;
    private const float EFFECT_DESTROY_TIME = 1f;
    #endregion

    #region Serialized Fields
    [Header("Data")]
    [SerializeField] public EnemyData data;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    #endregion

    #region Components
    public Rigidbody2D rb;
    public Transform player;
    public SpriteRenderer spriteRenderer;
    #endregion

    #region State
    //상태
    protected int health;
    protected float attackCooldown;
    #endregion

    #region Properties
    // 접근자 (외부에서 체력 확인용)
    public int CurrentHealth => health;
    public int MaxHealth => data != null ? data.maxHealth : 0;
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeData();
    }

    protected virtual void Start()
    {
        FindPlayer();
    }

    protected virtual void Update()
    {
        if (!IsValid()) return;

        UpdateCooldown();
        UpdateBehavior();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    private void InitializeData()
    {
        if (data == null)
        {
            Debug.LogError($"[ENEMY] {gameObject.name} has no EnemyData!");
            return;
        }

        health = data.maxHealth;
        SetSprite();
    }

    /// <summary>
    /// 스프라이트 설정
    /// </summary>
    private void SetSprite()
    {
        if (spriteRenderer != null && data.sprite != null)
        {
            spriteRenderer.sprite = data.sprite;
        }
    }

    /// <summary>
    /// 플레이어 찾기
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[ENEMY] {data?.enemyName} - Player not found!");
        }
    }
    #endregion

    #region Validation
    /// <summary>
    /// 유효성 검사
    /// </summary>
    private bool IsValid()
    {
        return player != null && data != null;
    }
    #endregion

    #region Update Logic
    /// <summary>
    /// 쿨다운 업데이트
    /// </summary>
    private void UpdateCooldown()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 행동 업데이트
    /// </summary>
    protected virtual void UpdateBehavior()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        if (IsInAttackRange(distanceToPlayer))
        {
            Attack();
        }
        else if (IsInDetectionRange(distanceToPlayer))
        {
            ChasePlayer();
        }
        else
        {
            StopMovement();
        }
    }

    /// <summary>
    /// 플레이어와의 거리 계산
    /// </summary>
    protected float GetDistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    /// <summary>
    /// 공격 범위 내 확인
    /// </summary>
    protected bool IsInAttackRange(float distance)
    {
        return distance <= data.attackRange;
    }

    /// <summary>
    /// 감지 범위 내 확인
    /// </summary>
    protected bool IsInDetectionRange(float distance)
    {
        return distance <= data.detectionRange;
    }

    /// <summary>
    /// 이동 정지
    /// </summary>
    protected void StopMovement()
    {
        rb.velocity = Vector2.zero;
    }
    #endregion

    #region AI Behavior
    /// <summary>
    /// 플레이어 추격
    /// </summary>
    protected virtual void ChasePlayer()
    {
        if (player == null || rb == null || data == null) return;
        Vector2 direction = GetDirectionToPlayer();
        rb.velocity = direction * data.moveSpeed;
    }
    /// <summary>
    /// 플레이어 방향 계산
    /// </summary>
    protected Vector2 GetDirectionToPlayer()
    {
        return (player.position - transform.position).normalized;
    }
    /// <summary>
    /// 공격
    /// </summary>
    protected virtual void Attack()
    {
        StopMovement();

        if (CanAttack())
        {
            ExecuteAttack();
        }
    }
    /// <summary>
    /// 공격 가능 여부
    /// </summary>
    protected bool CanAttack()
    {
        return attackCooldown <= 0;
    }

    /// <summary>
    /// 공격 실행
    /// </summary>
    protected virtual void ExecuteAttack()
    {
        attackCooldown = data.attackCooldown;
        DamagePlayer();
        Debug.Log($"[ENEMY] {data.enemyName}이(가) 공격");
    }
    /// <summary>
    /// 플레이어에게 데미지
    /// </summary>
    protected void DamagePlayer()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(data.damage);
        }
    }
    #endregion

    #region Damage System
    /// <summary>
    /// 피격
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (!ValidateData())
        {
            return;
        }

        ApplyDamage(damage);
        SpawnHitEffect();
        StartCoroutine(HitEffect());

        if (health <= 0)
        {
            Die();
        }
    }
    /// <summary>
    /// 데이터 유효성 검사
    /// </summary>
    private bool ValidateData()
    {
        if (data == null)
        {
            Debug.LogError($"[ENEMY] {gameObject.name} TakeDamage called but data is null!");
            Destroy(gameObject);
            return false;
        }
        return true;
    }

    /// <summary>
    /// 데미지 적용
    /// </summary>
    private void ApplyDamage(int damage)
    {
        health -= damage;
        Debug.Log($"[ENEMY] {data.enemyName} 체력: {health}/{data.maxHealth}");
    }

    /// <summary>
    /// 피격 이펙트 생성
    /// </summary>
    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                hitEffectPrefab,
                transform.position,
                Quaternion.identity
            );
            Destroy(effect, EFFECT_DESTROY_TIME);
        }
    }

    /// <summary>
    /// 피격 이펙트
    /// </summary>
    protected virtual IEnumerator HitEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;

            yield return new WaitForSeconds(HIT_EFFECT_DURATION);

            spriteRenderer.color = originalColor;
        }
    }
    #endregion

    #region Death
    /// <summary>
    /// 사망
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"[ENEMY] {data.enemyName} 사망");

        PlayDeathSound();
        DropGold();
        TryDropPassiveItem();

        Destroy(gameObject);
    }
    /// <summary>
    /// 사망 사운드 재생
    /// </summary>
    private void PlayDeathSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyDeathSFX();
        }
    }

    /// <summary>
    /// 골드 드롭
    /// </summary>
    private void DropGold()
    {
        if (PlayerStats.Instance != null && data != null)
        {
            PlayerStats.Instance.AddGold(data.goldDrop);
            Debug.Log($"[ENEMY] Dropped {data.goldDrop} gold");
        }
    }
    /// <summary>
    /// 패시브 아이템 드롭 시도
    /// </summary>
    void TryDropPassiveItem()
    {
        float dropChance = CalculateDropChance();

        if (Random.value < dropChance)
        {
            DropRandomPassiveItem();
        }
    }
    /// <summary>
    /// 드롭 확률 계산 (시너지 포함)
    /// </summary>
    private float CalculateDropChance()
    {
        const float BASE_DROP_CHANCE = 0.15f;

        float dropRateMultiplier = PlayerStats.Instance != null ?
            PlayerStats.Instance.GetDropRateMultiplier() : 1f;

        return BASE_DROP_CHANCE * dropRateMultiplier;
    }

    /// <summary>
    /// 랜덤 패시브 아이템 드롭
    /// </summary>
    void DropRandomPassiveItem()
    {
        PassiveItemData[] allItems = LoadPassiveItems(); if (allItems.Length == 0)
        {
            Debug.LogWarning("[ENEMY] No passive items found in Resources/PassiveItems");
            return;
        }
        PassiveItemData randomItem = SelectRandomItem(allItems);
        CreateItemDrop(randomItem);
    }
    /// <summary>
    /// 패시브 아이템 로드
    /// </summary>
    private PassiveItemData[] LoadPassiveItems()
    {
        return Resources.LoadAll<PassiveItemData>("PassiveItems");
    }

    /// <summary>
    /// 랜덤 아이템 선택
    /// </summary>
    private PassiveItemData SelectRandomItem(PassiveItemData[] items)
    {
        return items[Random.Range(0, items.Length)];
    }

    /// <summary>
    /// 아이템 드롭 생성
    /// </summary>
    private void CreateItemDrop(PassiveItemData item)
    {
        GameObject dropObj = CreateDropObject(item);
        AddPickupComponent(dropObj, item);
        AddVisualComponents(dropObj, item);
        AddCollider(dropObj);

        Debug.Log($"[ENEMY] Dropped passive item: {item.itemName} ({item.itemType})");
    }

    /// <summary>
    /// 드롭 오브젝트 생성
    /// </summary>
    private GameObject CreateDropObject(PassiveItemData item)
    {
        GameObject dropObj = new GameObject($"Drop_{item.itemName}");
        dropObj.transform.position = transform.position;
        dropObj.layer = LayerMask.NameToLayer("Default");
        return dropObj;
    }

    /// <summary>
    /// 픽업 컴포넌트 추가
    /// </summary>
    private void AddPickupComponent(GameObject dropObj, PassiveItemData item)
    {
        PickupPassiveItem pickup = dropObj.AddComponent<PickupPassiveItem>();
        pickup.passiveItem = item;
    }

    /// <summary>
    /// 시각적 컴포넌트 추가
    /// </summary>
    private void AddVisualComponents(GameObject dropObj, PassiveItemData item)
    {
        SpriteRenderer sr = dropObj.AddComponent<SpriteRenderer>();
        sr.sprite = item.icon;
        sr.sortingOrder = 10;
        sr.color = new Color(0.8f, 0.5f, 1f); // 보라색 톤
    }

    /// <summary>
    /// Collider 추가
    /// </summary>
    private void AddCollider(GameObject dropObj)
    {
        CircleCollider2D col = dropObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
    }
    #endregion

    #region Debug
    //디버그
    protected virtual void OnDrawGizmosSelected()
    {
        if (data == null) return;

        DrawDetectionRange();
        DrawAttackRange();
    }

    /// <summary>
    /// 감지 범위 표시
    /// </summary>
    private void DrawDetectionRange()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);
    }

    /// <summary>
    /// 공격 범위 표시
    /// </summary>
    private void DrawAttackRange()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
    #endregion
}

