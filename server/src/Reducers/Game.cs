using masks.server.Timers;
using SpacetimeDB;

namespace masks.server.Reducers;

public partial class Game
{
    [Reducer]
    public static void MoveAllPlayers(ReducerContext ctx, MoveAllPlayersTimer timer)
    {
        
        var worldSize = (ctx.Db.World.Id.Find(0) ?? throw new Exception("Config table is empty. Please initialize the database first.")).Size;
     
        // Handle player input
        foreach (var mask in ctx.Db.Mask.Iter())
        {
            var checkEntity = ctx.Db.Entity.Id.Find(mask.EntityId);
            if (checkEntity == null)
            {
                // This can happen if the circle has been eaten by another circle.
                Log.Error($"Entity with id {mask.EntityId} not found in the database.");
                continue;
            }

            var maskEntity = checkEntity.Value;
            // var circleRadius = MassToRadius(circleEntity.Mass);
            var direction = mask.Direction;// * circle.Speed;
            var newPosition = maskEntity.Position + direction;// * MassToMaxMoveSpeed(circleEntity.Mass);
            //
            // Log.Debug($"Direction {direction}");
            // Log.Debug($"Position {maskEntity.Position}");
            // Log.Debug($"New position {newPosition}");
            //
            
            
            
            
            
            // Clamp x position to floor????
            maskEntity.Position.X = newPosition.X; //Math.Clamp(newPosition.X, circleRadius, worldSize - circleRadius);
            maskEntity.Position.Y = newPosition.Y; //Math.Clamp(newPosition.Y, circleRadius, worldSize - circleRadius);

            Log.Debug(
                $"Moving mask with id {mask.EntityId} to position ({maskEntity.Position.X}, {maskEntity.Position.Y})");
            
            // Check collisions
            // foreach (var entity in ctx.Db.Entity.Iter())
            // {
            //     if (entity.Id == maskEntity.Id)
            //     {
            //         continue; // Skip self
            //     }
            //
            //     // if (IsOverlapping(maskEntity, entity))
            //     // {
            //     //     // Check to see if we're overlapping with food
            //     //     if (ctx.Db.Food.EntityId.Find(entity.EntityId).HasValue)
            //     //     {
            //     //         ctx.Db.Entity.EntityId.Delete(entity.EntityId);
            //     //         ctx.Db.Food.EntityId.Delete(entity.EntityId);
            //     //         maskEntity.Mass += entity.Mass;
            //     //     }
            //     //
            //     //     // Check to see if we're overlapping with another player
            //     //     var otherCircle = ctx.Db.Circle.EntityId.Find(entity.EntityId);
            //     //     if (otherCircle.HasValue && otherCircle.Value.PlayerId != circle.PlayerId)
            //     //     {
            //     //         var massRatio = (float)entity.Mass / maskEntity.Mass;
            //     //         if (massRatio < MinimumSafeMassRatio)
            //     //         {
            //     //             // We can eat the other circle
            //     //             ctx.Db.Entity.EntityId.Delete(entity.EntityId);
            //     //             ctx.Db.Circle.EntityId.Delete(entity.EntityId);
            //     //             maskEntity.Mass += entity.Mass;
            //     //         }                       
            //     //     }
            //     // }
            // }

            ctx.Db.Entity.Id.Update(maskEntity);
        }
    }
}