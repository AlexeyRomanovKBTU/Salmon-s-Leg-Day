using UnityEngine;

public class PlayerRagdollState : PlayerBaseState
{
    private float _settleTimer;

    public PlayerRagdollState(PlayerController ctx, PlayerStateFactory factory) : base(ctx, factory) {}

    public override void EnterState()
    {
        _settleTimer = Ctx.ragdollWait;

        Ctx.anim.Play("Ragdoll");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.eugh);

        Ctx.input.ResetRagdollTrigger();
    }

    public override void UpdateState()
    {
        _settleTimer -= Time.deltaTime;
    }

    public override void ExitState()
    {
        Ctx.ExitRagdoll();
    }

    public override void CheckSwitchStates()
    {
        if (_settleTimer <= 0)
            Ctx.SwitchState(Factory.Idle());
    }
}