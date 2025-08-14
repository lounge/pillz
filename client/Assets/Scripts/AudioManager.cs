using UnityEngine;

namespace pillz.client.Scripts
{
    [DefaultExecutionOrder(-100)]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private Positional2DAudio _positional2DAudio;
        private AudioSource _audioSource;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = gameObject.GetComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f; // 2D
            _audioSource.dopplerLevel = 0f;

            _positional2DAudio = gameObject.GetComponent<Positional2DAudio>();
        }

        public void Play(AudioClip clip, Vector3 worldPos, float volume = 1f)
        {
            if (!clip) return;
            _positional2DAudio.PlayOneShot(clip, worldPos, volume);
        }

        public void PlayGlobal(AudioClip clip, float volume = 1f)
        {
            if (!clip) return;
            _positional2DAudio.PlayOneShot(clip, baseVolume: volume);
        }

        public void PlayLoop(AudioClip clip, Vector3 worldPos, float volume = 1f, float pitch = 1f,
            bool retriggerIfSame = false)
        {
            if (!clip) return;
            _positional2DAudio.PlayLoop(clip, worldPos, volume, pitch, retriggerIfSame);
        }

        public void PlayLoop(AudioClip clip, Transform follow, float volume = 1f, float pitch = 1f,
            bool retriggerIfSame = false)
        {
            if (!clip) return;
            _positional2DAudio.PlayLoop(clip, follow, volume, pitch, retriggerIfSame);
        }

        public void StopLoop(float fadeOutSeconds = 0f)
        {
            _positional2DAudio.StopLoop(fadeOutSeconds);
        }
    }
}