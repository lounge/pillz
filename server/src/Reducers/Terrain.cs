using SpacetimeDB;
using DbVector2 = pillz.server.Tables.DbVector2;
using Portal = pillz.server.Tables.Portal;
using SpawnLocation = pillz.server.Tables.SpawnLocation;

namespace pillz.server.Reducers;

public partial class Terrain
{
    [Reducer]
    public static void AddTerrainTile(ReducerContext ctx, int x, int y)
    {
        ctx.Db.Terrain.Insert(new pillz.server.Tables.Terrain
        {
            Position = new DbVector2(x, y)
        });
    }

    [Reducer]
    public static void DeleteTerrainTile(ReducerContext ctx, int x, int y)
    {
        var tile = ctx.Db.Terrain.Iter().FirstOrDefault(t => (int)t.Position.X == x && (int)t.Position.Y == y);
        ctx.Db.Terrain.Id.Delete(tile.Id);

        Log.Info($"Deleted terrain tile at ({x}, {y}) with id {tile.Id}.");
    }
    
    [Reducer]
    public static void DeleteTerrainTiles(ReducerContext ctx, float radiu)
    {
    }

    [Reducer]
    public static void GenerateTerrain(ReducerContext ctx, int seed)
    {
        var world = ctx.Db.World.Iter().FirstOrDefault();

        if (!world.IsGenerated)
        {
            GenerateTiles(ctx, seed);
            world.IsGenerated = true;
            ctx.Db.World.Id.Update(world);
        }
    }

