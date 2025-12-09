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
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("References")]
    private Rigidbody2D rb;
    private Camera mainCamera;
    private Animator animator;
    private DashAfterImage dashAfterImage;
    private PlayerWeapon playerWeapon;
    private PlayerStateMachine stateMachine;

    private Vector2 moveInput;
    private Vector2 mousePosition;
    private bool dashInput;
    private bool attackInput;

    //대시 관련
    //private bool isDashing = false;
    //private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;

    //입력 활성화
    private bool inputEnabled = true;

    //상태에서 접근
    public Vector2 MoveInput => moveInput;
    public bool HasMoveInput => moveInput.magnitude > 0.1f;
    public bool IsDashInput => dashInput;
    public bool IsAttackInput => attackInput;
    public float DashDuration => dashDuration;
    public Vector2 DashDirection => dashDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        dashAfterImage = GetComponent<DashAfterImage>();
        playerWeapon = GetComponent<PlayerWeapon>();
        stateMachine = GetComponent<PlayerStateMachine>();
    }

    void Update()
    {
        if (!inputEnabled) return;

        //입력 받기
        HandleInput();

        //쿨다운 업데이트
        UpdateCooldowns();

        ////대시 중이 아닐 때만 입력 받음
        //if (!isDashing)
        //{ 
        //    // 이동 입력 받기
        //    HandleInput();
        //    // 마우스 방향 회전
        //    HandleRotation();
        //}

        ////대시 쿨다운
        //if (dashCooldownTimer > 0)
        //{
        //    dashCooldownTimer -= Time.deltaTime;
        //}

        ////대시 타이머
        //if (isDashing)
        //{
        //    dashTimer -= Time.deltaTime;
        //    if (dashTimer <= 0)
        //    {
        //        EndDash();
        //    }
        //}
    }

    //void FixedUpdate()
    //{
    //    if (isDashing)
    //    {
    //        HandleDash();
    //    }
    //    else
    //    {
    //        //이동 처리
    //        HandleMovement();
    //    }
            

    //    //최대 속도 제한
    //    if (rb.velocity.magnitude > maxSpeed)
    //    {
    //        rb.velocity = rb.velocity.normalized * maxSpeed;
    //    }
    //}

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
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // 대시 입력
        dashInput = Input.GetKeyDown(KeyCode.Space);

        //공격 입력
        attackInput = Input.GetMouseButtonDown(0);
    }
    ///<summary
    ///쿨다운 업데이트
    ///</summary>
    private void UpdateCooldowns()
    {
        if(dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 플레이어 이동
    /// </summary>
    public void HandleMovement()
    {
        rb.velocity = moveInput * moveSpeed;

        //애니메이션 파라미터 
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
        //플레이어 -> 마우스 방향 벡터
        Vector2 direction = mousePosition - (Vector2)transform.position;

        //각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //회전 적용
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    /// <summary>
    /// 이동 정지
    /// </summary>
    public void StopMovement()
    {
        rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

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
        //isDashing = true;
        //dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        //이동 중일때는 이동방향으로 아닐때는 마우스방향으로
        if (moveInput.magnitude > 0.1f)
        {
            dashDirection = moveInput;
        }
        else
        {
            dashDirection = (mousePosition - (Vector2)transform.position).normalized;
        }

        //잔상 효과 시작
        if (dashAfterImage != null)
        {
            dashAfterImage.StartDash();
            //Debug.Log("DashAfterimage.StartDash() 호출됨");
        }

        //애니메이션
        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }
        //Debug.Log("PlayerController.StartDash() 호출됨");
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
        //isDashing = false;
        //rb.velocity = Vector2.zero;

        //잔상 효과 종료
        if (dashAfterImage != null)
        {
            dashAfterImage.StopDash();
            //Debug.Log("DashAfterimage.StopDash() 호출됨");
        }
        //Debug.Log("PlayerController.EndDash() 호출됨");
    }

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

    /// <summary>
    /// 입력 비활성화
    /// </summary>
    public void DisableInput()
    {
        inputEnabled = false;
        moveInput = Vector2.zero;
        dashInput = false;
        attackInput = false;
    }

    /// <summary>
    /// 입력 활성화
    /// </summary>
    public void EnableInput()
    {
        inputEnabled = true;
    }

    //디버그 용 : 이동방향 표시
    void OnDrawGizmos()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            // 플레이어 → 마우스 방향 선 그리기
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, mousePos);
            //대시 쿨다운
            if(dashCooldownTimer > 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }
}
