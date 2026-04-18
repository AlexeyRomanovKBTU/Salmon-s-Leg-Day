using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerController Ctx;
    protected PlayerStateFactory Factory;

    public PlayerBaseState(PlayerController currentContext, PlayerStateFactory playerStateFactory)
    {
        Ctx = currentContext;
        Factory = playerStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
}