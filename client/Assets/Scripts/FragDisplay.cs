using TMPro;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class FragDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fragsText;

        public void SetFrags(float frags)
        {
            if (fragsText)
                fragsText.text = $"FRAGS {frags:0.}";        }
        
    }
}