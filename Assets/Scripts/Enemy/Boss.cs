using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// <summary>
/// 보스 적 (여러 패턴 공격)
/// </summary>
public class Boss : Enemy
{
    #region Constants
    private const float DEFAULT_PHASE2_THRESHOLD = 0.5f;
    private const float PHASE2_SPEED_MULTIPLIER = 1.5f;
    private const float PATTERN_COOLDOWN = 2f;
    private const int PATTERN_COUNT = 3;
    private const float CHARGE_WARNING_DURATION = 0.5f;
    private const float CHARGE_DURATION = 1f;
    private const float CHARGE_SPEED_MULTIPLIER = 3f;
    #endregion

    #region Serialized Fields
    [Header("Boss Patterns")]
    [SerializeField] private float phase2HealthPercent = 0.5f; // 50% 이하 시 페이즈 2
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileCount = 3; // 투사체 개수
    [SerializeField] private float projectileSpeed = 8f;
    #endregion

    #region State
    private bool isPhase2 = false;
    private float patternCooldown = 0f;
    private int currentPattern = 0;
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake(); // 부모(Enemy)의 Awake 호출
        Debug.Log($"[BOSS] {gameObject.name} Awake");
    }
    protected override void Update()
    {
        if (!ValidateData())
        {
            return;
        }

        if (!ValidatePlayer())
        {
            return;
        }

        base.Update();

        CheckPhaseTransition();
        UpdatePatternCooldown();
    }
    #endregion

    #region Validation
    /// <summary>
    /// 데이터 유효성 검사
    /// </summary>
    private bool ValidateData()
    {
        return data != null;
    }

    /// <summary>
    /// 플레이어 유효성 검사
    /// </summary>
    private bool ValidatePlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        return player != null;
    }
    #endregion

    #region Phase Management
    /// <summary>
    /// 페이즈 전환 확인
    /// </summary>
    private void CheckPhaseTransition()
    {
        if (ShouldEnterPhase2())
        {
            EnterPhase2();
        }
    }

    /// <summary>
    /// 페이즈 2 진입 조건 확인
    /// </summary>
    private bool ShouldEnterPhase2()
    {
        return !isPhase2 && health <= data.maxHealth * phase2HealthPercent;
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        Debug.Log("[BOSS] Entered Phase 2!");

        IncreaseSpeed();
        ChangeAppearance();
    }
    /// <summary>
    /// 속도 증가
    /// </summary>
    private void IncreaseSpeed()
    {
        if (rb != null && data != null)
        {
            data.moveSpeed *= PHASE2_SPEED_MULTIPLIER;
        }
    }

    /// <summary>
    /// 외형 변경 (시각적 피드백)
    /// </summary>
    private void ChangeAppearance()
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.red;
        }
    }
    #endregion

    #region Pattern Cooldown
    /// <summary>
    /// 패턴 쿨다운 업데이트
    /// </summary>
    private void UpdatePatternCooldown()
    {
        if (patternCooldown > 0)
        {
            patternCooldown -= Time.deltaTime;
        }
    }
    #endregion

    #region Attack System
    protected override void Attack()
    {
        if (player == null) return;

        StopMovement();

        if (CanAttack())
        {
            attackCooldown = data.attackCooldown;

            if (isPhase2)
            {
                ExecuteBossPattern();
            }
            else
            {
                BasicAttack();
            }
        }
    }

    void BasicAttack()
    {
        // 근접 공격
        float distance = GetDistanceToPlayer();

        if (distance <= data.attackRange)
        {
            DamagePlayer();
            Debug.Log($"[BOSS] Basic attack! Damage: {data.damage}");
        }
    }
    #endregion

    #region Boss Patterns
    void ExecuteBossPattern()
    {
        if (!CanExecutePattern())
        {
            return;
        }

        patternCooldown = PATTERN_COOLDOWN;
        currentPattern = GetNextPattern();

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
    /// 패턴 실행 가능 여부
    /// </summary>
    private bool CanExecutePattern()
    {
        return patternCooldown <= 0;
    }

    /// <summary>
    /// 다음 패턴 선택
    /// </summary>
    private int GetNextPattern()
    {
        return (currentPattern + 1) % PATTERN_COUNT;
    }
    /// <summary>
    /// 패턴 1: 3방향 투사체
    /// </summary>
    void Pattern_TripleShot()
    {
        if (!ValidateProjectile())
        {
            return;
        }

        Debug.Log("[BOSS] Pattern: Triple Shot");

        Vector2 directionToPlayer = GetDirectionToPlayer();
        float[] angles = { 0f, -20f, 20f };

        foreach (float angle in angles)
        {
            Vector2 direction = RotateVector(directionToPlayer, angle);
            FireProjectile(direction);
        }
    }

    /// <summary>
    /// 패턴 2: 8방향 원형 투사체
    /// </summary>
    void Pattern_CircleShot()
    {
        if (!ValidateProjectile())
        {
            return;
        }

        Debug.Log("[BOSS] Pattern: Circle Shot");

        int shotCount = 8;
        float angleStep = 360f / shotCount;

        for (int i = 0; i < shotCount; i++)
        {
            float angle = angleStep * i;
            Vector2 direction = CalculateCircleDirection(angle);
            FireProjectile(direction);
        }
    }
    /// <summary>
    /// 원형 방향 계산
    /// </summary>
    private Vector2 CalculateCircleDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
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
        rb.velocity = Vector2.zero;

        yield return StartCoroutine(ShowChargeWarning());

        Vector2 chargeDirection = GetDirectionToPlayer();
        rb.velocity = chargeDirection * data.moveSpeed * CHARGE_SPEED_MULTIPLIER;

        yield return new WaitForSeconds(CHARGE_DURATION);

        rb.velocity = Vector2.zero;
    }
    /// <summary>
    /// 돌진 경고 표시
    /// </summary>
    IEnumerator ShowChargeWarning()
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();

        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = Color.yellow;

            yield return new WaitForSeconds(CHARGE_WARNING_DURATION);

            sprite.color = originalColor;
        }
    }
    #endregion

    #region Projectile System
    /// <summary>
    /// 투사체 유효성 검사
    /// </summary>
    private bool ValidateProjectile()
    {
        return projectilePrefab != null && player != null;
    }

    /// <summary>
    /// 투사체 발사
    /// </summary>
    void FireProjectile(Vector2 direction)
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        InitializeProjectile(proj, direction);
        RotateProjectile(proj, direction);
    }

    /// <summary>
    /// 투사체 초기화
    /// </summary>
    private void InitializeProjectile(GameObject proj, Vector2 direction)
    {
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(data.damage, projectileSpeed, direction);
        }
    }

    /// <summary>
    /// 투사체 회전
    /// </summary>
    private void RotateProjectile(GameObject proj, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    #endregion

    #region Vector Math
    /// <summary>
    /// 벡터 회전
    /// </summary>
    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            cos * v.x - sin * v.y,
            sin * v.x + cos * v.y
        );
    }
    #endregion

    #region Death
    /// <summary>
    /// 사망 처리
    /// </summary>
    protected override void Die()
    {
        Debug.Log($"[BOSS] {data.enemyName} defeated!");

        DropGold();
        DropRareWeapon();

        Destroy(gameObject);
    }

    /// <summary>
    /// 골드 드롭
    /// </summary>
    private void DropGold()
    {
        if (PlayerStats.Instance != null && data != null)
        {
            PlayerStats.Instance.AddGold(data.goldDrop);
        }
    }

    /// <summary>
    /// 레어 무기 드롭
    /// </summary>
    void DropRareWeapon()
    {
        WeaponData[] rareWeapons = LoadRareWeapons();

        if (rareWeapons.Length == 0)
        {
            Debug.LogWarning("[BOSS] No Epic/Legendary weapons found!");
            return;
        }

        WeaponData selectedWeapon = SelectRandomWeapon(rareWeapons);
        CreateWeaponDrop(selectedWeapon, transform.position);
    }

    /// <summary>
    /// 레어 무기 로드
    /// </summary>
    private WeaponData[] LoadRareWeapons()
    {
        WeaponData[] allWeapons = Resources.LoadAll<WeaponData>("ScriptableObjects/Weapons");

        if (allWeapons == null || allWeapons.Length == 0)
        {
            Debug.LogWarning("[BOSS] No weapons found in Resources!");
            return new WeaponData[0];
        }

        return FilterRareWeapons(allWeapons);
    }

    /// <summary>
    /// Epic/Legendary 무기 필터링
    /// </summary>
    private WeaponData[] FilterRareWeapons(WeaponData[] allWeapons)
    {
        List<WeaponData> rareWeapons = new List<WeaponData>();

        foreach (var weapon in allWeapons)
        {
            if (IsRareWeapon(weapon))
            {
                rareWeapons.Add(weapon);
            }
        }

        return rareWeapons.ToArray();
    }

    /// <summary>
    /// 레어 무기 여부 확인
    /// </summary>
    private bool IsRareWeapon(WeaponData weapon)
    {
        return weapon.rarity == WeaponRarity.Epic ||
               weapon.rarity == WeaponRarity.Legendary;
    }

    /// <summary>
    /// 랜덤 무기 선택
    /// </summary>
    private WeaponData SelectRandomWeapon(WeaponData[] weapons)
    {
        return weapons[Random.Range(0, weapons.Length)];
    }

    /// <summary>
    /// 무기 드롭 생성
    /// </summary>
    void CreateWeaponDrop(WeaponData weapon, Vector3 position)
    {
        if (weapon == null) return;

        GameObject dropObj = CreateDropObject(weapon, position);
        AddWeaponDropComponent(dropObj, weapon);
        AddSpriteRenderer(dropObj, weapon);
        AddGlowEffect(dropObj, weapon);
        AddCollider(dropObj);

        Debug.Log($"[BOSS] Dropped {weapon.GetRarityName()} weapon: {weapon.weaponName}");
    }

    /// <summary>
    /// 드롭 오브젝트 생성
    /// </summary>
    private GameObject CreateDropObject(WeaponData weapon, Vector3 position)
    {
        GameObject dropObj = new GameObject($"WeaponDrop_{weapon.weaponName}");
        dropObj.transform.position = position;
        dropObj.layer = LayerMask.NameToLayer("Default");
        return dropObj;
    }

    /// <summary>
    /// WeaponDrop 컴포넌트 추가
    /// </summary>
    private void AddWeaponDropComponent(GameObject dropObj, WeaponData weapon)
    {
        WeaponDrop drop = dropObj.AddComponent<WeaponDrop>();
        drop.Initialize(weapon);
    }

    /// <summary>
    /// SpriteRenderer 추가
    /// </summary>
    private void AddSpriteRenderer(GameObject dropObj, WeaponData weapon)
    {
        SpriteRenderer sr = dropObj.AddComponent<SpriteRenderer>();

        if (weapon.weaponIcon != null)
        {
            sr.sprite = weapon.weaponIcon;
        }

        sr.sortingOrder = 10;
        sr.color = weapon.GetRarityColor();
    }

    /// <summary>
    /// 글로우 효과 추가
    /// </summary>
    private void AddGlowEffect(GameObject dropObj, WeaponData weapon)
    {
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(dropObj.transform);
        glowObj.transform.localPosition = Vector3.zero;
        glowObj.transform.localScale = Vector3.one * 1.5f;

        SpriteRenderer glowSr = glowObj.AddComponent<SpriteRenderer>();
        glowSr.sprite = weapon.weaponIcon;
        glowSr.sortingOrder = 9;

        Color glowColor = weapon.GetRarityColor();
        glowSr.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
    }

    /// <summary>
    /// Collider 추가
    /// </summary>
    private void AddCollider(GameObject dropObj)
    {
        CircleCollider2D col = dropObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.8f;
    }
    #endregion
}
