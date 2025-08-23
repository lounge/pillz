using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts.AbilityEffects
{
    public class KnockbackEffect : AbilityEffect
    {
        public float MaxForce { get; set; } = 50f;

        public override void Execute(uint playerId, Rigidbody2D target, in ExplosionHit hit)
        {
            var impulse = hit.Direction * (MaxForce * hit.Falloff);
            Game.Connection.Reducers.ApplyForce(playerId, new DbVector2(impulse.x, impulse.y));
        }
    }
}