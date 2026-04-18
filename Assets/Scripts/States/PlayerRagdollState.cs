using UnityEngine;

public class PlayerRagdollState : PlayerBaseState
{
    private float _settleTimer;
    private bool _hasHitGround;

    public PlayerRagdollState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState()
    {
        _hasHitGround = false;
        _settleTimer = Ctx.ragdollWait;

        // Disable IK — legs go fully dynamic and flop with everything else
        Ctx.EnterRagdoll();

        Ctx.anim.Play("Ragdoll");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.eugh);

        Ctx.input.ResetRagdollTrigger();
    }

    public override void UpdateState()
    {
        if (Ctx.isGrounded)
            _hasHitGround = true;

        if (_hasHitGround)
            _settleTimer -= Time.deltaTime;
    }

    public override void ExitState()
    {
        // Re-enable IK — legs return to kinematic and snap back under IK control
        Ctx.ExitRagdoll();
    }

    public override void CheckSwitchStates()
    {
        if (_hasHitGround && _settleTimer <= 0)
            Ctx.SwitchState(Factory.Idle());
    }
}