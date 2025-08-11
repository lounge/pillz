using TMPro;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fragsText;
        [SerializeField] private TextMeshProUGUI dmgText;

        public void SetDmg(float dmg)
        {
            if (dmgText)
                dmgText.text = $"DMG {dmg}";
        }
        
        public void SetFrags(float frags)
        {
            if (fragsText)
                fragsText.text = $"FRAGS {frags}";
        }
    }
}