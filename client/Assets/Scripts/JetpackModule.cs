using pillz.client.Scripts.ScriptableObjects.Pill;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class JetpackModule : MonoBehaviour
    {
        [SerializeField] private JetpackConfig config;
        [SerializeField] private GameObject visual;
        [SerializeField] private GameObject flames;

        public float Fuel { get; private set; }
        public bool Enabled { get; private set; }
        public bool Throttling { get; private set; }

        private float _cooldown;
        private float _lastMovementSendTimestamp;
        private JetpackInput _lastMovementInput;
        private PillHud _pillHud;

        public void Init(JetpackConfig cfg, PillHud pill)
        {
            config = cfg;
            _pillHud = pill;
            Fuel = cfg.maxFuel;
        }

        private void Awake()
        {
            if (flames) flames.SetActive(false);
            if (visual) visual.SetActive(false);
        }

        public void OnJetpackUpdated(Jetpack newValJetpack)
        {
            Log.Debug(
                $"JetpackController: OnJetpackUpdated called with isEnabled={newValJetpack.Enabled}, isThrottling={newValJetpack.Throttling} fuel={newValJetpack.Fuel}");
            visual.gameObject.SetActive(newValJetpack.Enabled);
            flames?.gameObject.SetActive(newValJetpack.Throttling);

            _pillHud.SetFuel(newValJetpack.Fuel);
        }

        public void Toggle()
        {
            if (!Enabled && Fuel <= 0f) 
                return;
            
            Enabled = !Enabled;
            if (visual)
            {
                visual.SetActive(Enabled);
            }

            if (!Enabled)
            {
                ThrottleOff();
            }
        }

        public void ThrottleOn()
        {
            if (!Enabled || Fuel <= 0f)
                return;
            
            Throttling = true;
            if (flames)
            {
                flames.SetActive(true);
            }
        }

        public void ThrottleOff()
        {
            if (!Throttling)
                return;
            
            Throttling = false;
            if (flames)
            {
                flames.SetActive(false);
            }
            _cooldown = config.refuelCooldown;
        }

        public void FixedTick()
        {
            // cooldown
            if (!Throttling && _cooldown > 0f)
                _cooldown = Mathf.Max(0f, _cooldown - Time.fixedDeltaTime);

            // burn
            if (Throttling)
            {
                Fuel = Mathf.Max(0f, Fuel - config.burnRate * Time.fixedDeltaTime);
                if (Fuel <= 0f)
                {
                    Fuel = 0f;
                    ThrottleOff();
                    Enabled = false;
                    if (visual) visual.SetActive(false);
                }
            }

            // refuel
            if (!Throttling && _cooldown <= 0f && Fuel < config.maxFuel)
                Fuel = Mathf.Min(config.maxFuel, Fuel + config.refuelRate * Time.fixedDeltaTime);

            var jetpackInput = new JetpackInput(Fuel, Enabled, Throttling);
            if (Time.time - _lastMovementSendTimestamp >= EntityController.SendUpdatesFrequency &&
                !jetpackInput.Equals(_lastMovementInput))
            {
                GameHandler.Connection.Reducers.UpdateJetpack(jetpackInput);
            }

            _lastMovementInput = jetpackInput;
        }
    }
}