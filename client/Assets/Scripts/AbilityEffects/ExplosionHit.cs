using UnityEngine;

namespace pillz.client.Scripts.AbilityEffects
{
    public struct ExplosionHit
    {
        public Vector2 ContactPoint;   // explosion center
        public float Radius;           // explosion radius
        public float Distance;         // distance(target, center)
        public float Falloff;          // 0..1 (closer = bigger)
        public Vector2 Direction;      // from center -> target (normalized)
    }

}