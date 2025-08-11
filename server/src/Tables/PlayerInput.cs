namespace pillz.server.Tables;

[SpacetimeDB.Type]
public partial struct PlayerInput
{
    public DbVector2 Direction;
    public DbVector2 Position;
    public bool IsPaused;
    public WeaponType SelectedWeapon;

    public PlayerInput(DbVector2 direction, DbVector2 position, bool isPaused, WeaponType selectedWeapon)
    {
        Direction = direction;
        Position = position;
        IsPaused = isPaused;
        SelectedWeapon = selectedWeapon;
    }
}