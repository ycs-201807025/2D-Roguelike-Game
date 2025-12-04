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
    [SerializeField] private EnemyData data;

    [Header("References")]
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer spriteRenderer;

    //상태
    private int currentHealth;
    private float attackCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //초기 설정
        if (data != null)
        {
            currentHealth = data.maxHealth;
            if(spriteRenderer != null && data.Sprite != null)
            {
                spriteRenderer.sprite = data.Sprite;
            }
        }
    }

    void Start()
    {
        //플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
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
    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * data.moveSpeed;
    }

    /// <summary>
    /// 공격
    /// </summary>
    private void Attack()
    {
        //정지
        rb.velocity = Vector2.zero;

        if(attackCooldown <= 0)
        {
            attackCooldown = data.attackCooldown;

            //플레이어에게 대미지
            //player.GetComponent<PlayerHealth>()?.TakeDamage(data.damage);

            Debug.Log($"{data.enemyName}이(가) 공격");
        }
    }
    /// <summary>
    /// 피격
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{data.enemyName} 체력 : {currentHealth}/{data.maxHealth}");

        //피격 이펙트
        StartCoroutine(HitEffect());

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 피격 이펙트
    /// </summary>
    private IEnumerator HitEffect()
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
    private void Die()
    {
        Debug.Log($"{data.enemyName} 사망");

        //재화 드랍
        //DropRewards();

        //사망이펙트

        //오브젝트 제거
        Destroy(gameObject);
    }

    //디버그
    private void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            //감지 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.detectionRange);

            //공격 범위
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.attackRange);
        }
    }
}

