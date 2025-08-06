using TMPro;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class DmgDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dmgText;

        public void SetDmg(float dmg)
        {
            if (dmgText)
                dmgText.text = $"DMG {dmg}";
        }
    }
}