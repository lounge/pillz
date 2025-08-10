using UnityEngine;

namespace pillz.client.Scripts.ScriptableObjects.Pill
{
    [CreateAssetMenu(menuName = "Pill/JetpackConfig")]
    public class JetpackConfig : ScriptableObject
    {
        public float maxFuel = 100f;
        public float burnRate = 10f;
        public float refuelRate = 5f; 
        public float refuelCooldown = 5f;
        public float force = 15f; 
    }
}