using SpacetimeDB;

namespace masks.server.Tables;

[Table(Public = true)]
[Table(Name = "LoggedOutPlayer")]
public partial struct Player
{
    [PrimaryKey]
    public Identity Identity;

    [Unique, AutoInc]
    public uint Id;
    public string Name;
}