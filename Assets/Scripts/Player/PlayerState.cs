using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 상태 기반 추상클래스
/// </summary>
public abstract class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected PlayerController player;


    /// <summary>
    /// 생성자
    /// </summary>
    public PlayerState(PlayerStateMachine stateMachine, PlayerController player)
    {
        this.stateMachine = stateMachine;
        this.player = player;
    }
    /// <summary>
    /// 상태 진입 시 호출
    /// </summary>
    public virtual void Enter()
    {
        Debug.Log($"Enter State{this.GetType().Name}");
    }

    /// <summary>
    /// 매 프레임 업데이트
    /// </summary>
    public virtual void Update()
    {

    }

    /// <summary>
    /// 물리 업데이트
    /// </summary>
    public virtual void FixedUpdate()
    {
    }

    /// <summary>
    /// 상태 종료 시 호출
    /// </summary>
    public virtual void Exit()
    {
        Debug.Log($"Exit State{this.GetType().Name}");
    }

    /// <summary>
    /// 상태 전환 조건 체크
    /// </summary>
    public virtual void CheckTransitions()
    {
    }
}
