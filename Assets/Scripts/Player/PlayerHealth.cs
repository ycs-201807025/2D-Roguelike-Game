using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 체력 관리
/// MVP 패턴 - Presenter와 연동
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1f;
    private float invincibilityTimer = 0f;

    private SpriteRenderer spriteRenderer;
    private PlayerUIPresenter uiPresenter;
    private PlayerStats playerStats;
    private bool isInitialized = false;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        StartCoroutine(InitializeWithDelay());
    }

    IEnumerator InitializeWithDelay()
    {
        // PlayerStats와 UIPresenter가 준비될 때까지 대기
        int attempts = 0;
        while (attempts < 10)
        {
            // PlayerStats 확인
            if (PlayerStats.Instance != null)
            {
                playerStats = PlayerStats.Instance;
                Debug.Log("[PLAYER HEALTH] PlayerStats found");
                break;
            }

            Debug.Log($"[PLAYER HEALTH] Waiting for PlayerStats... Attempt {attempts + 1}");
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (playerStats == null)
        {
            Debug.LogError("[PLAYER HEALTH] PlayerStats not found after waiting!");
            yield break;
        }

        // UI Presenter 찾기
        uiPresenter = FindObjectOfType<PlayerUIPresenter>();

        if (uiPresenter == null)
        {
            Debug.LogWarning("[PLAYER HEALTH] PlayerUIPresenter not found! UI will not update.");
            // UI가 없어도 게임은 진행되도록 (PlayerStats 직접 사용)
        }
        else
        {
            Debug.Log("[PLAYER HEALTH] Connected to UI Presenter");
        }

        // 초기 체력 설정
        if (playerStats != null)
        {
            playerStats.SetHealth(playerStats.MaxHealth, playerStats.MaxHealth);
            Debug.Log($"[PLAYER HEALTH] Initialized - HP: {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
        }

        isInitialized = true;
    }
    void Update()
    {
        // 무적 시간 감소
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PLAYER HEALTH] Not initialized yet, ignoring damage");
            return;
        }
        // 무적 상태면 데미지 무시
        if (invincibilityTimer > 0) return;

        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }

        invincibilityTimer = invincibilityDuration;
        // ★★★ 피격 소리 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHitSFX();
        }

        Debug.Log($"[PLAYER HEALTH] Took {damage} damage");

        // 피격 이펙트
        StartCoroutine(HitEffect());

        if (playerStats != null && playerStats.CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(int amount)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PLAYER HEALTH] Not initialized yet, ignoring heal");
            return;
        }

        if (playerStats != null)
        {
            playerStats.Heal(amount);
            Debug.Log($"[PLAYER HEALTH] Healed {amount}");
        }
    }

    /// <summary>
    /// 피격 이펙트
    /// </summary>
    private IEnumerator HitEffect()
    {
        if (spriteRenderer != null)
        {
            // 빨간색 깜빡임
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log("[PLAYER HEALTH] Player died!");
        // ★★★ 플레이어 사망 소리 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerDeathSFX();
        }
        // 영혼 획득 계산 (사망 시에만)
        if (uiPresenter != null && playerStats != null)
        {
            // 간단한 계산: 현재 골드의 10%
            int soulsEarned = playerStats.Gold / 10;
            if (soulsEarned > 0)
            {
                playerStats.AddSouls(soulsEarned);
                Debug.Log($"[PLAYER HEALTH] Earned {soulsEarned} souls from {playerStats.Gold} gold");
            }
        }

        // 게임오버 처리는 GameOverManager가 자동으로 처리
        // (PlayerStats.OnPlayerDied 이벤트 발생)
    }

    // Getters
    public int CurrentHealth => playerStats?.CurrentHealth ?? 0;
    public int MaxHealth => playerStats?.MaxHealth ?? maxHealth;
    public bool IsInvincible => invincibilityTimer > 0;

    void OnDestroy()
    {
        // 정리
        StopAllCoroutines();
    }
}
