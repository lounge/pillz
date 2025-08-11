using TMPro;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class AmmoDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI primaryAmmoText;
        [SerializeField] private TextMeshProUGUI secondaryAmmoText;

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