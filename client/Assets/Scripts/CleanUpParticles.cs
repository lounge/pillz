using UnityEngine;

namespace pillz.client.Scripts
{
    public class CleanUpParticles : MonoBehaviour
    {
        private ParticleSystem _ps;
        public float destroyAfterSeconds = 0f;

        private void Start()
        {
            _ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (_ps && !_ps.IsAlive())
            {
                Destroy(gameObject, destroyAfterSeconds);
            }
        }
    }
}