using UnityEngine;

namespace pillz.client.Scripts.AbilityEffects
{
    public class DamageEffect : AbilityEffect
    {
        public uint MaxDamage { get; set; }

        public override void Execute(uint playerId, Rigidbody2D target, in ExplosionHit hit)
        {
            var dmg = Mathf.Max(1, Mathf.RoundToInt(MaxDamage * hit.Falloff));
            Game.Connection.Reducers.ApplyDamage(playerId, dmg);
        }
    }
}