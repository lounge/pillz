using UnityEngine;

namespace pillz.client.Scripts
{
    public class PortalState : MonoBehaviour
    {
        [SerializeField] private float portalCooldown = 0.35f;
        
        private bool _inPortal;
        private float _cooldown;

        public bool CanTrigger => _cooldown <= 0f && !_inPortal;

        public void OnTeleported()
        {
            _inPortal = true;
            _cooldown = portalCooldown;
        }

        public void OnPortalExit()
        {
            _inPortal = false;
        }

        public void Tick()
        {
            if (_cooldown > 0f)
            {
                _cooldown -= Time.deltaTime;
                if (_cooldown < 0f) _cooldown = 0f;
            }

            if (_inPortal && _cooldown == 0f)
                _inPortal = false;
        }
    }
}