using UnityEngine;

namespace pillz.client.Scripts.AbilityEffects
{
    public class DamageEffect : AbilityEffect
    {
        public uint MaxDamage { get; set; }

        public override void Execute(uint playerId, Rigidbody2D target, in ExplosionHit hit)
        {
            // Scale damage by proximity (at least 1)
            var dmg = Mathf.Max(1, Mathf.RoundToInt(MaxDamage * hit.Falloff));
            GameInit.Connection.Reducers.ApplyDamage(playerId, dmg);
        }
    }
}