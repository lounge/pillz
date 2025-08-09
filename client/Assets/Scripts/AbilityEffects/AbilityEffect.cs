using UnityEngine;

namespace pillz.client.Scripts.AbilityEffects
{
    public abstract class AbilityEffect
    {
        public abstract void Execute(uint playerId, Rigidbody2D target, in ExplosionHit hit);
    }
}