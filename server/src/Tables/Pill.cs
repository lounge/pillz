using SpacetimeDB;

namespace pillz.server.Tables;

[Table(Public = true)]
[SpacetimeDB.Index.BTree(Name = "PlayerId", Columns = [nameof(PlayerId)])]
public partial struct Pill
{
    [PrimaryKey]
    public uint EntityId;
    public uint PlayerId;
    public DbVector2 Direction;
    public DbVector2 Position;
    public int Hp;
    public int Dmg;
    public uint Frags;
    public Jetpack Jetpack;
    public DbVector2 AimDir;
    public DbVector2? Force;
    public WeaponType SelectedWeapon;
    public Weapon PrimaryWeapon;
    public Weapon SecondaryWeapon;
}