    private static void GenerateTiles(ReducerContext ctx, int seed)
    {
        Log.Debug($"[GenerateTiles] Generating terrain with seed {seed}...");

        const int width = 300;
        const int height = 200;
        const int margin = 5;
        const int seedCount = 200;
        const int growthIterations = 4;
        const int connectThreshold = 20;

        var halfWidth = width / 2;
        var halfHeight = height / 2;

        var rng = new Random(seed);
        var  terrain = new bool[width, height];

        // STEP 1: Natural base layer with height jitter and sine waves
        var  terrainHeights = BaseLayer(width, rng,  terrain);

        // STEP 2: Place sparse random seed pixels for tunnels
        var seeds = GenerateRandomSeeds(seedCount, rng, margin, width, height,  terrain);

        // STEP 3: MST to connect all seeds with Bresenham
        ConnectSeeds(seeds,  terrain);

        // STEP 4: Tunnel drops to base only if near base
        ConnectToGround(seeds,  terrainHeights, connectThreshold,  terrain);

        // STEP 5: Growth iterations with 8-way neighbors and bias
        GrowthGeneration(growthIterations, width, height,  terrain, rng);

        // STEP 6: Write to database (centered)
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if ( terrain[x, y])
                {
                    ctx.Db.Terrain.Insert(new pillz.server.Tables.Terrain
                    {
                        Position = new DbVector2(x - halfWidth, y - halfHeight)
                    });
                }
            }
        }

        // STEP 7: Spawn and Portal locations
        GenerateSpawnLocations(ctx, width, height,  terrain, halfWidth, halfHeight);
        GeneratePortalLocations(ctx, width, height,  terrain, halfWidth, halfHeight);
    }
    
    private static void GenerateSpawnLocations(ReducerContext ctx, int width, int height, bool[,]  terrain, int halfWidth,
        int halfHeight)
    {
        for (var x = 2; x < width - 2; x++)
        {
            for (var y = 2; y < height - 3; y++) // ensure we can check y+3 safely
            {
                // This tile and the three above must be empty
                if ( terrain[x, y] ||  terrain[x, y + 1] ||  terrain[x, y + 2] ||  terrain[x, y + 3])
                    continue;

                // Ground below
                if (! terrain[x, y - 1]) continue;

                var gx = x - halfWidth;
                var gy = y - halfHeight;
                var pos = new DbVector2(gx, gy);

                ctx.Db.SpawnLocation.Insert(new SpawnLocation { Position = pos });
            }
        }
    }
    
    private static void GeneratePortalLocations(ReducerContext ctx, int width, int height, bool[,]  terrain, 
        int halfWidth, int halfHeight)
    {
        float minAllowedY = -40f;
        
        (float x, float y)? highest = null;
        (float x, float y)? lowest = null;
        (float x, float y)? leftest = null;
        (float x, float y)? rightest = null;

        
        for (var x = 1; x < width - 2; x++) // leave 1 tile margin left/right
        {
            for (var y = 2; y < height - 5; y++) // leave space above for 3-tile portal, and 2 tiles below for checks
            {
                var spaceClear =
                    ! terrain[x, y]     && ! terrain[x + 1, y] &&
                    ! terrain[x, y + 1] && ! terrain[x + 1, y + 1] &&
                    ! terrain[x, y + 2] && ! terrain[x + 1, y + 2];

                if (!spaceClear)
                    continue;

                // Ensure solid  terrain directly below either side
                var hasGroundBelow =  terrain[x, y - 1] ||  terrain[x + 1, y - 1];
                if (!hasGroundBelow)
                    continue;

                var gx = x - halfWidth;
                var gy = y - halfHeight;
                
                if (lowest == null || gy < lowest.Value.y)
                    lowest = (gx, gy);
                if (highest == null || gy > highest.Value.y) 
                    highest = (gx, gy); 
                
                if (gy >= minAllowedY)
                {
                    if (leftest == null || gx < leftest.Value.x)
                        leftest = (gx, gy);
                    if (rightest == null || gx > rightest.Value.x)
                        rightest = (gx, gy);
                }
            }
        }

        // Portals (adjust vertical placement to match visual center of portal)
        if (lowest != null && highest != null && leftest != null && rightest != null)
        {
            var portal1 = ctx.Db.Portal.Insert(new Portal
            {
                Position = new DbVector2(lowest.Value.x, lowest.Value.y)
            });

            var portal2 = ctx.Db.Portal.Insert(new Portal
            {
                Position = new DbVector2(highest.Value.x, highest.Value.y),
                ConnectedPortalId = portal1.Id
            });
            
            var portal3 = ctx.Db.Portal.Insert(new Portal
            {
                Position = new DbVector2(leftest.Value.x, leftest.Value.y),
                ConnectedPortalId = portal2.Id
            });
            
            var portal4 = ctx.Db.Portal.Insert(new Portal
            {
                Position = new DbVector2(rightest.Value.x, rightest.Value.y),
                ConnectedPortalId = portal3.Id
            });

            portal1.ConnectedPortalId = portal2.Id;
            ctx.Db.Portal.Id.Update(portal1);
            
            portal3.ConnectedPortalId = portal4.Id;
            ctx.Db.Portal.Id.Update(portal3);
        }
    }


    private static void ConnectToGround(List<(int x, int y)> seeds, int[]  terrainHeights, int connectThreshold,
        bool[,]  terrain)
    {
        foreach (var (x, y) in seeds)
        {
            var baseY =  terrainHeights[x];
            if (y - baseY <= connectThreshold)
            {
                for (var yy = y; yy >= baseY; yy--)
                     terrain[x, yy] = true;
            }
        }
    }

    private static void GrowthGeneration(int growthIterations, int width, int height, bool[,]  terrain, Random rng)
    {
        for (var i = 0; i < growthIterations; i++)
        {
            var growList = new List<(int x, int y)>();

            for (var x = 1; x < width - 1; x++)
            {
                for (var y = 1; y < height - 1; y++)
                {
                    if ( terrain[x, y]) continue;

                    var count = 0;
                    for (var dx = -1; dx <= 1; dx++)
                    {
                        for (var dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            if ( terrain[x + dx, y + dy]) count++;
                        }
                    }

                    // Randomness makes tunnels more varied
                    var bias = rng.Next(0, 3);

                    if (count + bias >= 5)
                    {
                        growList.Add((x, y));

                        if (count >= 6 && rng.NextDouble() < 0.75)
                        {
                            // Grow extra to add thickness
                            for (var dx = -1; dx <= 1; dx++)
                            for (var dy = -1; dy <= 1; dy++)
                                if (x + dx > 0 && y + dy > 0 && x + dx < width && y + dy < height)
                                    growList.Add((x + dx, y + dy));
                        }
                    }
                }
            }

            foreach (var (x, y) in growList)
            {
                 terrain[x, y] = true;
            }
        }
    }

    private static void ConnectSeeds(List<(int x, int y)> seeds, bool[,]  terrain)
    {
        var connected = new List<(int x, int y)> { seeds[0] };
        var remaining = new List<(int x, int y)>(seeds.Skip(1));

        while (remaining.Count > 0)
        {
            double minDist = double.MaxValue;
            (int x1, int y1) = (0, 0);
            (int x2, int y2) = (0, 0);

            foreach (var a in connected)
            {
                foreach (var b in remaining)
                {
                    double dist = Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        x1 = a.x;
                        y1 = a.y;
                        x2 = b.x;
                        y2 = b.y;
                    }
                }
            }

            foreach (var (x, y) in BresenhamLine(x1, y1, x2, y2))
            {
                 terrain[x, y] = true;
            }

            connected.Add((x2, y2));
            remaining.Remove((x2, y2));
        }
    }

    private static List<(int x, int y)> GenerateRandomSeeds(int seedCount, Random rng, int margin, int width,
        int height, bool[,]  terrain)
    {
        var seeds = new List<(int x, int y)>();
        for (var i = 0; i < seedCount; i++)
        {
            var x = rng.Next(margin, width - margin);
            var y = rng.Next(15, height - margin);

            if (! terrain[x, y])
            {
                 terrain[x, y] = true;
                seeds.Add((x, y));

                // Add chunk
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (rng.NextDouble() < 0.8)
                             terrain[x + dx, y + dy] = true;
                    }
                }
            }
        }

        return seeds;
    }

    private static int[] BaseLayer(int width, Random rng, bool[,]  terrain)
    {
        var  terrainHeights = new int[width];
        for (var x = 0; x < width; x++)
        {
            var noise = Math.Sin((x + rng.NextDouble()) * 0.1);
            var h = 8 + (int)(noise * 4) + rng.Next(2); // more organic variation
             terrainHeights[x] = h;

            for (var y = 0; y < h; y++)
            {
                 terrain[x, y] = true;
            }
        }

        return  terrainHeights;
    }

    private static IEnumerable<(int x, int y)> BresenhamLine(int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            yield return (x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}