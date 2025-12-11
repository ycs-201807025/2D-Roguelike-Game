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
        gameOverPanel.SetActive(false);

        restartButton.onClick.AddListener(OnRestart);
        mainMenuButton.onClick.AddListener(OnMainMenu);

        // 플레이어 사망 이벤트 구독
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied += ShowGameOver;
        }
    }

    void ShowGameOver()
    {
        Debug.Log("[GAME OVER] Showing game over screen");
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지
    }

    void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GamePlay");
    }

    void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied -= ShowGameOver;
        }
    }

}
