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
        health -= damage;
        Debug.Log($"{data.enemyName} 체력 : {health}/{data.maxHealth}");

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
    }

    /// <summary>
    /// 사망
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{data.enemyName} 사망");

        // 골드 드롭
        if (PlayerStats.Instance != null && data != null)
        {
            PlayerStats.Instance.AddGold(data.goldDrop);
            Debug.Log($"[ENEMY] Dropped {data.goldDrop} gold");
        }


        //오브젝트 제거
        Destroy(gameObject);
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

