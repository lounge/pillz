using SpacetimeDB;

namespace masks.server.Tables;

[Table(Public = true)]
public partial struct Portal
{
    [PrimaryKey, AutoInc]
    public uint Id;
    
    public uint ConnectedPortalId;
    
    public DbVector2 Position;
}