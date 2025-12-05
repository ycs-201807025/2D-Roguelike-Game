using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 원거리 공격 적
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RangedEnemy : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EnemyData data;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float keepDistanceRange = 5f; // 유지 거리

    [Header("References")]
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer spriteRenderer;

    // 상태
    private int currentHealth;
    private float attackCooldown;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (data != null)
        {
            currentHealth = data.maxHealth;
            if (spriteRenderer != null && data.sprite != null)
            {
                spriteRenderer.sprite = data.sprite;
            }
        }

        // FirePoint 자동 생성
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, 0.5f, 0);
            firePoint = fp.transform;
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 쿨다운 감소
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        // AI 행동
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= data.detectionRange)
        {
            // 플레이어 방향 바라보기
            LookAtPlayer();

            if (distanceToPlayer <= data.attackRange)
            {
                // 공격 범위 내 - 공격
                Attack();

                // 너무 가까우면 후퇴
                if (distanceToPlayer < keepDistanceRange)
                {
                    MoveAwayFromPlayer();
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
            else
            {
                // 감지 범위 내 - 접근 (일정 거리까지)
                if (distanceToPlayer > keepDistanceRange)
                {
                    ChasePlayer();
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }
        else
        {
            // 범위 밖 - 정지
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 플레이어 바라보기
    /// </summary>
    private void LookAtPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
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
    /// 플레이어로부터 후퇴
    /// </summary>
    private void MoveAwayFromPlayer()
    {
        Vector2 direction = (transform.position - player.position).normalized;
        rb.velocity = direction * data.moveSpeed * 0.7f; // 후퇴 속도는 조금 느리게
    }

    /// <summary>
    /// 공격 (투사체 발사)
    /// </summary>
    private void Attack()
    {
        if (attackCooldown <= 0 && projectilePrefab != null)
        {
            attackCooldown = data.attackCooldown;

            // 투사체 생성
            GameObject projectileObj = Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation
            );

            // 투사체 초기화
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                Vector2 direction = (player.position - firePoint.position).normalized;
                projectile.Initialize(data.damage, 8f, direction);
            }

            // 레이어 변경 (플레이어와 충돌하도록)
            projectileObj.layer = LayerMask.NameToLayer("EnemyProjectile");

            Debug.Log($"{data.enemyName}이(가) 투사체 발사!");
        }
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log($"{data.enemyName} 체력: {currentHealth}/{data.maxHealth}");

        StartCoroutine(HitEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 피격 이펙트
    /// </summary>
    private System.Collections.IEnumerator HitEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log($"{data.enemyName} 사망!");
        Destroy(gameObject);
    }

    // 디버그
    void OnDrawGizmosSelected()
    {
        if (data == null) return;

        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);

        // 유지 거리
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, keepDistanceRange);
    }
}
