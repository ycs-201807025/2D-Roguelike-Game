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

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        // UI Presenter 찾기
        uiPresenter = FindObjectOfType<PlayerUIPresenter>();

        if (uiPresenter != null)
        {
            playerStats = uiPresenter.GetPlayerStats();

            // 초기 체력 설정
            playerStats.SetHealth(maxHealth, maxHealth);

            Debug.Log("[PLAYER HEALTH] Connected to UI Presenter");
        }
        else
        {
            Debug.LogError("[PLAYER HEALTH] PlayerUIPresenter not found!");
        }
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
        // 무적 상태면 데미지 무시
        if (invincibilityTimer > 0) return;

        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }

        invincibilityTimer = invincibilityDuration;

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
        if (playerStats != null)
        {
            playerStats.Heal(amount);
            Debug.Log($"[PLAYER HEALTH] Healed {amount}");
        }
    }

    /// <summary>
    /// 피격 이펙트
    /// </summary>
    private System.Collections.IEnumerator HitEffect()
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

        // 게임오버 처리 (추후 구현)
        // GameManager.Instance.GameOver();

        // 임시: 씬 재시작
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // Getters
    public int CurrentHealth => playerStats?.CurrentHealth ?? 0;
    public int MaxHealth => playerStats?.MaxHealth ?? maxHealth;
    public bool IsInvincible => invincibilityTimer > 0;
}
