using SpacetimeDB;

namespace pillz.server.Tables;

[Table(Public = true)]
public partial struct Ammo
{
    [PrimaryKey]
    public uint EntityId;
    public WeaponType AmmoType;
    public DbVector2 Position;
}