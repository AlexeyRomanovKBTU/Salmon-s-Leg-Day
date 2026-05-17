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
        Debug.Log($"[Ragdoll] timer: {_settleTimer:F2}");
    }

    public override void ExitState()
    {
        Debug.Log("[Ragdoll] ExitState called");
        Ctx.ExitRagdoll();
    }

    public override void CheckSwitchStates()
    {
        if (_settleTimer <= 0)
        {
            Debug.Log("[Ragdoll] Switching to Idle");
            Ctx.SwitchState(Factory.Idle());
        }
    }
}