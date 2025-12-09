using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 게임오버 화면 관리
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    void Start()
    {
        // 버튼 이벤트 연결
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        // 처음엔 숨김
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // PlayerStats 사망 이벤트 구독
        PlayerUIPresenter presenter = FindObjectOfType<PlayerUIPresenter>();
        if (presenter != null)
        {
            PlayerStats stats = presenter.GetPlayerStats();
            if (stats != null)
            {
                stats.OnPlayerDied += ShowGameOver;
                Debug.Log("[GAMEOVER] Subscribed to player death event");
            }
        }
    }

    /// <summary>
    /// 게임오버 화면 표시
    /// </summary>
    public void ShowGameOver()
    {
        Debug.Log("[GAMEOVER] Showing game over screen");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 게임 일시정지
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 재시작 버튼 클릭
    /// </summary>
    private void OnRestartClicked()
    {
        Debug.Log("[GAMEOVER] Restart clicked");

        // 게임 재개
        Time.timeScale = 1f;

        // 현재 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 메인메뉴 버튼 클릭
    /// </summary>
    private void OnMainMenuClicked()
    {
        Debug.Log("[GAMEOVER] Main menu clicked");

        // 게임 재개
        Time.timeScale = 1f;

        // 메인 메뉴 씬으로 (나중에 구현)
        // SceneManager.LoadScene("MainMenu");

        // 임시: 현재 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
