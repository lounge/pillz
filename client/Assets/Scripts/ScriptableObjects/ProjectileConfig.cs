using UnityEngine;

namespace pillz.client.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Weapon/ProjectileConfig")]
    public class ProjectileConfig : ScriptableObject
    {
       public GameObject explosionPrefab;
       public float maxForce = 50f;
       public uint maxDamage = 20;
       public float explosionRadius = 3.5f;
    }
}