using SpacetimeDB;

namespace pillz.server.Tables;

[Table(Public = true)]
public partial struct Ammo
{
    [PrimaryKey, AutoInc]
    public uint Id;
    public WeaponType AmmoType;
    public DbVector2 Position;

}