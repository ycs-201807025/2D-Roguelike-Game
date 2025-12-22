using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 이동 및 회전 제어
/// 2025-12-03 (1일차) : 기본 이동, 마우스 방향 회전
/// 플레이어 대시
/// 2025-12-04 (2일차) : 대시 기능 추가
/// 플레이어 상태 패턴
/// 2025-12-05 (3일차) : 상태 머신 
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerController : MonoBehaviour
{
    #region Constants
    private const float ROTATION_OFFSET = 90f;
    private const float MIN_MOVE_INPUT = 0.1f;
    #endregion

    #region Serialized Fields
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    #endregion

    #region Components
    private Rigidbody2D rb;
    private Camera mainCamera;
    private Animator animator;
    private DashAfterImage dashAfterImage;
    private PlayerWeapon playerWeapon;
    private PlayerStateMachine stateMachine;
    #endregion

    #region Input State
    private Vector2 moveInput;
    private Vector2 mousePosition;
    private bool dashInput;
    private bool attackInput;
    private bool inputEnabled = true;
    #endregion

    #region Dash State
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;
    #endregion

    //입력 활성화

    #region Properties
    //상태에서 접근
    public Vector2 MoveInput => moveInput;
    public bool HasMoveInput => moveInput.magnitude > 0.1f;
    public bool IsDashInput => dashInput;
    public bool IsAttackInput => attackInput;
    public float DashDuration => dashDuration;
    public Vector2 DashDirection => dashDirection;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeComponents();
    }
    
    void Update()
    {
        if (!inputEnabled) return;

        //입력 받기
        HandleInput();

        //쿨다운 업데이트
        UpdateCooldowns();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 필수 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        dashAfterImage = GetComponent<DashAfterImage>();
        playerWeapon = GetComponent<PlayerWeapon>();
        stateMachine = GetComponent<PlayerStateMachine>();

        if (mainCamera == null)
        {
            Debug.LogError("[PLAYER] Main Camera not found!");
        }
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// 키보드 마우스 입력 처리
    /// </summary>
    private void HandleInput()
    {
        //WASD 입력 받기
        float horizontal = Input.GetAxisRaw("Horizontal");//A,D
        float vertical = Input.GetAxisRaw("Vertical");//W,S
        moveInput = new Vector2(horizontal, vertical).normalized;

        //마우스 위치
        if (mainCamera != null)
        {
            mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        // 대시 입력
        dashInput = Input.GetKeyDown(KeyCode.Space);

        //공격 입력
        attackInput = Input.GetMouseButtonDown(0);
    }

    ///<summary>
    /// 쿨다운 업데이트
    ///</summary>
    private void UpdateCooldowns()
    {
        if(dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    #endregion

    #region Movement
    /// <summary>
    /// 플레이어 이동
    /// </summary>
    public void HandleMovement()
    {
        float finalMoveSpeed = GetFinalMoveSpeed();
        rb.velocity = moveInput * finalMoveSpeed;

        UpdateMovementAnimation();
        //// 시너지 효과가 적용된 이동속도 사용
        //float finalMoveSpeed = PlayerStats.Instance != null ?
        //    PlayerStats.Instance.GetFinalMoveSpeed() : moveSpeed;

        //rb.velocity = moveInput * moveSpeed;

        ////애니메이션 파라미터 
        //if (animator != null)
        //{
        //    animator.SetFloat("Speed", moveInput.magnitude);
        //}
    }
    /// <summary>
    /// 최종 이동 속도 계산 (시너지 효과 포함)
    /// </summary>
    private float GetFinalMoveSpeed()
    {
        if (PlayerStats.Instance != null)
        {
            return PlayerStats.Instance.GetFinalMoveSpeed();
        }
        return moveSpeed;
    }

    /// <summary>
    /// 이동 애니메이션 업데이트
    /// </summary>
    private void UpdateMovementAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
        }
    }

    /// <summary>
    /// 마우스 방향 회전
    /// </summary>
    public void HandleRotation()
    {
        Vector2 direction = mousePosition - (Vector2)transform.position;
        float angle = CalculateRotationAngle(direction);
        transform.rotation = Quaternion.Euler(0, 0, angle - ROTATION_OFFSET);
        ////플레이어 -> 마우스 방향 벡터
        //Vector2 direction = mousePosition - (Vector2)transform.position;

        ////각도 계산
        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        ////회전 적용
        //transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    /// <summary>
    /// 방향 벡터로부터 회전 각도 계산
    /// </summary>
    private float CalculateRotationAngle(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
    /// <summary>
    /// 이동 정지
    /// </summary>
    public void StopMovement()
    {
        rb.velocity = Vector2.zero;

        UpdateMovementAnimation();
    }
    #endregion

    #region Dash
    /// <summary>
    /// 대시 가능 여부 확인
    /// </summary>
    public bool CanDash()
    {
        return dashCooldownTimer <= 0;
    }

    /// <summary>
    /// 대시 시작
    /// </summary>
    public void StartDash()
    {       
        dashCooldownTimer = dashCooldown;

        PlayDashSound();
        CalculateDashDirection();
        StartDashAfterImage();
        PlayDashAnimation();
        //// ★★★ 대시 소리 추가 ★★★
        //if (SoundManager.Instance != null)
        //{
        //    SoundManager.Instance.PlayDashSFX();
        //}

        ////이동 중일때는 이동방향으로 아닐때는 마우스방향으로
        //if (moveInput.magnitude > 0.1f)
        //{
        //    dashDirection = moveInput;
        //}
        //else
        //{
        //    dashDirection = (mousePosition - (Vector2)transform.position).normalized;
        //}

        ////잔상 효과 시작
        //if (dashAfterImage != null)
        //{
        //    dashAfterImage.StartDash();
        //    //Debug.Log("DashAfterimage.StartDash() 호출됨");
        //}

        ////애니메이션
        //if (animator != null)
        //{
        //    animator.SetTrigger("Dash");
        //}
        ////Debug.Log("PlayerController.StartDash() 호출됨");
    }
    /// <summary>
    /// 대시 방향 계산
    /// </summary>
    private void CalculateDashDirection()
    {
        if (moveInput.magnitude > MIN_MOVE_INPUT)
        {
            // 이동 중이면 이동 방향으로
            dashDirection = moveInput;
        }
        else
        {
            // 정지 중이면 마우스 방향으로
            dashDirection = (mousePosition - (Vector2)transform.position).normalized;
        }
    }

    /// <summary>
    /// 대시 잔상 효과 시작
    /// </summary>
    private void StartDashAfterImage()
    {
        if (dashAfterImage != null)
        {
            dashAfterImage.StartDash();
        }
    }

    /// <summary>
    /// 대시 애니메이션 재생
    /// </summary>
    private void PlayDashAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }
    }

    /// <summary>
    /// 대시 사운드 재생
    /// </summary>
    private void PlayDashSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDashSFX();
        }
    }
    /// <summary>
    /// 대시 중 이동 처리
    /// </summary>
    public void HandleDashMovement()
    {
        rb.velocity = dashDirection * dashSpeed;
    }

    /// <summary>
    /// 대시 종료
    /// </summary>
    public void EndDash()
    {
        //잔상 효과 종료
        if (dashAfterImage != null)
        {
            dashAfterImage.StopDash();
        }
    }
    #endregion
    #region Attack
    /// <summary>
    /// 공격 가능 여부
    /// </summary>
    public bool CanAttack()
    {
        return playerWeapon != null && playerWeapon.CanAttack();
    }

    /// <summary>
    /// 공격 실행
    /// </summary>
    public void PerformAttack()
    {
        if (playerWeapon != null)
        {
            playerWeapon.Attack();
        }
    }
    #endregion
    #region Input Control
    /// <summary>
    /// 입력 비활성화
    /// </summary>
    public void DisableInput()
    {
        inputEnabled = false;
        ResetInput();
    }
    /// <summary>
    /// 입력 활성화
    /// </summary>
    public void EnableInput()
    {
        inputEnabled = true;
    }
    /// <summary>
    /// 입력 상태 초기화
    /// </summary>
    private void ResetInput()
    {
        moveInput = Vector2.zero;
        dashInput = false;
        attackInput = false;
    }
    #endregion
    #region Debug
    //디버그 용 : 이동방향 표시
    void OnDrawGizmos()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            // 플레이어 → 마우스 방향 선 그리기
            DrawMouseDirectionLine();
            //대시 쿨다운
            DrawDashCooldownIndicator();
        }
    }
    /// <summary>
    /// 마우스 방향 선 그리기
    /// </summary>
    private void DrawMouseDirectionLine()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, mousePos);
    }

    /// <summary>
    /// 대시 쿨다운 표시
    /// </summary>
    private void DrawDashCooldownIndicator()
    {
        if (dashCooldownTimer > 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
    #endregion
}
