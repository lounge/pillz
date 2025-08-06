using System;
using SpacetimeDB;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class JetpackController : MonoBehaviour
    {
        public Component jetpack;
        public Component flames;

        [NonSerialized] public float Fuel = 100f;
        private bool _isEnabled;

        private bool _throttling;


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
            jetpack.gameObject.SetActive(true);
        }

        public void Disable()
        {
            _isEnabled = false;
            _throttling = false;
            jetpack.gameObject.SetActive(false);
            flames.gameObject.SetActive(false);
        }

        public void ThrottleOn()
        {
            Log.Debug("JetpackController: ThrottleOn called.");
            if (!_isEnabled)
            {
                return;
            }

            _throttling = true;
            flames.gameObject.SetActive(true);
        }

        public void ThrottleOff()
        {
            _throttling = false;
            flames.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            Log.Debug($"JetpackController: FixedUpdate called. fuel={Fuel}, isEnabled={_isEnabled}, throttling={_throttling}");
            // each 10 seconds increase fuel by 1
            if (Fuel < 100f)
            {
                Log.Debug("JetpackController: Regenerating fuel.");
                Fuel += Time.fixedDeltaTime * 1f; // Adjust fuel regeneration rate
                if (Fuel > 100f)
                {
                    Fuel = 100f;
                }
            }
            
            if (Fuel <= 0f)
            {
                if (_isEnabled)
                {
                    Disable();

                    if (transform.parent.TryGetComponent(out PillController pill))
                    {
                        pill.OnJetpackDepleted();
                    }
                }

                return;
            }

            if (_throttling)
            {
                Fuel -= Time.fixedDeltaTime * 10f; // Adjust consumption rate
                if (Fuel < 0f)
                {
                    Fuel = 0f;
                }

                Debug.Log($"Jetpack Fuel: {Fuel}");
            }
        }
    }
}