using SpacetimeDB;

namespace pillz.server.Tables;

[Table(Public = true)]
public partial struct Terrain {
    
    [PrimaryKey, AutoInc]
    public uint Id;
    
    public DbVector2 Position;
}