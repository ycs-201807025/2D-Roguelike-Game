using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대시 잔상 효과 
/// </summary>
public class DashAfterImage : MonoBehaviour
{
    [Header("Afterimage Settings")]
    [SerializeField] private float afterimageInterval = 0.05f; //잔상 생성 간격
    [SerializeField] private float afterimageDuration = 0.3f; //잔상 지속 시간
    [SerializeField] private Color afterimageColor = new Color(1f, 1f, 1f, 0.5f); //잔상 색상,반투명 흰색

    [Header("References")]
    [SerializeField] private SpriteRenderer playerSprite;

    private float afterimageTimer = 0f;
    private bool isDashing = false;

    void Awake()
    {
        //플레이어 스프라이트 자동 할당
        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
        }
    }
    void Update()
    {
        if (isDashing)
        {
            afterimageTimer += Time.deltaTime;
            if (afterimageTimer >= afterimageInterval)
            {
                CreateAfterimage();
                afterimageTimer = 0f;
            }
        }
    }

    /// <summary>
    /// 대시 시작
    /// </summary>
    public void StartDash()
    {
        isDashing = true;
        afterimageTimer = 0f;
    }

    /// <summary>
    /// 대시 종료
    /// </summary>
    public void StopDash()
    {
        isDashing = false;
    }

    /// <summary>
    /// 잔상 생성
    /// </summary>
    private void CreateAfterimage()
    {
        if (playerSprite == null) return;

        //새 게임 오브젝트 생성
        GameObject afterimage = new GameObject("Afterimage");
        afterimage.transform.position = transform.position;
        afterimage.transform.rotation = transform.rotation;

        //스프라이트 렌더러 추가
        SpriteRenderer sr = afterimage.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite.sprite;
        sr.color = afterimageColor;
        sr.sortingLayerName = playerSprite.sortingLayerName;
        sr.sortingOrder = playerSprite.sortingOrder - 1; //플레이어 뒤에 렌더링
        sr.flipX= playerSprite.flipX;
        sr.flipY= playerSprite.flipY;

        //잔상 컴포넌트 추가
        AfterimageEffect effect = afterimage.AddComponent<AfterimageEffect>();
        effect.Initialize(afterimageDuration);
    }

}
/// <summary>
/// 잔상 페이드 아웃
/// </summary>
public class AfterimageEffect : MonoBehaviour
{
    private float duration;
    private float timer;
    private SpriteRenderer sr;
    private Color initialColor;

    public void Initialize(float duration)
    {
        this.duration = duration;
        this.timer = duration;

        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if(timer <= 0)
        {
            Destroy(gameObject);
            return;
        }
        //페이드 아웃
        float alpha = timer / duration;
        sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, initialColor.a * alpha);
    }
}
