using TMPro;
using UnityEngine;

namespace masks.client.Scripts
{
    public class HpDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0); // height above character

        private Transform _target;

        public void SetHp(uint hp)
        {
            if (hpText)
                hpText.text = $"{hp} HP";
        }

        public void AttachTo(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Follow target position with offset
            transform.position = _target.position + offset;
            transform.rotation = Quaternion.identity; // keep text upright
        }
    }
}