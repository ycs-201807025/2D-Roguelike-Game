using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 무기 공격
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponData currentWeapon;

    [Header("Attack Point")]
    [SerializeField] private Transform attackPoint;//공격 시작 위치

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

        //공격 입력(좌클릭)
        if (Input.GetMouseButtonDown(0) && CanAttack())
        {
            Attack();
        }
    }

    /// <summary>
    /// 공격 가능 여부
    /// </summary>
    private bool CanAttack()
    {
        return currentWeapon != null && attackCooldown <= 0;
    }

    /// <summary>
    /// 공격 실행
    /// </summary>
    private void Attack()
    {
        attackCooldown = currentWeapon.attackSpeed;

        if (currentWeapon.IsMelee)
        {
            MeleeAttack();
        }
        else
        {
            RangedAttack();

            Debug.Log($"{currentWeapon.weaponName}공격!");
        }
    }

    ///<summary
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

        //이펙트 
        if(currentWeapon.attackEffectPrefab != null)
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
        if (currentWeapon.projectilePrefab == null) return;

        //투사체 생성
        GameObject projectile = Instantiate(
            currentWeapon.projectilePrefab,
            attackPoint.position,
            attackPoint.rotation
            );

        //투사체 대미지 적용
        //var proj = projectile.GetComponent<projectile>();
        //if(proj != null)
        //{
        //    proj.Initialize(currentWeapon.damage,currentWeapon.projectileSpeed);
        //}
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
