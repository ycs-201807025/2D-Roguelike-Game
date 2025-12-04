using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 체력
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    //Getter
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => invincibilityTimer > 0;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1f; //피격후 무적 시간
    private float invincibilityTimer = 0f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        //무적 시간 감소
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 대미지 피격
    /// </summary>
    public void TakeDamage(int damage)
    {
        //무적 상태면 대미지 무시
        if (invincibilityTimer > 0) return;

        currentHealth -= damage;
        invincibilityTimer = invincibilityDuration;

        Debug.Log($"Player체력 : {currentHealth}/{maxHealth}");

        //피격 이펙트
        StartCoroutine(HitEffect());

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player체력 회복: {currentHealth}/{maxHealth}"); 
    }

    /// <summary>
    /// 피격 이펙트
    /// </summary>
    private IEnumerator HitEffect()
    {
        if (spriteRenderer != null)
        {
            //빨간색 깜빡임 효과
            for(int i = 0; i <3; i++)
            {
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    /// <summary>
    /// 사망
    /// </summary>
    private void Die()
    {
        Debug.Log("Player 사망");
        //게임 오버 처리
        //GameManager.Instance.GameOver();
    }

    
}
