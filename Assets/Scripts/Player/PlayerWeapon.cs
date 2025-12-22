using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 무기 공격
/// 2025-12-04 (2일차) : 근접공격
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
    #region Constants
    private const string ENEMY_LAYER = "Enemy";
    #endregion

    #region Serialized Fields
    [Header("Weapon")]
    [SerializeField] private WeaponData currentWeapon;

    [Header("Attack Point")]
    [SerializeField] private Transform attackPoint;//공격 시작 위치

    [Header("Effects")]
    [SerializeField] private GameObject attackEffectPrefab;
    #endregion

    #region State
    private float attackCooldown = 0f;
    private Camera mainCamera;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateCooldown();
    }
    #endregion

    #region Cooldown Management
    /// <summary>
    /// 쿨다운 감소
    /// </summary>
    private void UpdateCooldown()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
    }
    #endregion

    #region Attack System
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

        SetAttackCooldown();
        PlayAttackSound();

        if (currentWeapon.IsMelee)
        {
            PerformMeleeAttack();
        }
        else
        {
            PerformRangedAttack();
        }

        Debug.Log($"[PLAYER WEAPON] {currentWeapon.weaponName} 공격!");
    }

    /// <summary>
    /// 공격 쿨다운 설정
    /// </summary>
    private void SetAttackCooldown()
    {
        float attackSpeedMultiplier = GetAttackSpeedMultiplier();
        attackCooldown = currentWeapon.attackSpeed / attackSpeedMultiplier;
    }

    /// <summary>
    /// 공격 속도 배율 가져오기 (시너지 포함)
    /// </summary>
    private float GetAttackSpeedMultiplier()
    {
        if (PlayerStats.Instance != null)
        {
            return PlayerStats.Instance.GetAttackSpeedMultiplier();
        }
        return 1f;
    }

    /// <summary>
    /// 공격 사운드 재생
    /// </summary>
    private void PlayAttackSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAttackSFX();
        }
    }
    #endregion

    #region Melee Attack
    /// <summary>
    /// 근접 공격 실행
    /// </summary>
    private void PerformMeleeAttack()
    {
        Collider2D[] hits = DetectEnemiesInRange();
        DamageAllHits(hits);
        SpawnAttackEffect();
    }
    /// <summary>
    /// 범위 내 적 탐지
    /// </summary>
    private Collider2D[] DetectEnemiesInRange()
    {
        return Physics2D.OverlapCircleAll(
            attackPoint.position,
            currentWeapon.attackRange,
            LayerMask.GetMask(ENEMY_LAYER)
        );
    }

    /// <summary>
    /// 탐지된 모든 적에게 데미지
    /// </summary>
    private void DamageAllHits(Collider2D[] hits)
    {
        foreach (Collider2D hit in hits)
        {
            DamageEnemy(hit);
        }
    }

    /// <summary>
    /// 개별 적에게 데미지
    /// </summary>
    private void DamageEnemy(Collider2D hit)
    {
        Enemy enemy = hit.GetComponent<Enemy>();
        if (enemy != null)
        {
            int damage = CalculateDamage();
            enemy.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 최종 데미지 계산 (시너지 포함)
    /// </summary>
    private int CalculateDamage()
    {
        if (PlayerStats.Instance != null)
        {
            return PlayerStats.Instance.CalculateDamage() + currentWeapon.damage;
        }
        return currentWeapon.damage;
    }

    /// <summary>
    /// 공격 이펙트 생성
    /// </summary>
    private void SpawnAttackEffect()
    {
        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                attackEffectPrefab,
                attackPoint.position,
                attackPoint.rotation
            );
            Destroy(effect, 1f);
        }

        if (currentWeapon.attackEffectPrefab != null)
        {
            Instantiate(
                currentWeapon.attackEffectPrefab,
                attackPoint.position,
                attackPoint.rotation
            );
        }
    }
    #endregion

    #region Ranged Attack
    /// <summary>
    /// 원거리 공격 실행
    /// </summary>
    private void PerformRangedAttack()
    {
        if (!ValidateProjectile())
        {
            return;
        }

        GameObject projectile = SpawnProjectile();
        InitializeProjectile(projectile);
    }

    /// <summary>
    /// 투사체 유효성 검사
    /// </summary>
    private bool ValidateProjectile()
    {
        if (currentWeapon.projectilePrefab == null)
        {
            Debug.LogWarning("[PLAYER WEAPON] 원거리 무기인데 투사체 프리팹이 없습니다!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 투사체 생성
    /// </summary>
    private GameObject SpawnProjectile()
    {
        return Instantiate(
            currentWeapon.projectilePrefab,
            attackPoint.position,
            attackPoint.rotation
        );
    }

    /// <summary>
    /// 투사체 초기화
    /// </summary>
    private void InitializeProjectile(GameObject projectileObj)
    {
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            Vector2 direction = attackPoint.up;
            int damage = CalculateDamage();
            projectile.Initialize(damage, currentWeapon.projectileSpeed, direction);
        }
        else
        {
            Debug.LogError("[PLAYER WEAPON] 투사체에 Projectile 컴포넌트가 없습니다!");
        }
    }
    #endregion

    #region Weapon Management
    /// <summary>
    /// 무기 변경
    /// </summary>
    public void ChangeWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("[PLAYER WEAPON] Cannot change to null weapon");
            return;
        }

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
    #endregion

    #region Debug
    //디버그
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null && currentWeapon != null && currentWeapon.IsMelee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, currentWeapon.attackRange);
        }
       
    }
    #endregion
}
