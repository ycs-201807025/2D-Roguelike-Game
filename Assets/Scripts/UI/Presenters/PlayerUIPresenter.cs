using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player UI Presenter (MVP 패턴)
/// Model과 View를 연결
/// </summary>
public class PlayerUIPresenter : MonoBehaviour
{
    [Header("Model")]
    private PlayerStats playerStats;

    [Header("Views")]
    [SerializeField] private HealthBarView healthBarView;
    [SerializeField] private CurrencyView currencyView;

    void Start()
    {
        // Model 초기화
        playerStats = new PlayerStats(100); // 최대 체력 100

        // Model 이벤트 구독
        playerStats.OnHealthChanged += OnHealthChanged;
        playerStats.OnGoldChanged += OnGoldChanged;
        playerStats.OnSoulsChanged += OnSoulsChanged;
        playerStats.OnPlayerDied += OnPlayerDied;

        // 초기 UI 업데이트
        UpdateAllUI();

        Debug.Log("[PRESENTER] PlayerUIPresenter initialized");
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= OnHealthChanged;
            playerStats.OnGoldChanged -= OnGoldChanged;
            playerStats.OnSoulsChanged -= OnSoulsChanged;
            playerStats.OnPlayerDied -= OnPlayerDied;
        }
    }

    /// <summary>
    /// 모든 UI 업데이트
    /// </summary>
    private void UpdateAllUI()
    {
        OnHealthChanged(playerStats.CurrentHealth, playerStats.MaxHealth);
        OnGoldChanged(playerStats.Gold);
        OnSoulsChanged(playerStats.Souls);
    }

    /// <summary>
    /// 체력 변경 시 (Model → View)
    /// </summary>
    private void OnHealthChanged(int current, int max)
    {
        if (healthBarView != null)
        {
            healthBarView.UpdateHealth(current, max);
        }
    }

    /// <summary>
    /// 골드 변경 시 (Model → View)
    /// </summary>
    private void OnGoldChanged(int amount)
    {
        if (currencyView != null)
        {
            currencyView.UpdateGold(amount);
        }
    }

    /// <summary>
    /// 영혼 변경 시 (Model → View)
    /// </summary>
    private void OnSoulsChanged(int amount)
    {
        if (currencyView != null)
        {
            currencyView.UpdateSouls(amount);
        }
    }

    /// <summary>
    /// 플레이어 사망 시
    /// </summary>
    private void OnPlayerDied()
    {
        Debug.Log("[PRESENTER] Player died!");
        // 게임오버 화면 표시 (추후 구현)
    }

    // Public API (다른 스크립트에서 호출)
    public PlayerStats GetPlayerStats() => playerStats;

    // 테스트용 메서드들
    void Update()
    {
        // T키: 데미지 테스트
        if (Input.GetKeyDown(KeyCode.T))
        {
            playerStats.TakeDamage(10);
            Debug.Log("Test: Took 10 damage");
        }

        // Y키: 회복 테스트
        if (Input.GetKeyDown(KeyCode.Y))
        {
            playerStats.Heal(20);
            Debug.Log("Test: Healed 20");
        }

        // U키: 골드 추가 테스트
        if (Input.GetKeyDown(KeyCode.U))
        {
            playerStats.AddGold(10);
            Debug.Log("Test: Added 10 gold");
        }

        // I키: 영혼 추가 테스트
        if (Input.GetKeyDown(KeyCode.I))
        {
            playerStats.AddSouls(5);
            Debug.Log("Test: Added 5 souls");
        }
    }
}
