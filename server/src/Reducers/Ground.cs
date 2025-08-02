using SpacetimeDB;

namespace masks.server.Reducers;

public partial class Ground
{
    [Reducer]
    public static void GenerateGround(ReducerContext ctx)
    {
        var world = ctx.Db.World.Iter().FirstOrDefault();

        if (!world.IsGenerated)
        {
            GenerateTiles(ctx);
            world.IsGenerated = true;
            ctx.Db.World.Id.Update(world);
        }
    }

    private static void GenerateTiles(ReducerContext ctx)
    {
        int width = 200;
        // int height = 100; 

        int halfWidth = width / 2;

        for (int x = -halfWidth; x < width - halfWidth; x++)
        {
            for (int y = -100; y <= -5; y++)
            {
                ctx.Db.Ground.Insert(new Tables.Ground { X = x, Y = y });
            }
        }
    }

    [Reducer]
    public static void AddGroundTile(ReducerContext ctx, int x, int y)
    {
        ctx.Db.Ground.Insert(new Tables.Ground { X = x, Y = y });
    }

    [Reducer]
    public static void DeleteGroundTile(ReducerContext ctx, int x, int y)
    {
        var tile = ctx.Db.Ground.Iter().FirstOrDefault(t => t.X == x && t.Y == y);
        ctx.Db.Ground.Id.Delete(tile.Id);

        Log.Info($"Deleted ground tile at ({x}, {y}) with id {tile.Id}.");
    }
}