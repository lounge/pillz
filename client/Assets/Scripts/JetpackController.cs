using SpacetimeDB;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class JetpackController : MonoBehaviour
    {
        [SerializeField] private Component jetpack;
        [SerializeField] private Component flames;

        public float Fuel { get; private set; } = 100f;

        private bool _isEnabled;
        private bool _isThrottling;
        private float _refuelCooldownTimer;
        
        private const float MaxFuel = 100f;
        private const float BurnRate = 10f;      // Units per second
        private const float RefuelRate = 5f;     // Units per second
        private const float RefuelCooldown = 5f; // Seconds after last throttle off


        private void Awake()
        {
            flames?.gameObject.SetActive(false);
        }

        public void Enable()
        {
            if (Fuel <= 0f)
            {
                Log.Debug("Jetpack cannot be enabled, no fuel available.");
                return;
            }

            _isEnabled = true;
            gameObject.SetActive(true);
            jetpack?.gameObject.SetActive(true);
        }

        public void Disable()
        {
            _isEnabled = false;
            _isThrottling = false;

            jetpack?.gameObject.SetActive(false);
            flames?.gameObject.SetActive(false);
        }

        public void ThrottleOn()
        {
            if (_isThrottling || !_isEnabled || Fuel <= 0f)
                return;

            _isThrottling = true;
            flames?.gameObject.SetActive(true);

            _refuelCooldownTimer = RefuelCooldown;
        }

        public void ThrottleOff()
        {
            if (!_isThrottling)
                return;
            
            _isThrottling = false;
            flames?.gameObject.SetActive(false);
            
            _refuelCooldownTimer = RefuelCooldown;
        }

        private void FixedUpdate()
        {
            // Countdown cooldown timer
            if (!_isThrottling && _refuelCooldownTimer > 0f)
            {
                _refuelCooldownTimer -= Time.fixedDeltaTime;
                Log.Debug($"JetpackController: Countdown cooldown timer. Current value: {_refuelCooldownTimer}");
                
                if (_refuelCooldownTimer < 0f)
                    _refuelCooldownTimer = 0f;
            }

            // Burn fuel while throttling
            if (_isThrottling)
            {
                Fuel -= Time.fixedDeltaTime * BurnRate;
                if (Fuel <= 0f)
                {
                    Fuel = 0f;
                    _isThrottling = false;
                    Disable();

                    if (transform.parent.TryGetComponent(out PillController pill))
                    {
                        pill.OnJetpackDepleted();
                    }
                }
            }

            // Refuel only when cooldown has expired and not throttling
            if (!_isThrottling && _refuelCooldownTimer <= 0f && Fuel < MaxFuel)
            {
                Log.Debug($"JetpackController: Refueling. Current fuel: {Fuel}");
                Fuel += Time.fixedDeltaTime * RefuelRate;
                if (Fuel > MaxFuel)
                    Fuel = MaxFuel;
            }
        }
    }
}
