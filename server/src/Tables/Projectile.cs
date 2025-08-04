using SpacetimeDB;

namespace masks.server.Tables;

[Table(Public = true)]
[SpacetimeDB.Index.BTree(Name = "PlayerId", Columns = [nameof(PlayerId)])]
public partial struct Projectile
{
    [PrimaryKey]
    public uint EntityId;
    public uint PlayerId;
    public DbVector2 Direction;
    public DbVector2 Position;
    public float Speed;
}