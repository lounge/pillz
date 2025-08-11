using UnityEngine;

namespace pillz.client.Scripts.ScriptableObjects.Pill
{
    [CreateAssetMenu(menuName = "Pill/StimConfig")]
    public class StimConfig : ScriptableObject
    {
        public int amount = 4;
        public int strength = 40;
    }
}