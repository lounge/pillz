using System;
using SpacetimeDB;
using UnityEngine;
namespace pillz.client.Scripts
{
    public class JetpackController : MonoBehaviour
    {
        [SerializeField] private Component jetpack;
        [SerializeField] private Component flames;
        
        public bool IsEnabled { get; private set; }
        public bool IsThrottling { get; private set; }
        public float Fuel { get; private set; } = 100f;

        private float _refuelCooldownTimer;
        private PillController _parentPill;

        private const float MaxFuel = 100f;
        private const float BurnRate = 10f;      // Units per second
        private const float RefuelRate = 5f;     // Units per second
        private const float RefuelCooldown = 5f; // Seconds after last throttle off

        private void Awake()
        {
            flames?.gameObject.SetActive(false);
            if (transform.parent.TryGetComponent(out PillController pill))
            {
                _parentPill = pill;
            }
        }

        public void SetVisuals(bool isEnabled, bool isThrottling)
        {
            gameObject.SetActive(true);
            jetpack.gameObject.SetActive(isEnabled);
            flames?.gameObject.SetActive(isThrottling);
        }

        public void Enable()
        {
            if (Fuel <= 0f)
            {
                Log.Debug("Jetpack cannot be enabled, no fuel available.");
                return;
            }

            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
            IsThrottling = false;
        }

        public void ThrottleOn()
        {
            if (IsThrottling || !IsEnabled || Fuel <= 0f)
                return;

            IsThrottling = true;
            _refuelCooldownTimer = RefuelCooldown;
        }

        public void ThrottleOff()
        {
            if (!IsThrottling)
                return;
            
            IsThrottling = false;
            _refuelCooldownTimer = RefuelCooldown;
        }

        private void FixedUpdate()
        {
            // Countdown cooldown timer
            if (!IsThrottling && _refuelCooldownTimer > 0f)
            {
                _refuelCooldownTimer -= Time.fixedDeltaTime;
                Log.Debug($"JetpackController: Countdown cooldown timer. Current value: {_refuelCooldownTimer}");
                
                if (_refuelCooldownTimer < 0f)
                    _refuelCooldownTimer = 0f;
            }

            // Burn fuel while throttling
            if (IsThrottling)
            {
                Fuel -= Time.fixedDeltaTime * BurnRate;
                if (Fuel <= 0f)
                {
                    Fuel = 0f;
                    IsThrottling = false;
                    Disable();

                    if (_parentPill)
                    {
                        _parentPill.OnJetpackDepleted();
                    }
                }
            }

            // Refuel only when cooldown has expired and not throttling
            if (!IsThrottling && _refuelCooldownTimer <= 0f && Fuel < MaxFuel)
            {
                Log.Debug($"JetpackController: Refueling. Current fuel: {Fuel}");
                Fuel += Time.fixedDeltaTime * RefuelRate;
                if (Fuel > MaxFuel)
                    Fuel = MaxFuel;
            }
        }
    }
}
