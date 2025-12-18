using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// <summary>
/// 보스 적 (여러 패턴 공격)
/// </summary>
public class Boss : Enemy
{
    [Header("Boss Patterns")]
    [SerializeField] private float phase2HealthPercent = 0.5f; // 50% 이하 시 페이즈 2
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileCount = 3; // 투사체 개수
    [SerializeField] private float projectileSpeed = 8f;

    private bool isPhase2 = false;
    private float patternCooldown = 0f;
    private int currentPattern = 0;
    
    protected override void Awake()
    {
        base.Awake(); // 부모(Enemy)의 Awake 호출
        Debug.Log($"[BOSS] {gameObject.name} Awake");
    }
    protected override void Update()
    {
        if(data == null) return;
        if (player == null)
        {
            // 플레이어 다시 찾기 시도
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                return; // 플레이어 없으면 그냥 리턴
            }
        }
        base.Update();

        // 페이즈 2 진입 체크
        if (!isPhase2 && health <= data.maxHealth * phase2HealthPercent)
        {
            EnterPhase2();
        }

        // 패턴 쿨다운
        if (patternCooldown > 0)
        {
            patternCooldown -= Time.deltaTime;
        }
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        Debug.Log("[BOSS] Entered Phase 2!");

        // 속도 증가
        if (rb != null)
        {
            data.moveSpeed *= 1.5f;
        }

        // 색상 변경 (시각적 피드백)
        var sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.red;
        }
    }

    protected override void Attack()
    {
        if (player == null) return;

        rb.velocity = Vector2.zero;

        if (attackCooldown <= 0)
        {
            attackCooldown = data.attackCooldown;

            // 보스는 여러 패턴 사용
            if (isPhase2)
            {
                ExecuteBossPattern();
            }
            else
            {
                // 페이즈 1: 기본 공격
                BasicAttack();
            }
        }
    }

    void BasicAttack()
    {
        // 근접 공격
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= data.attackRange)
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(data.damage);
                Debug.Log($"[BOSS] Basic attack! Damage: {data.damage}");
            }
        }
    }

    void ExecuteBossPattern()
    {
        if (patternCooldown > 0) return;

        patternCooldown = 2f; // 패턴 간 쿨다운

        // 패턴 순환
        currentPattern = (currentPattern + 1) % 3;

        switch (currentPattern)
        {
            case 0:
                Pattern_TripleShot();
                break;
            case 1:
                Pattern_CircleShot();
                break;
            case 2:
                Pattern_Charge();
                break;
        }
    }

    /// <summary>
    /// 패턴 1: 3방향 투사체
    /// </summary>
    void Pattern_TripleShot()
    {
        if (projectilePrefab == null || player == null) return;

        Debug.Log("[BOSS] Pattern: Triple Shot");

        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        // 중앙, 좌, 우 방향으로 발사
        float[] angles = { 0f, -20f, 20f };

        foreach (float angle in angles)
        {
            Vector2 direction = Rotate(directionToPlayer, angle);
            FireProjectile(direction);
        }
    }

    /// <summary>
    /// 패턴 2: 8방향 원형 투사체
    /// </summary>
    void Pattern_CircleShot()
    {
        if (projectilePrefab == null) return;

        Debug.Log("[BOSS] Pattern: Circle Shot");

        int count = 8;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            FireProjectile(direction);
        }
    }

    /// <summary>
    /// 패턴 3: 돌진
    /// </summary>
    void Pattern_Charge()
    {
        if (player == null) return;

        Debug.Log("[BOSS] Pattern: Charge");

        StartCoroutine(ChargeCoroutine());
    }

    IEnumerator ChargeCoroutine()
    {
        // 잠깐 멈춤 (예고)
        rb.velocity = Vector2.zero;

        // 색상 깜빡임 (경고)
        var sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = Color.yellow;
            yield return new WaitForSeconds(0.5f);
            sprite.color = originalColor;
        }

        // 플레이어 방향으로 빠르게 돌진
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * data.moveSpeed * 3f;

        // 1초간 돌진
        yield return new WaitForSeconds(1f);

        // 정지
        rb.velocity = Vector2.zero;
    }

    void FireProjectile(Vector2 direction)
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(data.damage, projectileSpeed, direction);
        }

        // 투사체 회전 (방향에 맞게)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        return new Vector2(
            cos * v.x - sin * v.y,
            sin * v.x + cos * v.y
        );
    }

    protected override void Die()
    {
        Debug.Log($"[BOSS] {data.enemyName} defeated!");

        // 골드 드롭
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddGold(data.goldDrop);
        }

        // 보스는 확정 아이템 드롭
        DropGuaranteedItem();

        Destroy(gameObject);
    }

    void DropGuaranteedItem()
    {
        // 보스 아이템 드롭 로직 (확정)
        // TODO: 특별한 아이템 드롭
        Debug.Log("[BOSS] Dropped guaranteed item!");
    }
}
