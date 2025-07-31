namespace masks.server.Tables;

[SpacetimeDB.Type]
public partial struct PlayerInput
{
    public DbVector2 Velocity;
    public DbVector2 Position;
    public bool IsPaused;
    
    public PlayerInput(DbVector2 velocity, DbVector2 position, bool isPaused)
    {
        Velocity = velocity;
        Position = position;
        IsPaused = isPaused;
    }


}