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
    #region Constants
    private const string GAMEPLAY_SCENE = "GamePlay";
    private const string MAINMENU_SCENE = "MainMenu";
    private const float PAUSED_TIME_SCALE = 0f;
    private const float NORMAL_TIME_SCALE = 1f;
    #endregion

    #region Serialized Fields
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        Initialize();
    }

    void OnDestroy()
    {
        Cleanup();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 초기화
    /// </summary>
    private void Initialize()
    {
        HideGameOverPanel();
        SetupButtonEvents();
        SubscribeToPlayerEvents();
    }

    /// <summary>
    /// 게임오버 패널 숨김
    /// </summary>
    private void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 버튼 이벤트 설정
    /// </summary>
    private void SetupButtonEvents()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestart);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenu);
        }
    }

    /// <summary>
    /// 플레이어 이벤트 구독
    /// </summary>
    private void SubscribeToPlayerEvents()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied += ShowGameOver;
        }
    }
    #endregion

    #region Game Over Display
    /// <summary>
    /// 게임오버 화면 표시
    /// </summary>
    private void ShowGameOver()
    {
        Debug.Log("[GAME OVER] Showing game over screen");

        ActivateGameOverPanel();
        PauseGame();
    }

    /// <summary>
    /// 게임오버 패널 활성화
    /// </summary>
    private void ActivateGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    private void PauseGame()
    {
        Time.timeScale = PAUSED_TIME_SCALE;
    }
    #endregion

    #region Button Handlers
    /// <summary>
    /// 재시작 버튼 클릭
    /// </summary>
    private void OnRestart()
    {
        ResumeGame();
        LoadGameplayScene();
    }

    /// <summary>
    /// 메인 메뉴 버튼 클릭
    /// </summary>
    private void OnMainMenu()
    {
        ResumeGame();
        LoadMainMenuScene();
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    private void ResumeGame()
    {
        Time.timeScale = NORMAL_TIME_SCALE;
    }

    /// <summary>
    /// 게임플레이 씬 로드
    /// </summary>
    private void LoadGameplayScene()
    {
        SceneManager.LoadScene(GAMEPLAY_SCENE);
    }

    /// <summary>
    /// 메인 메뉴 씬 로드
    /// </summary>
    private void LoadMainMenuScene()
    {
        SceneManager.LoadScene(MAINMENU_SCENE);
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// 정리
    /// </summary>
    private void Cleanup()
    {
        UnsubscribeFromPlayerEvents();
    }

    /// <summary>
    /// 플레이어 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeFromPlayerEvents()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerDied -= ShowGameOver;
        }
    }
    #endregion
}
