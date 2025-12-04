using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 이동 및 회전 제어
/// 2025-12-03 (1일차) : 기본 이동, 마우스 방향 회전
/// 플레이어 대시
/// 2025-12-04 (2일차) : 대시 기능 추가
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxSpeed = 10f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("References")]
    private Rigidbody2D rb;
    private Camera mainCamera;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 mousePosition;

    //대시 관련
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //대시 중이 아닐 때만 입력 받음
        if (!isDashing)
        { 
            // 이동 입력 받기
            HandleInput();
            // 마우스 방향 회전
            HandleRotation();
        }

        //대시 쿨다운
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        //대시 타이머
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDash();
        }
        else
        {
            //이동 처리
            HandleMovement();
        }
            

        //최대 속도 제한
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

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

        //대시 입력 (스페이스바)
        if (Input.GetKeyDown(KeyCode.Space) && CanDash())
        {
            StartDash();
        }
    }

    /// <summary>
    /// 플레이어 이동
    /// </summary>
    private void HandleMovement()
    {
        rb.velocity = moveInput * moveSpeed;

        //애니메이션 파라미터 
        if (animator != null)
        {
            animator.SetFloat("Speed", rb.velocity.magnitude);
        }
    }

    /// <summary>
    /// 마우스 방향 회전
    /// </summary>
    
    private void HandleRotation()
    {
        //플레이어 -> 마우스 방향 벡터
        Vector2 direction = mousePosition - (Vector2)transform.position;

        //각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //회전 적용
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    /// <summary>
    /// 대시 가능 여부 확인
    /// </summary>
    private bool CanDash()
    {
        return !isDashing && dashCooldownTimer <= 0;
    }

    /// <summary>
    /// 대시 시작
    /// </summary>
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
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
    }

    /// <summary>
    /// 대시 중 이동 처리
    /// </summary>
    private void HandleDash()
    {
        rb.velocity = dashDirection * dashSpeed;
    }

    /// <summary>
    /// 대시 종료
    /// </summary>
    private void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero;
        Debug.Log("대시 종료");
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
