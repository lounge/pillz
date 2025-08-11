using pillz.server.Tables;
using SpacetimeDB;

namespace pillz.server.Reducers;

public partial class Items
{
    [Reducer]
    public static void UpdateAmmo(ReducerContext ctx, uint id, DbVector2 position)
    {
        var ammo = ctx.Db.Ammo.Id.Find(id);
        if (ammo != null)
        {
            ammo = ammo.Value with { Position = position };
            ctx.Db.Ammo.Id.Update(ammo.Value);
        }
    }
    
    [Reducer]
    public static void DeleteAmmo(ReducerContext ctx, uint id)
    {
        var ammo = ctx.Db.Ammo.Id.Find(id) ?? throw new Exception("Ammo not found");
        ctx.Db.Ammo.Id.Delete(ammo.Id);
    }
}