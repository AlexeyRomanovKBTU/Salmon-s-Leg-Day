public class PlayerStateFactory
{
    private PlayerController _context;

    private PlayerBaseState _idle;
    private PlayerBaseState _walk;
    private PlayerBaseState _jump;
    private PlayerBaseState _ragdoll;

    public PlayerStateFactory(PlayerController currentContext)
    {
        _context = currentContext;
        _idle    = new PlayerIdleState(_context, this);
        _walk    = new PlayerWalkState(_context, this);
        _jump    = new PlayerJumpState(_context, this);
        _ragdoll = new PlayerRagdollState(_context, this);
    }

    public PlayerBaseState Idle()    => _idle;
    public PlayerBaseState Walk()    => _walk;
    public PlayerBaseState Jump()    => _jump;
    public PlayerBaseState Ragdoll() => _ragdoll;
}