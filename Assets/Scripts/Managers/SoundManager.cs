using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사운드 관리 시스템
/// BGM과 SFX를 관리
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip dungeonBGM;
    [SerializeField] private AudioClip bossBGM;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip itemPickupSFX;
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip doorOpenSFX;
    [SerializeField] private AudioClip enemyDeathSFX;
    [SerializeField] private AudioClip playerDeathSFX;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.7f;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioSources()
    {
        // AudioSource가 없으면 자동 생성
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // 볼륨 설정
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;

        Debug.Log("[SOUND MANAGER] Initialized");
    }

    #region BGM Methods

    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null || bgmSource == null) return;

        // 같은 BGM이 이미 재생 중이면 무시
        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();

        Debug.Log($"[SOUND MANAGER] Playing BGM: {clip.name}");
    }

    /// <summary>
    /// 메인 메뉴 BGM
    /// </summary>
    public void PlayMainMenuBGM()
    {
        PlayBGM(mainMenuBGM);
    }

    /// <summary>
    /// 던전 BGM
    /// </summary>
    public void PlayDungeonBGM()
    {
        PlayBGM(dungeonBGM);
    }

    /// <summary>
    /// 보스 BGM
    /// </summary>
    public void PlayBossBGM()
    {
        PlayBGM(bossBGM);
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// BGM 페이드 아웃
    /// </summary>
    public void FadeOutBGM(float duration = 1f)
    {
        if (bgmSource != null)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }
    }

    System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }

    #endregion

    #region SFX Methods

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
    }

    /// <summary>
    /// 공격 소리
    /// </summary>
    public void PlayAttackSFX()
    {
        PlaySFX(attackSFX);
    }

    /// <summary>
    /// 피격 소리
    /// </summary>
    public void PlayHitSFX()
    {
        PlaySFX(hitSFX);
    }

    /// <summary>
    /// 대시 소리
    /// </summary>
    public void PlayDashSFX()
    {
        PlaySFX(dashSFX);
    }

    /// <summary>
    /// 아이템 획득 소리
    /// </summary>
    public void PlayItemPickupSFX()
    {
        PlaySFX(itemPickupSFX);
    }

    /// <summary>
    /// 버튼 클릭 소리
    /// </summary>
    public void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickSFX, 0.5f);
    }

    /// <summary>
    /// 문 열리는 소리
    /// </summary>
    public void PlayDoorOpenSFX()
    {
        PlaySFX(doorOpenSFX);
    }

    /// <summary>
    /// 적 사망 소리
    /// </summary>
    public void PlayEnemyDeathSFX()
    {
        PlaySFX(enemyDeathSFX);
    }

    /// <summary>
    /// 플레이어 사망 소리
    /// </summary>
    public void PlayPlayerDeathSFX()
    {
        PlaySFX(playerDeathSFX);
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    /// <summary>
    /// SFX 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    #endregion
}
