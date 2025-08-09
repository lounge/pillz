using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class JetpackController : MonoBehaviour
    {
        [SerializeField] private Component jetpack;
        [SerializeField] private Component flames;

        private float _fuel = 100f;
        private bool _isEnabled;
        private bool _isThrottling;
        private float _refuelCooldownTimer;
        private float _lastMovementSendTimestamp;
        private PillController _parentPill;
        private PillHud _pillHud;
        private JetpackInput _lastMovementInput;

        private const float MaxFuel = 100f;
        private const float BurnRate = 10f; // Units per second
        private const float RefuelRate = 5f; // Units per second
        private const float RefuelCooldown = 5f; // Seconds after last throttle off

        public void Init(PillController pillController, PillHud pillHud)
        {
            gameObject.SetActive(true);

            jetpack?.gameObject.SetActive(false);
            flames?.gameObject.SetActive(false);
            _parentPill = pillController;
            _pillHud = pillHud;

            _pillHud.SetFuel(_fuel);
        }

        public void OnJetpackUpdated(bool isEnabled, bool isThrottling, float jetpackFuel)
        {
            Log.Debug(
                $"JetpackController: OnJetpackUpdated called with isEnabled={isEnabled}, isThrottling={isThrottling} fuel={jetpackFuel}");
            jetpack.gameObject.SetActive(isEnabled);
            flames?.gameObject.SetActive(isThrottling);

            _pillHud.SetFuel(jetpackFuel);
        }

        public void Enable()
        {
            if (_fuel <= 0f)
            {
                Log.Debug("Jetpack cannot be enabled, no fuel available.");
                return;
            }

            _isEnabled = true;
            Log.Debug("Jetpack has been enabled.");
        }

        public void Disable()
        {
            _isEnabled = false;
            _isThrottling = false;
            Log.Debug("Jetpack has been disabled.");
        }

        public void ThrottleOn()
        {
            if (_isThrottling || !_isEnabled || _fuel <= 0f)
                return;

            _isThrottling = true;
            _refuelCooldownTimer = RefuelCooldown;
        }

        public void ThrottleOff()
        {
            if (!_isThrottling)
                return;

            _isThrottling = false;
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
                _fuel -= Time.fixedDeltaTime * BurnRate;
                if (_fuel <= 0f)
                {
                    _fuel = 0f;
                    _isThrottling = false;
                    Disable();

                    if (_parentPill)
                    {
                        _parentPill.OnJetpackDepleted();
                    }
                }
            }

            // Refuel only when cooldown has expired and not throttling
            if (!_isThrottling && _refuelCooldownTimer <= 0f && _fuel < MaxFuel)
            {
                Log.Debug($"JetpackController: Refueling. Current fuel: {_fuel}");
                _fuel += Time.fixedDeltaTime * RefuelRate;
                if (_fuel > MaxFuel)
                    _fuel = MaxFuel;
            }

            var jetpackInput = new JetpackInput(_fuel, _isEnabled, _isThrottling);
            if (Time.time - _lastMovementSendTimestamp >= EntityController.SendUpdatesFrequency &&
                !jetpackInput.Equals(_lastMovementInput))
            {
                GameHandler.Connection.Reducers.UpdateJetpack(jetpackInput);
            }

            _lastMovementInput = jetpackInput;
        }
    }
}