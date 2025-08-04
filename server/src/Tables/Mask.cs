using SpacetimeDB;

namespace masks.server.Tables;

[Table(Public = true)]
[SpacetimeDB.Index.BTree(Name = "PlayerId", Columns = [nameof(PlayerId)])]
public partial struct Mask
{
    [PrimaryKey]
    public uint EntityId;
    public uint PlayerId;
    public DbVector2 Direction;
    public DbVector2 Position;
    public bool IsGrounded;
    public uint Hp;
    public uint Dmg;
    public uint Frags;
    public DbVector2 AimDir;
}