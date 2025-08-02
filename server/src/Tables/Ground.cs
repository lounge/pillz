using SpacetimeDB;

namespace masks.server.Tables;

[Table(Public = true)]
public partial struct Ground {
    
    [PrimaryKey, AutoInc]
    public uint Id;
    
    public  int X;
    public  int Y;
}