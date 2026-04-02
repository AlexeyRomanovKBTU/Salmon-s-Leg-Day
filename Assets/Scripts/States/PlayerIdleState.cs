using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}
    public override void EnterState() => Ctx.anim.Play("Idle");
    public override void UpdateState() {}
    public override void ExitState() {}
    public override void CheckSwitchStates()
    {
        if (Ctx.input.RagdollTriggered) Ctx.SwitchState(Factory.Ragdoll());
        else if (Ctx.input.IsHoldingJump && (Ctx.isGrounded || Ctx.coyoteCounter > 0)) Ctx.SwitchState(Factory.Jump());
        else if (Ctx.input.MoveInput.x != 0 && Ctx.isGrounded) Ctx.SwitchState(Factory.Walk());
    }
}
