using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 무기 획득 UI 관리
/// </summary>
public class WeaponAcquiredUI : MonoBehaviour
{
    #region Singleton
    public static WeaponAcquiredUI Instance { get; private set; }
    #endregion

    #region Constants
    private const float DEFAULT_FADE_DURATION = 0.5f;
    private const float DEFAULT_DISPLAY_DURATION = 3f;
    private const float DEFAULT_SCALE_DURATION = 0.3f;
    private const float FADE_OUT_DURATION = 0.3f;
    private const float HIT_COLOR_R = 1f;
    private const float HIT_COLOR_G = 0.5f;
    private const float HIT_COLOR_B = 0.5f;
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponRarityText;
    [SerializeField] private TextMeshProUGUI weaponDescriptionText;
    [SerializeField] private Button confirmButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = DEFAULT_FADE_DURATION;
    [SerializeField] private float displayDuration = DEFAULT_DISPLAY_DURATION;
    [SerializeField] private float scaleUpDuration = DEFAULT_SCALE_DURATION;

    [Header("Effects")]
    [SerializeField] private GameObject particleEffectPrefab;
    [SerializeField] private AudioClip acquireSound;
    #endregion

    #region Components
    private CanvasGroup canvasGroup;
    #endregion

    #region State
    private bool isShowing = false;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeSingleton();
        InitializeComponents();
        SetupButtonEvents();
        HidePanel();
    }

    void OnDestroy()
    {
        EnsureGameResumed();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 싱글톤 초기화
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// 버튼 이벤트 설정
    /// </summary>
    private void SetupButtonEvents()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HidePanel);
        }
    }

    /// <summary>
    /// 패널 초기 숨김
    /// </summary>
    private void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// 무기 획득 UI 표시
    /// </summary>
    public void ShowWeaponAcquired(WeaponData weapon)
    {
        if (!ValidateWeapon(weapon))
        {
            return;
        }

        if (isShowing)
        {
            Debug.LogWarning("[WEAPON ACQUIRED UI] Already showing a weapon!");
            return;
        }

        StartCoroutine(ShowWeaponCoroutine(weapon));
    }
    #endregion

    #region Validation
    /// <summary>
    /// 무기 유효성 검사
    /// </summary>
    private bool ValidateWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            Debug.LogError("[WEAPON ACQUIRED UI] WeaponData is null!");
            return false;
        }
        return true;
    }
    #endregion

    #region Show Sequence
    /// <summary>
    /// 무기 획득 연출 코루틴
    /// </summary>
    IEnumerator ShowWeaponCoroutine(WeaponData weapon)
    {
        isShowing = true;

        ActivatePanel();
        PauseGame();
        SetWeaponInfo(weapon);
        SpawnParticleEffect();
        PlayAcquireSound();

        yield return StartCoroutine(FadeIn());
        yield return StartCoroutine(ScaleUpIcon());
        yield return new WaitForSecondsRealtime(displayDuration);

        if (isShowing)
        {
            yield return StartCoroutine(HidePanelCoroutine());
        }
    }

    /// <summary>
    /// 패널 활성화
    /// </summary>
    private void ActivatePanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 파티클 효과 생성
    /// </summary>
    private void SpawnParticleEffect()
    {
        if (particleEffectPrefab != null && weaponIcon != null && panel != null)
        {
            Vector3 iconPos = weaponIcon.transform.position;
            GameObject particle = Instantiate(
                particleEffectPrefab,
                iconPos,
                Quaternion.identity,
                panel.transform
            );
            Destroy(particle, 2f);
        }
    }

    /// <summary>
    /// 획득 사운드 재생
    /// </summary>
    private void PlayAcquireSound()
    {
        if (acquireSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(acquireSound);
        }
    }
    #endregion

    #region Weapon Info Display
    /// <summary>
    /// 무기 정보 설정
    /// </summary>
    void SetWeaponInfo(WeaponData weapon)
    {
        SetWeaponIcon(weapon);
        SetWeaponName(weapon);
        SetWeaponRarity(weapon);
        SetWeaponDescription(weapon);
    }

    /// <summary>
    /// 무기 아이콘 설정
    /// </summary>
    private void SetWeaponIcon(WeaponData weapon)
    {
        if (weaponIcon != null && weapon.weaponIcon != null)
        {
            weaponIcon.sprite = weapon.weaponIcon;
        }
    }

    /// <summary>
    /// 무기 이름 설정
    /// </summary>
    private void SetWeaponName(WeaponData weapon)
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = weapon.weaponName;
        }
    }

    /// <summary>
    /// 무기 등급 설정
    /// </summary>
    private void SetWeaponRarity(WeaponData weapon)
    {
        if (weaponRarityText != null)
        {
            weaponRarityText.text = weapon.GetRarityName();
            weaponRarityText.color = weapon.GetRarityColor();
        }
    }

    /// <summary>
    /// 무기 설명 설정
    /// </summary>
    private void SetWeaponDescription(WeaponData weapon)
    {
        if (weaponDescriptionText != null)
        {
            weaponDescriptionText.text = BuildWeaponDescription(weapon);
        }
    }

    /// <summary>
    /// 무기 설명 텍스트 생성
    /// </summary>
    private string BuildWeaponDescription(WeaponData weapon)
    {
        string description = $"공격력: {weapon.damage}\n";
        description += $"공격 속도: {weapon.attackSpeed:F1}초\n";
        description += $"공격 범위: {weapon.attackRange:F1}m";

        if (weapon.isPiercing)
        {
            description += $"\n관통: {weapon.pierceCount}회";
        }

        if (weapon.isAreaAttack)
        {
            description += $"\n광역 범위: {weapon.areaRadius:F1}m";
        }

        return description;
    }
    #endregion

    #region Animations
    /// <summary>
    /// 페이드 인 효과
    /// </summary>
    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 아이콘 스케일 업 효과
    /// </summary>
    IEnumerator ScaleUpIcon()
    {
        if (weaponIcon == null) yield break;

        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        weaponIcon.transform.localScale = startScale;
        float elapsed = 0f;

        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / scaleUpDuration;

            // EaseOutBack 효과
            float easeT = CalculateEaseOutBack(t);
            weaponIcon.transform.localScale = Vector3.Lerp(startScale, targetScale, easeT);

            yield return null;
        }

        weaponIcon.transform.localScale = targetScale;
    }

    /// <summary>
    /// EaseOutBack 계산
    /// </summary>
    private float CalculateEaseOutBack(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    /// <summary>
    /// 페이드 아웃 효과
    /// </summary>
    IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 1f;
        float elapsed = 0f;

        while (elapsed < FADE_OUT_DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / FADE_OUT_DURATION);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
    #endregion

    #region Hide Sequence
    /// <summary>
    /// 패널 숨기기 코루틴
    /// </summary>
    IEnumerator HidePanelCoroutine()
    {
        yield return StartCoroutine(FadeOut());

        DeactivatePanel();
        ResumeGame();

        isShowing = false;
    }

    /// <summary>
    /// 패널 비활성화
    /// </summary>
    private void DeactivatePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 게임 재개 보장
    /// </summary>
    private void EnsureGameResumed()
    {
        Time.timeScale = 1f;
    }
    #endregion
}
