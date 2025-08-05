using System;
using SpacetimeDB;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class JetpackController : MonoBehaviour
    {
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
        }

        public void Disable()
        {
            _isEnabled = false;
            gameObject.SetActive(false);
            flames.gameObject.SetActive(false);
        }

        public void ThrottleOn()
        {
            if (!_isEnabled)
            {
                return;
            }

            _throttling = true;
            flames.gameObject.SetActive(true);
        }

        public void ThrottleOff()
        {
            flames.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
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