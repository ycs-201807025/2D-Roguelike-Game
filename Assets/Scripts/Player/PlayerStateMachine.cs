using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 상태 머신
/// </summary>
public class PlayerStateMachine : MonoBehaviour
{
    //상태
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }

    private PlayerState currentState;
    private PlayerController playerController;

    [Header("Debug")]
    [SerializeField] private string currentStateName;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();

        //모든 상태 초기화
        IdleState = new PlayerIdleState(this, playerController);
        MoveState = new PlayerMoveState(this, playerController);
        DashState = new PlayerDashState(this, playerController);
        AttackState = new PlayerAttackState(this, playerController);
        DeadState = new PlayerDeadState(this, playerController);
    }

    void Start()
    {
        //초기 상태 설정
        ChangeState(IdleState);
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
            currentState.CheckTransitions();
        }
    }

    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    public void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;

        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();

        //디버그용 상태 이름
        currentStateName = currentState.GetType().Name;
    }

    /// <summary>
    /// 현재 상태 변환
    /// </summary>
    public PlayerState CurrentState => currentState;
}
