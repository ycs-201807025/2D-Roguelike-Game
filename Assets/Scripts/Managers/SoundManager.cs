using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사운드 관리 시스템
/// BGM과 SFX를 관리
/// </summary>
public class SoundManager : MonoBehaviour
{
    #region Singleton
    public static SoundManager Instance { get; private set; }
    #endregion

    #region Constants
    private const float DEFAULT_BGM_VOLUME = 0.5f;
    private const float DEFAULT_SFX_VOLUME = 0.7f;
    private const string BGM_SOURCE_NAME = "BGM_Source";
    private const string SFX_SOURCE_NAME = "SFX_Source";
    #endregion

    #region Serialized Fields
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

    [Header("UI Button Sounds")] 
    [SerializeField] private AudioClip startButtonSFX;      // 게임 시작 버튼
    [SerializeField] private AudioClip upgradeButtonSFX;    // 영구 강화 버튼
    [SerializeField] private AudioClip quitButtonSFX;       // 게임 종료 버튼

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.7f;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeSingleton();
        InitializeAudioSources();
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 오디오 소스 초기화
    /// </summary>
    private void InitializeAudioSources()
    {
        CreateBGMSource();
        CreateSFXSource();
        SetInitialVolumes();

        Debug.Log("[SOUND MANAGER] Initialized");
    }

    /// <summary>
    /// BGM 소스 생성
    /// </summary>
    private void CreateBGMSource()
    {
        if (bgmSource == null)
        {
            bgmSource = CreateAudioSource(BGM_SOURCE_NAME, true);
        }
    }

    /// <summary>
    /// SFX 소스 생성
    /// </summary>
    private void CreateSFXSource()
    {
        if (sfxSource == null)
        {
            sfxSource = CreateAudioSource(SFX_SOURCE_NAME, false);
        }
    }

    /// <summary>
    /// 오디오 소스 생성 헬퍼
    /// </summary>
    private AudioSource CreateAudioSource(string sourceName, bool loop)
    {
        GameObject sourceObj = new GameObject(sourceName);
        sourceObj.transform.SetParent(transform);

        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;

        return source;
    }

    /// <summary>
    /// 초기 볼륨 설정
    /// </summary>
    private void SetInitialVolumes()
    {
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
    }
    #endregion

    #region BGM Methods
    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (!ValidateBGMClip(clip))
        {
            return;
        }

        if (IsSameClipPlaying(clip))
        {
            return;
        }

        SetBGMClip(clip, loop);
        bgmSource.Play();

        Debug.Log($"[SOUND MANAGER] Playing BGM: {clip.name}");
    }

    /// <summary>
    /// BGM 클립 유효성 검사
    /// </summary>
    private bool ValidateBGMClip(AudioClip clip)
    {
        return clip != null && bgmSource != null;
    }

    /// <summary>
    /// 같은 클립이 재생 중인지 확인
    /// </summary>
    private bool IsSameClipPlaying(AudioClip clip)
    {
        return bgmSource.clip == clip && bgmSource.isPlaying;
    }

    /// <summary>
    /// BGM 클립 설정
    /// </summary>
    private void SetBGMClip(AudioClip clip, bool loop)
    {
        bgmSource.clip = clip;
        bgmSource.loop = loop;
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

    /// <summary>
    /// 페이드 아웃 코루틴
    /// </summary>
    private IEnumerator FadeOutCoroutine(float duration)
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

    /// <summary>
    /// 던전 BGM으로 복귀 (보스 처치 후)
    /// </summary>
    public void ReturnToDungeonBGM()
    {
        PlayDungeonBGM();
        Debug.Log("[SOUND MANAGER] Returned to Dungeon BGM");
    }

    /// <summary>
    /// BGM 서서히 전환
    /// </summary>
    public void CrossfadeToDungeonBGM(float duration = 2f)
    {
        StartCoroutine(CrossfadeBGMCoroutine(dungeonBGM, duration));
    }

    /// <summary>
    /// 크로스페이드 코루틴
    /// </summary>
    private IEnumerator CrossfadeBGMCoroutine(AudioClip newClip, float duration)
    {
        float startVolume = bgmSource.volume;

        // 페이드 아웃
        float elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
            yield return null;
        }

        // BGM 변경
        bgmSource.clip = newClip;
        bgmSource.Play();

        // 페이드 인
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVolume, elapsed / (duration / 2f));
            yield return null;
        }

        bgmSource.volume = startVolume;
    }
    #endregion

    #region SFX Methods
    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (!ValidateSFXClip(clip))
        {
            return;
        }

        float finalVolume = CalculateFinalSFXVolume(volumeScale);
        sfxSource.PlayOneShot(clip, finalVolume);
    }

    /// <summary>
    /// SFX 클립 유효성 검사
    /// </summary>
    private bool ValidateSFXClip(AudioClip clip)
    {
        return clip != null && sfxSource != null;
    }

    /// <summary>
    /// 최종 SFX 볼륨 계산
    /// </summary>
    private float CalculateFinalSFXVolume(float volumeScale)
    {
        return volumeScale * sfxVolume;
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
    /// 게임 시작 버튼 소리
    /// </summary>
    public void PlayStartButtonSFX()
    {
        PlaySFX(startButtonSFX, 0.6f);
        Debug.Log("[SOUND MANAGER] Playing Start Button SFX");
    }

    /// <summary>
    /// 영구 강화 버튼 소리
    /// </summary>
    public void PlayUpgradeButtonSFX()
    {
        PlaySFX(upgradeButtonSFX, 0.5f);
        Debug.Log("[SOUND MANAGER] Playing Upgrade Button SFX");
    }

    /// <summary>
    /// 게임 종료 버튼 소리
    /// </summary>
    public void PlayQuitButtonSFX()
    {
        PlaySFX(quitButtonSFX, 0.5f);
        Debug.Log("[SOUND MANAGER] Playing Quit Button SFX");
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
