using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 투사체 (화살,마법...)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 5f; // 생존 시간
    [SerializeField] private LayerMask targetLayers; // 충돌 대상 레이어

    private int damage;
    private float speed;
    private Rigidbody2D rb;
    private float lifeTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 생존 시간 체크
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            DestroyProjectile();
        }
    }

    /// <summary>
    /// 초기화 (생성 시 호출)
    /// </summary>
    public void Initialize(int damage, float speed, Vector2 direction)
    {
        this.damage = damage;
        this.speed = speed;
        lifeTimer = 0f;

        // 방향으로 발사
        rb.velocity = direction.normalized * speed;

        // 방향으로 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    /// <summary>
    /// 충돌 처리
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 대상 레이어 체크
        if (((1 << collision.gameObject.layer) & targetLayers) == 0)
        {
            return;
        }

        // 적에게 데미지
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Projectile hit {collision.name} for {damage} damage");
        }

        // 투사체 제거
        DestroyProjectile();
    }

    /// <summary>
    /// 투사체 제거
    /// </summary>
    private void DestroyProjectile()
    {
        // TODO: 나중에 오브젝트 풀링으로 변경
        Destroy(gameObject);
    }
}
