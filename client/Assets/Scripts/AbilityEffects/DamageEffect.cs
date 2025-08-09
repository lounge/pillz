using SpacetimeDB;
using UnityEngine;

namespace pillz.client.Scripts.AbilityEffects
{
    public class DamageEffect : AbilityEffect
    {
        public uint Damage { get; set; }

        public override void Execute(uint playerId, Rigidbody2D target, Vector2 contactPoint)
        {
            GameHandler.Connection.Reducers.ApplyDamage(playerId, Damage);

            Log.Debug("AbilityEffect: Executing damage effect with damage: " + Damage);
        }
    }
}