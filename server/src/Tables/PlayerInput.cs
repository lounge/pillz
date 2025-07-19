namespace masks.server.Tables;

[SpacetimeDB.Type]
public partial struct PlayerInput
{
    public DbVector2 Velocity;
    public bool IsPaused;
    public bool IsGrounded;
    
    public PlayerInput(DbVector2 velocity, bool isPaused, bool isGrounded)
    {
        Velocity = velocity;
        IsPaused = isPaused;
        IsGrounded = isGrounded;
    }


}