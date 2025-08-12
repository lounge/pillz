using pillz.server.Tables;
using SpacetimeDB;

namespace pillz.server.Reducers;

public partial class Items
{
    [Reducer]
    public static void UpdateAmmo(ReducerContext ctx, uint id, DbVector2 position)
    {
        var ammo = ctx.Db.Entity.Id.Find(id);
        if (ammo != null)
        {
            ammo = ammo.Value with { Position = position };
            ctx.Db.Entity.Id.Update(ammo.Value);
        }
    }
    
    [Reducer]
    public static void DeleteAmmo(ReducerContext ctx, uint id)
    {
        var ammo = ctx.Db.Ammo.EntityId.Find(id) ?? throw new Exception("Ammo not found");

        ctx.Db.Entity.Id.Delete(id);
        ctx.Db.Ammo.EntityId.Delete(ammo.EntityId);
    }
}