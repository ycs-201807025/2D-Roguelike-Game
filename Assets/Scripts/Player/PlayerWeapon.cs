using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 무기 공격
/// 2025-12-04 (2일차) : 근접공격
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponData currentWeapon;

    [Header("Attack Point")]
    [SerializeField] private Transform attackPoint;//공격 시작 위치

    [Header("Effects")]
    [SerializeField] private GameObject attackEffectPrefab;

    private float attackCooldown = 0f;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        //쿨다운 감소
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        ////공격 입력(좌클릭) - 상태머신으로 이동
        //if (Input.GetMouseButtonDown(0) && CanAttack())
        //{
        //    Attack();
        //}
    }

    /// <summary>
    /// 공격 가능 여부
    /// </summary>
    public bool CanAttack()
    {
        return currentWeapon != null && attackCooldown <= 0;
    }

    /// <summary>
    /// 공격 실행
    /// </summary>
    public void Attack()
    {
        if (!CanAttack()) return;

        attackCooldown = currentWeapon.attackSpeed;

        // ★★★ 공격 소리 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAttackSFX();
        }

        if (currentWeapon.IsMelee)
        {
            MeleeAttack();
        }
        else
        {
            RangedAttack();
  
        }
        Debug.Log($"{currentWeapon.weaponName}공격!");
    }

    ///<summary>
    ///근접공격
    ///</summary>
    private void MeleeAttack()
    {
        //공격 범위 내 적 탐지
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            currentWeapon.attackRange,
            LayerMask.GetMask("Enemy") //적(Enemy) 레이어
        );

        foreach (Collider2D hit in hits)
        {
            //적 데미지 처리
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentWeapon.damage);
            }
        }
        // ★★★ 파티클 이펙트 추가 ★★★
        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 1f); // 1초 후 자동 삭제
        }
        //이펙트 
        if (currentWeapon.attackEffectPrefab != null)
        {
            Instantiate(
                currentWeapon.attackEffectPrefab,
                attackPoint.position,
                attackPoint.rotation
            );
        }
    }
    /// <summary>
    /// 원거리 공격
    /// </summary>
    private void RangedAttack()
    {
        if (currentWeapon.projectilePrefab == null)
        {
            Debug.LogWarning("원거리 무기인데 투사체 프리팹이 없습니다!");
            return;
        }

        // 투사체 생성
        GameObject projectileObj = Instantiate(
            currentWeapon.projectilePrefab,
            attackPoint.position,
            attackPoint.rotation
        );

        // 투사체 초기화
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            // 발사 방향 (attackPoint의 up 방향)
            Vector2 direction = attackPoint.up;
            projectile.Initialize(currentWeapon.damage, currentWeapon.projectileSpeed, direction);
        }
        else
        {
            Debug.LogError("투사체에 Projectile 컴포넌트가 없습니다!");
        }
    }

    /// <summary>
    /// 무기 변경
    /// </summary>
    public void ChangeWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;
        attackCooldown = 0;
        Debug.Log($"무기 변경: {newWeapon.weaponName}");
    }
    /// <summary>
    /// 무기 장착 (Inventory에서 사용) ★★★ 추가! ★★★
    /// </summary>
    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("[PLAYER WEAPON] Cannot equip null weapon");
            return;
        }

        currentWeapon = newWeapon;
        attackCooldown = 0;

        Debug.Log($"[PLAYER WEAPON] Equipped: {newWeapon.weaponName}");
    }
    //디버그
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null && currentWeapon != null && currentWeapon.IsMelee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, currentWeapon.attackRange);
        }
       
    }
}
