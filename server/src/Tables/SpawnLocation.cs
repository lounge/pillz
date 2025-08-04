
using SpacetimeDB;

namespace masks.server.Tables;

[Table(Public = true)]
public partial struct SpawnLocation
{
    [PrimaryKey, AutoInc]
    public uint Id;
    public DbVector2 Position;
}