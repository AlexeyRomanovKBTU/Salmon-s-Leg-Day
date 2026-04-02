using UnityEngine;

public class PlayerStateFactory
{
    PlayerController _context;

    PlayerBaseState _idle;
    PlayerBaseState _walk;
    PlayerBaseState _jump;
    PlayerBaseState _ragdoll;

    public PlayerStateFactory(PlayerController currentContext)
    {
        _context = currentContext;
        _idle = new PlayerIdleState(_context, this);
        _walk = new PlayerWalkState(_context, this);
        _jump = new PlayerJumpState(_context, this);
        _ragdoll = new PlayerRagdollState(_context, this);
    }

    public PlayerBaseState Idle() => _idle;
    public PlayerBaseState Walk() => _walk;
    public PlayerBaseState Jump() => _jump;
    public PlayerBaseState Ragdoll() => _ragdoll;
}