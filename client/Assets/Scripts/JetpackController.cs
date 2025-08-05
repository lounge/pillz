using System;
using UnityEngine;

namespace masks.client.Scripts
{
    public class JetpackController : MonoBehaviour
    {
        public Component flames;

        private void Awake()
        {
            flames?.gameObject.SetActive(false);
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            flames.gameObject.SetActive(false);
        }

        public void ThrottleOn(float gas)
        {
            flames.gameObject.SetActive(true);
        }
        
        public void ThrottleOff()
        {
            flames.gameObject.SetActive(false);
        }
    }
}