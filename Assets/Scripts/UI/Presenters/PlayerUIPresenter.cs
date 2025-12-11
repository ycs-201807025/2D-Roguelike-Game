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
        // PlayerStats 찾기
        playerStats = PlayerStats.Instance;

        if (playerStats == null)
        {
            Debug.LogError("[PRESENTER] PlayerStats.Instance is NULL!");
            return;
        }

        // View 유효성 체크
        if (healthBarView == null)
        {
            Debug.LogError("[PRESENTER] HealthBarView is not assigned!");
        }

        if (currencyView == null)
        {
            Debug.LogError("[PRESENTER] CurrencyView is not assigned!");
        }

        // 이벤트 구독
        SubscribeToEvents();

        // 초기 UI 업데이트
        UpdateAllUI();

        Debug.Log("[PRESENTER] PlayerUIPresenter initialized");
    }

    void SubscribeToEvents()
    {
        if (playerStats == null) return;

        playerStats.OnHealthChanged += OnHealthChanged;
        playerStats.OnGoldChanged += OnGoldChanged;
        playerStats.OnSoulsChanged += OnSoulsChanged;
    }

    void UpdateAllUI()
    {
        if (playerStats == null) return;

        if (healthBarView != null)
        {
            healthBarView.UpdateHealth(playerStats.CurrentHealth, playerStats.MaxHealth);
        }

        if (currencyView != null)
        {
            currencyView.UpdateGold(playerStats.Gold);
            currencyView.UpdateSouls(playerStats.Souls);
        }
    }

    void OnHealthChanged(int current, int max)
    {
        if (healthBarView != null)
        {
            healthBarView.UpdateHealth(current, max);
        }
    }

    void OnGoldChanged(int amount)
    {
        if (currencyView != null)
        {
            currencyView.UpdateGold(amount);
        }
    }

    void OnSoulsChanged(int amount)
    {
        if (currencyView != null)
        {
            currencyView.UpdateSouls(amount);
        }
    }

    void Update()
    {
        // null 체크 추가!
        if (playerStats == null)
        {
            return;
        }

        // 테스트 키들
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[PRESENTER] T key - Taking damage");
            playerStats.TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[PRESENTER] Y key - Healing");
            playerStats.Heal(20);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("[PRESENTER] U key - Adding gold");
            playerStats.AddGold(10);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("[PRESENTER] I key - Adding souls");
            playerStats.AddSouls(5);
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= OnHealthChanged;
            playerStats.OnGoldChanged -= OnGoldChanged;
            playerStats.OnSoulsChanged -= OnSoulsChanged;
        }
    }

    // Getter (다른 스크립트에서 접근용)
    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }
}

