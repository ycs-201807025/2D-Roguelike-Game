using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 메인 메뉴 관리
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    #region Constants
    private const float QUIT_SOUND_DELAY = 0.5f; // 종료 전 대기 시간 (0.5초)
    private const float NORMAL_TIME_SCALE = 1f;
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button quitButton;

    [Header("Upgrade Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private UpgradeManager upgradeManager;

    [Header("Quit Settings")]
    [SerializeField] private float quitSoundDelay = 0.5f; // Inspector에서 조절 가능

    [Header("Auto Find Buttons")] 
    [SerializeField] private bool autoFindButtons = true;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        //제일 먼저 timeScale 복원 
        EnsureTimeScaleRestored();

        if (autoFindButtons)
        {
            FindButtonsIfMissing();
        }
    }
    void Start()
    {
        // 버튼 이벤트 연결
        InitializeButtons();

        // 강화 패널 비활성화
        HideUpgradePanel();

        StopAllBGM();

        // BGM 재생
        PlayMainMenuBGM();

        Debug.Log("[MENU] Main Menu Initialized");

        ValidateButtonReferences();
    }
    void Update()
    {
        // F2 키로 timeScale 확인
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log($"[DEBUG] Current Time.timeScale: {Time.timeScale}");
        }
    }
    void OnDestroy()
    {
        CleanupButtons();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// timeScale 복원 보장
    /// </summary>
    private void EnsureTimeScaleRestored()
    {
        if (Time.timeScale != NORMAL_TIME_SCALE)
        {
            Time.timeScale = NORMAL_TIME_SCALE;
            Debug.Log("[MENU] Time.timeScale was not 1, restored to normal");
        }
    }
    /// <summary>
    /// 버튼이 없으면 자동으로 찾기
    /// </summary>
    private void FindButtonsIfMissing()
    {
        // Canvas 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[MENU] Canvas not found!");
            return;
        }

        // 버튼들을 이름으로 찾기
        if (startButton == null)
        {
            startButton = FindButtonByName(canvas.transform, "게임 시작");
            if (startButton != null)
            {
                Debug.Log("[MENU] Found Start Button");
            }
        }

        if (upgradeButton == null)
        {
            upgradeButton = FindButtonByName(canvas.transform, "영구 강화");
            if (upgradeButton != null)
            {
                Debug.Log("[MENU] Found Upgrade Button");
            }
        }

        if (quitButton == null)
        {
            quitButton = FindButtonByName(canvas.transform, "게임 종료");
            if (quitButton != null)
            {
                Debug.Log("[MENU] Found Quit Button");
            }
        }

        // UpgradePanel 찾기
        if (upgradePanel == null)
        {
            GameObject panel = GameObject.Find("UpgradePanel");
            if (panel != null)
            {
                upgradePanel = panel;
                Debug.Log("[MENU] Found Upgrade Panel");
            }
        }

        // UpgradeManager 찾기
        if (upgradeManager == null)
        {
            upgradeManager = FindObjectOfType<UpgradeManager>();
            if (upgradeManager != null)
            {
                Debug.Log("[MENU] Found Upgrade Manager");
            }
        }
    }

    /// <summary>
    /// 이름으로 버튼 찾기
    /// </summary>
    private Button FindButtonByName(Transform parent, string buttonName)
    {
        // 모든 자식 중에서 이름이 일치하는 버튼 찾기
        Button[] buttons = parent.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.gameObject.name.Contains(buttonName))
            {
                return btn;
            }
        }
        return null;
    }

    /// <summary>
    /// 버튼 참조 유효성 검사
    /// </summary>
    private void ValidateButtonReferences()
    {
        if (startButton == null)
        {
            Debug.LogError("[MENU] Start Button is NULL!");
        }

        if (upgradeButton == null)
        {
            Debug.LogError("[MENU] Upgrade Button is NULL!");
        }

        if (quitButton == null)
        {
            Debug.LogError("[MENU] Quit Button is NULL!");
        }

        if (upgradePanel == null)
        {
            Debug.LogWarning("[MENU] Upgrade Panel is NULL!");
        }

        if (upgradeManager == null)
        {
            Debug.LogWarning("[MENU] Upgrade Manager is NULL!");
        }
    }
    /// <summary>
    /// 버튼 초기화
    /// </summary>
    private void InitializeButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartGame);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnOpenUpgrade);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuit);
        }
    }

    /// <summary>
    /// 강화 패널 숨김
    /// </summary>
    private void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
    /// <summary>
    /// 모든 BGM 정지
    /// </summary>
    private void StopAllBGM()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            Debug.Log("[MENU] Stopped all previous BGM");
        }
    }
    /// <summary>
    /// 메인 메뉴 BGM 재생
    /// </summary>
    private void PlayMainMenuBGM()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMainMenuBGM();
        }
        else
        {
            Debug.LogWarning("[MENU] SoundManager.Instance is NULL!");
        }
    }
    #endregion

    #region Button Handlers
    void OnStartGame()
    {
        Debug.Log("[MENU] Starting game...");

        // 게임 시작 버튼 사운드
        PlayStartButtonSound();

        SceneManager.LoadScene("GamePlay");
    }

    void OnOpenUpgrade()
    {
        Debug.Log("[MENU] Opening upgrade panel");

        // 영구 강화 버튼 사운드
        PlayUpgradeButtonSound();

        upgradeManager.OpenPanel();
    }

    void OnQuit()
    {
        Debug.Log("[MENU] Quitting game");

        // ★★★ 코루틴으로 사운드 재생 후 종료 ★★★
        StartCoroutine(QuitGameCoroutine());
    }
    #endregion

    #region Sound Methods
    /// <summary>
    /// 게임 시작 버튼 사운드
    /// </summary>
    private void PlayStartButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayStartButtonSFX();
        }
    }

    /// <summary>
    /// 영구 강화 버튼 사운드
    /// </summary>
    private void PlayUpgradeButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUpgradeButtonSFX();
        }
    }

    /// <summary>
    /// 게임 종료 버튼 사운드
    /// </summary>
    private void PlayQuitButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayQuitButtonSFX();
        }
        else
        {
            Debug.LogWarning("[MENU] SoundManager.Instance is NULL!");
        }
    }
    #endregion

    #region Quit Logic
    /// <summary>
    /// 게임 종료 코루틴 (사운드 재생 후 종료)
    /// </summary>
    private IEnumerator QuitGameCoroutine()
    {
        // 1. 버튼 비활성화 (중복 클릭 방지)
        DisableQuitButton();

        // 2. 종료 사운드 재생
        PlayQuitButtonSound();

        // 3. 사운드가 재생될 시간을 기다림
        Debug.Log($"[MENU] Waiting {quitSoundDelay} seconds for sound to play...");
        yield return new WaitForSeconds(quitSoundDelay);

        // 4. 게임 종료
        Debug.Log("[MENU] Quitting game NOW");
        QuitApplication();
    }

    /// <summary>
    /// 종료 버튼 비활성화
    /// </summary>
    private void DisableQuitButton()
    {
        if (quitButton != null)
        {
            quitButton.interactable = false;
        }
    }

    /// <summary>
    /// 애플리케이션 종료
    /// </summary>
    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Upgrade Panel
    /// <summary>
    /// 강화 패널 열기
    /// </summary>
    private void OpenUpgradePanel()
    {
        if (upgradeManager != null)
        {
            upgradeManager.OpenPanel();
        }
        else
        {
            Debug.LogError("[MENU] UpgradeManager is NULL!");
        }
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// 버튼 이벤트 정리
    /// </summary>
    private void CleanupButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartGame);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnOpenUpgrade);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuit);
        }
    }
    #endregion
}
