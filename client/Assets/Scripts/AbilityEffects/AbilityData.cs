using System.Collections.Generic;
using pillz.client.Scripts.Constants;
using UnityEngine;

namespace pillz.client.Scripts.AbilityEffects
{
    public class AbilityData
    {
        public List<AbilityEffect> Effects = new();

        /// <summary>
        /// Finds all pills within the radius and applies Effects to each using a shared ExplosionHit context.
        /// </summary>
        public void ApplyExplosionAt(
            Vector2 contactPoint,
            float radius,
            string pillTag = Tags.Pill)
        {
            var colliders = Physics2D.OverlapCircleAll(contactPoint, radius);
            foreach (var col in colliders)
            {
                if (!col || !col.CompareTag(pillTag))
                    continue;

                var pill = col.GetComponent<PillController>();
                var rb = col.attachedRigidbody;
                if (!pill || !rb)
                    continue;

                var dir = rb.position - contactPoint;
                var dist = dir.magnitude;
                if (dist <= Mathf.Epsilon)
                {
                    dir = Vector2.up;
                }
                else
                {
                    dir /= dist;
                }

                var falloff = Mathf.Clamp01(1f - (dist / radius));

                var hit = new ExplosionHit
                {
                    ContactPoint = contactPoint,
                    Radius = radius,
                    Distance = dist,
                    Falloff = falloff,
                    Direction = dir
                };

                // Fan out to effects
                foreach (var effect in Effects)
                {
                    effect.Execute(pill.Owner.PlayerId, rb, in hit);
                }
            }
        }
    }
}