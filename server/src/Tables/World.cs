using SpacetimeDB;

namespace masks.server.Tables;

[Table(Public = true)]
public partial struct World
{
    [PrimaryKey]
    public uint Id;

    public ulong Size;
}