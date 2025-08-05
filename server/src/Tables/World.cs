using SpacetimeDB;

namespace pillz.server.Tables;

[Table(Public = true)]
public partial struct World
{
    [PrimaryKey, AutoInc]
    public uint Id;

    public ulong Width;

    public ulong Height;

    public bool IsGenerated;
}