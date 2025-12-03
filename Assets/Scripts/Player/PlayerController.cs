using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 이동 및 회전 제어
/// 2025-12-03 (1일차) : 기본 이동, 마우스 방향 회전
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    private Rigidbody2D rb;
    private Camera mainCamera;

    private Vector2 moveInput;
    private Vector2 mousePosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 이동 입력 받기
        HandleInput();
        // 마우스 방향 회전
        HandleRotation();
    }

    void FixedUpdate()
    {
        //이동 처리
        HandleMovement();
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
    }

    /// <summary>
    /// 플레이어 이동
    /// </summary>
    private void HandleMovement()
    {
        rb.velocity = moveInput * moveSpeed;
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

    //디버그 용 : 이동방향 표시
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)moveInput * 2f);
    }
}
