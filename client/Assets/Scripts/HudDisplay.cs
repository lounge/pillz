using TMPro;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class HudDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fragsText;
        [SerializeField] private TextMeshProUGUI dmgText;
        [SerializeField] private TextMeshProUGUI stimText;
        [SerializeField] private TextMeshProUGUI primaryAmmoText;
        [SerializeField] private TextMeshProUGUI secondaryAmmoText;


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
        
        public void SetStim(int stim)
        {
            if (stimText)
                stimText.text = $"{stim}";
        }
        
        public void SetPrimary(int ammo)
        {
            if (primaryAmmoText)
                primaryAmmoText.text = $"{ammo}";
        }
        
        public void SetSecondary(int ammo)
        {
            if (secondaryAmmoText)
                secondaryAmmoText.text = $"{ammo}";
        }
    }
}