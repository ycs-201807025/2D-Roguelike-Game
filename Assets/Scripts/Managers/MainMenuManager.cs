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
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button quitButton;

    [Header("Upgrade Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private UpgradeManager upgradeManager;

    void Start()
    {
        // 버튼 이벤트 연결
        startButton.onClick.AddListener(OnStartGame);
        upgradeButton.onClick.AddListener(OnOpenUpgrade);
        quitButton.onClick.AddListener(OnQuit);

        // 강화 패널 비활성화
        upgradePanel.SetActive(false);

        // ★★★ BGM 재생 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMainMenuBGM();
        }

        Debug.Log("[MENU] Main Menu Initialized");
    }

    void OnStartGame()
    {
        Debug.Log("[MENU] Starting game...");

        // ★★★ 버튼 클릭음 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClickSFX();
        }

        SceneManager.LoadScene("GamePlay");
    }

    void OnOpenUpgrade()
    {
        Debug.Log("[MENU] Opening upgrade panel");

        // ★★★ 버튼 클릭음 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClickSFX();
        }

        upgradeManager.OpenPanel();
    }

    void OnQuit()
    {
        Debug.Log("[MENU] Quitting game");

        // ★★★ 버튼 클릭음 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClickSFX();
        }
    #if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
    #else
    Application.Quit();
    #endif
    }
}
