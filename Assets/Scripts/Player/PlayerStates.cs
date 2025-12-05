using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Idle 상태 - 정지
/// </summary>
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerStateMachine stateMachine, PlayerController player)
       : base(stateMachine, player) { }

    public override void Enter()
    {
        base.Enter();
        player.StopMovement();
    }
    public override void CheckTransitions()
    {
        //이동 입력 시 Move 상태
        if (player.HasMoveInput)
        {
            stateMachine.ChangeState(stateMachine.MoveState);
        }
        //대시 입력
        else if (player.IsDashInput && player.CanDash())
        {
            stateMachine.ChangeState(stateMachine.DashState);
        }
        //공격 입력
        else if (player.IsAttackInput && player.CanAttack())
        {
            stateMachine.ChangeState(stateMachine.AttackState);
        }
    }
}

/// <summary>
/// Move 상태 - 이동
/// </summary>
public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player) { }

    public override void FixedUpdate()
    {
        player.HandleMovement();
    }

    public override void Update()
    {
        player.HandleRotation();
    }

    public override void CheckTransitions()
    {
        // 이동 입력이 없으면 Idle로
        if (!player.HasMoveInput)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
        // 대시 입력
        else if (player.IsDashInput && player.CanDash())
        {
            stateMachine.ChangeState(stateMachine.DashState);
        }
        // 공격 입력
        else if (player.IsAttackInput && player.CanAttack())
        {
            stateMachine.ChangeState(stateMachine.AttackState);
        }
    }
}

/// <summary>
/// Dash 상태 - 대시
/// </summary>
public class PlayerDashState : PlayerState
{
    private float dashTimer;

    public PlayerDashState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player) { }

    public override void Enter()
    {
        base.Enter();
        player.StartDash();
        dashTimer = player.DashDuration;
    }

    public override void Update()
    {
        dashTimer -= Time.deltaTime;
    }

    public override void FixedUpdate()
    {
        player.HandleDashMovement();
    }

    public override void CheckTransitions()
    {
        // 대시 시간이 끝나면
        if (dashTimer <= 0)
        {
            // 이동 입력이 있으면 Move, 없으면 Idle
            if (player.HasMoveInput)
            {
                stateMachine.ChangeState(stateMachine.MoveState);
            }
            else
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.EndDash();
    }
}

/// <summary>
/// Attack 상태 - 공격
/// </summary>
public class PlayerAttackState : PlayerState
{
    private float attackTimer;

    public PlayerAttackState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player) { }

    public override void Enter()
    {
        base.Enter();
        player.PerformAttack();
        attackTimer = 0.3f; // 공격 애니메이션 시간
    }

    public override void Update()
    {
        attackTimer -= Time.deltaTime;

        // 공격 중에도 회전 가능
        player.HandleRotation();
    }

    public override void CheckTransitions()
    {
        // 공격 애니메이션이 끝나면
        if (attackTimer <= 0)
        {
            if (player.HasMoveInput)
            {
                stateMachine.ChangeState(stateMachine.MoveState);
            }
            else
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
        }
    }
}

/// <summary>
/// Dead 상태 - 사망
/// </summary>
public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(PlayerStateMachine stateMachine, PlayerController player)
        : base(stateMachine, player) { }

    public override void Enter()
    {
        base.Enter();
        player.StopMovement();
        player.DisableInput();

        // 사망 처리
        Debug.Log("Player Dead!");
    }

    public override void CheckTransitions()
    {
        // 사망 상태에서는 전환 없음
    }
}
