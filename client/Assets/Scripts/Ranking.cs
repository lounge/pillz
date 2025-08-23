using System;
using SpacetimeDB;

namespace pillz.client.Scripts
{
    public abstract class Ranking
    {
        public struct Weights
        {
            public readonly int FragPts;   
            public readonly int DamagePts; 
            public readonly int DeathPts;  
            public Weights(int fragPts, int damagePts, int deathPts)
                => (FragPts, DamagePts, DeathPts) = (fragPts, damagePts, deathPts);
        }

        public static Func<Stats, int> MakeScorer(Weights w)
        {
            return p =>
            {
                // Clamp to avoid overflow if needed:
                var score = Math.Clamp(w.FragPts * p.Frags + w.DamagePts * p.Dmg - w.DeathPts * p.Deaths, 0, int.MaxValue);
            
                Log.Debug($"Score for {p.Username}: score={score}");

                return score;
            };
        }
    }
}