using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 체력 관리
/// MVP 패턴 - Presenter와 연동
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    #region Constants
    private const int MAX_INIT_ATTEMPTS = 10;
    private const float INIT_RETRY_DELAY = 0.1f;
    private const int HIT_FLASH_COUNT = 3;
    private const float HIT_FLASH_DURATION = 0.1f;
    #endregion

    #region Serialized Fields
    [Header("Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1f;
    #endregion

    #region Components
    private SpriteRenderer spriteRenderer;
    private PlayerUIPresenter uiPresenter;
    private PlayerStats playerStats;
    #endregion

    #region State
    private float invincibilityTimer = 0f;
    private bool isInitialized = false;
    #endregion

    #region Properties
    public int CurrentHealth => playerStats?.CurrentHealth ?? 0;
    public int MaxHealth => playerStats?.MaxHealth ?? maxHealth;
    public bool IsInvincible => invincibilityTimer > 0;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        StartCoroutine(InitializeWithDelay());
    }
    void Update()
    {
        UpdateInvincibility();
    }
    #endregion

    #region Initialization
    IEnumerator InitializeWithDelay()
    {
        // PlayerStats와 UIPresenter가 준비될 때까지 대기
        int attempts = 0;
        while (attempts < MAX_INIT_ATTEMPTS)
        {
            if (TryInitialize())
            {
                yield break;
            }

            Debug.Log($"[PLAYER HEALTH] Waiting for PlayerStats... Attempt {attempts + 1}");
            yield return new WaitForSeconds(INIT_RETRY_DELAY);
            attempts++;
        }

        HandleInitializationFailure();
    }
    /// <summary>
    /// 초기화 시도
    /// </summary>
    private bool TryInitialize()
    {
        if (!FindPlayerStats())
        {
            return false;
        }

        FindUIPresenter();
        InitializeHealth();

        isInitialized = true;
        Debug.Log($"[PLAYER HEALTH] Initialized - HP: {CurrentHealth}/{MaxHealth}");

        return true;
    }

    /// <summary>
    /// PlayerStats 찾기
    /// </summary>
    private bool FindPlayerStats()
    {
        playerStats = PlayerStats.Instance;

        if (playerStats != null)
        {
            Debug.Log("[PLAYER HEALTH] PlayerStats found");
            return true;
        }

        return false;
    }

    /// <summary>
    /// UI Presenter 찾기
    /// </summary>
    private void FindUIPresenter()
    {
        uiPresenter = FindObjectOfType<PlayerUIPresenter>();

        if (uiPresenter == null)
        {
            Debug.LogWarning("[PLAYER HEALTH] PlayerUIPresenter not found! UI will not update.");
        }
        else
        {
            Debug.Log("[PLAYER HEALTH] Connected to UI Presenter");
        }
    }

    /// <summary>
    /// 체력 초기화
    /// </summary>
    private void InitializeHealth()
    {
        if (playerStats != null)
        {
            playerStats.SetHealth(playerStats.MaxHealth, playerStats.MaxHealth);
        }
    }

    /// <summary>
    /// 초기화 실패 처리
    /// </summary>
    private void HandleInitializationFailure()
    {
        Debug.LogError("[PLAYER HEALTH] PlayerStats not found after waiting!");
    }
    #endregion

    #region Invincibility
    /// <summary>
    /// 무적 시간 업데이트
    /// </summary>
    private void UpdateInvincibility()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 무적 시간 설정
    /// </summary>
    private void SetInvincibility()
    {
        invincibilityTimer = invincibilityDuration;
    }
    #endregion

    #region Damage System
    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!ValidateForDamage())
        {
            return;
        }

        if (IsInvincible)
        {
            return;
        }

        ApplyDamage(damage);
        SetInvincibility();
        PlayHitSound();
        StartCoroutine(HitEffect());

        if (playerStats.CurrentHealth <= 0)
        {
            Die();
        }
    }
    /// <summary>
    /// 데미지 유효성 검사
    /// </summary>
    private bool ValidateForDamage()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PLAYER HEALTH] Not initialized yet, ignoring damage");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 데미지 적용
    /// </summary>
    private void ApplyDamage(int damage)
    {
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            Debug.Log($"[PLAYER HEALTH] Took {damage} damage");
        }
    }

    /// <summary>
    /// 피격 사운드 재생
    /// </summary>
    private void PlayHitSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHitSFX();
        }
    }
    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(int amount)
    {
        if (!ValidateForHeal())
        {
            return;
        }

        if (playerStats != null)
        {
            playerStats.Heal(amount);
            Debug.Log($"[PLAYER HEALTH] Healed {amount}");
        }
    }
    /// <summary>
    /// 회복 유효성 검사
    /// </summary>
    private bool ValidateForHeal()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PLAYER HEALTH] Not initialized yet, ignoring heal");
            return false;
        }
        return true;
    }
    #endregion

    #region Visual Effects
    /// <summary>
    /// 피격 이펙트
    /// </summary>
    private IEnumerator HitEffect()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        Color originalColor = spriteRenderer.color;
        Color hitColor = new Color(1f, 0.5f, 0.5f);

        for (int i = 0; i < HIT_FLASH_COUNT; i++)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(HIT_FLASH_DURATION);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(HIT_FLASH_DURATION);
        }
    }
    #endregion

    #region Death
    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log("[PLAYER HEALTH] Player died!");

        PlayDeathSound();
        StopBGM();
        AwardSouls();
    }
    /// <summary>
    /// 사망 사운드 재생
    /// </summary>
    private void PlayDeathSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerDeathSFX();
        }
    }
    /// <summary>
    /// BGM 정지 
    /// </summary>
    private void StopBGM()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            Debug.Log("[PLAYER HEALTH] Stopped BGM on death");
        }
    }
    /// <summary>
    /// 영혼 보상
    /// </summary>
    private void AwardSouls()
    {
        if (uiPresenter != null && playerStats != null)
        {
            int soulsEarned = CalculateSoulsEarned();

            if (soulsEarned > 0)
            {
                playerStats.AddSouls(soulsEarned);
                Debug.Log($"[PLAYER HEALTH] Earned {soulsEarned} souls from {playerStats.Gold} gold");
            }
        }
    }

    /// <summary>
    /// 획득 영혼 계산
    /// </summary>
    private int CalculateSoulsEarned()
    {
        const float GOLD_TO_SOULS_RATIO = 0.1f;
        return Mathf.RoundToInt(playerStats.Gold * GOLD_TO_SOULS_RATIO);
    }
    #endregion

    #region Cleanup
    void OnDestroy()
    {
        // 정리
        StopAllCoroutines();
    }
    #endregion
}
