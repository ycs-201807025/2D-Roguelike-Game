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
    public static WeaponAcquiredUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponRarityText;
    [SerializeField] private TextMeshProUGUI weaponDescriptionText;
    [SerializeField] private Button confirmButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float scaleUpDuration = 0.3f;

    [Header("Effects")]
    [SerializeField] private GameObject particleEffectPrefab;
    [SerializeField] private AudioClip acquireSound;

    private CanvasGroup canvasGroup;
    private bool isShowing = false;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // CanvasGroup 추가 (페이드 효과용)
        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
        }

        // 버튼 이벤트
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HidePanel);
        }

        // 처음엔 숨김
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// 무기 획득 UI 표시
    /// </summary>
    public void ShowWeaponAcquired(WeaponData weapon)
    {
        if (weapon == null)
        {
            Debug.LogError("[WEAPON ACQUIRED UI] WeaponData is null!");
            return;
        }

        if (isShowing)
        {
            Debug.LogWarning("[WEAPON ACQUIRED UI] Already showing a weapon!");
            return;
        }

        StartCoroutine(ShowWeaponCoroutine(weapon));
    }

    /// <summary>
    /// 무기 획득 연출 코루틴
    /// </summary>
    IEnumerator ShowWeaponCoroutine(WeaponData weapon)
    {
        isShowing = true;

        // 패널 활성화
        if (panel != null)
        {
            panel.SetActive(true);
        }

        // 게임 일시정지 (선택사항)
        Time.timeScale = 0f;

        // UI 정보 설정
        SetWeaponInfo(weapon);

        // 파티클 효과
        if (particleEffectPrefab != null && weaponIcon != null)
        {
            Vector3 iconPos = weaponIcon.transform.position;
            GameObject particle = Instantiate(particleEffectPrefab, iconPos, Quaternion.identity, panel.transform);
            Destroy(particle, 2f);
        }

        // 사운드 재생
        if (acquireSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(acquireSound);
        }

        // 페이드 인 효과
        yield return StartCoroutine(FadeIn());

        // 아이콘 스케일 업 효과
        if (weaponIcon != null)
        {
            yield return StartCoroutine(ScaleUpIcon());
        }

        // 자동으로 닫기 (displayDuration 후)
        yield return new WaitForSecondsRealtime(displayDuration);

        // 버튼을 누르지 않았다면 자동으로 닫기
        if (isShowing)
        {
            HidePanel();
        }
    }

    /// <summary>
    /// 무기 정보 설정
    /// </summary>
    void SetWeaponInfo(WeaponData weapon)
    {
        // 무기 아이콘
        if (weaponIcon != null && weapon.weaponIcon != null)
        {
            weaponIcon.sprite = weapon.weaponIcon;
        }

        // 무기 이름
        if (weaponNameText != null)
        {
            weaponNameText.text = weapon.weaponName;
        }

        // 무기 등급
        if (weaponRarityText != null)
        {
            weaponRarityText.text = weapon.GetRarityName();
            weaponRarityText.color = weapon.GetRarityColor();
        }

        // 무기 설명 (스탯 정보)
        if (weaponDescriptionText != null)
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

            weaponDescriptionText.text = description;
        }
    }

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
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        weaponIcon.transform.localScale = startScale;
        float elapsed = 0f;

        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / scaleUpDuration;

            // EaseOutBack 효과 (통통 튀는 느낌)
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            weaponIcon.transform.localScale = Vector3.Lerp(startScale, targetScale, easeT);

            yield return null;
        }

        weaponIcon.transform.localScale = targetScale;
    }

    /// <summary>
    /// 페이드 아웃 효과
    /// </summary>
    IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 1f;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 패널 숨기기
    /// </summary>
    public void HidePanel()
    {
        if (!isShowing) return;

        StartCoroutine(HidePanelCoroutine());
    }

    /// <summary>
    /// 패널 숨기기 코루틴
    /// </summary>
    IEnumerator HidePanelCoroutine()
    {
        // 페이드 아웃
        yield return StartCoroutine(FadeOut());

        // 패널 비활성화
        if (panel != null)
        {
            panel.SetActive(false);
        }

        // 게임 재개
        Time.timeScale = 1f;

        isShowing = false;
    }

    void OnDestroy()
    {
        // 게임 재개 보장
        Time.timeScale = 1f;
    }
}
