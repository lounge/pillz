using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts.ScriptableObjects.Pill
{
    [CreateAssetMenu(menuName = "Pill/WeaponDefinition")]
    public class WeaponDefinition : ScriptableObject
    {
        public WeaponType type;
        public GameObject controllerPrefab;
    }
}