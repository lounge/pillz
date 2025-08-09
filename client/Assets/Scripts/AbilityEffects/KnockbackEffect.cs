using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts.AbilityEffects
{
    public class KnockbackEffect : AbilityEffect
    {
        public float Force { get; set; }
        
        public float ExplosionRadius { get; set; }

        public override void Execute(uint playerId, Rigidbody2D target, Vector2 contactPoint)
        {
            Log.Debug("AbilityEffect: Executing knockback effect with force: " + Force);
            
            var direction = target.position - contactPoint;
            var distance = direction.magnitude;
            
            // Ignore if outside of explosion range
            if (distance > ExplosionRadius)
                return;
            
            // Normalize direction vector
            direction.Normalize();
            
            // Invert falloff: closer = stronger force
            var falloff = 1f - (distance / ExplosionRadius);
            var f = Force * falloff;
            
            // return body.AddForce(, ForceMode2D.Impulse);
            
            var force =   direction * f;
            
            GameHandler.Connection.Reducers.ApplyForce(playerId, new DbVector2(force.x, force.y));

           
        }
    }
}