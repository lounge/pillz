using System.Collections;
using UnityEngine;

namespace pillz.client.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class Positional2DAudio : MonoBehaviour
    {
        private AudioSource _src;
        private Transform _followTarget;
        private Coroutine _fadeCR;
        private float _baseVolume = 1f;

        private void Awake()
        {
            _src = GetComponent<AudioSource>();
            if (!_src)
            {
                _src = gameObject.AddComponent<AudioSource>();
            }

            _src.playOnAwake = false;
            _src.spatialBlend = 0f;
            _src.dopplerLevel = 0f;

            _baseVolume = _src.volume > 0f ? _src.volume : 1f;
        }

        private void LateUpdate()
        {
            if (_followTarget)
            {
                transform.position = _followTarget.position;
            }
        }

        /// <summary>Play a one-shot at this transform (non-positional UI etc.).</summary>
        public void PlayOneShot(AudioClip clip, float baseVolume = 1f)
        {
            if (!clip || !_src) return;
            _src.PlayOneShot(clip, baseVolume);
        }

        /// <summary>Play a one-shot at a world position.</summary>
        public void PlayOneShot(AudioClip clip, Vector3 worldPos, float baseVolume = 1f)
        {
            if (!clip || !_src) return;
            transform.position = worldPos;
            _src.PlayOneShot(clip, baseVolume);
        }

        /// <summary>
        /// Start (or update) a looping clip at a fixed world position.
        /// If the same clip is already looping, just updates volume/pitch unless retriggerIfSame = true.
        /// </summary>
        public void PlayLoop(AudioClip clip, Vector3 worldPos, float volume = 1f, float pitch = 1f,
            bool retriggerIfSame = false)
        {
            if (!clip || !_src) return;

            _followTarget = null;
            transform.position = worldPos;
            StartOrUpdateLoop(clip, volume, pitch, retriggerIfSame);
        }

        /// <summary>
        /// Start (or update) a looping clip that follows a Transform.
        /// If the same clip is already looping, just updates volume/pitch unless retriggerIfSame = true.
        /// </summary>
        public void PlayLoop(AudioClip clip, Transform follow, float volume = 1f, float pitch = 1f,
            bool retriggerIfSame = false)
        {
            if (!clip || !_src) return;

            _followTarget = follow;
            if (_followTarget) transform.position = _followTarget.position;
            StartOrUpdateLoop(clip, volume, pitch, retriggerIfSame);
        }

        /// <summary>
        /// Stop the current loop immediately or with a fade out.
        /// </summary>
        public void StopLoop(float fadeOutSeconds = 0f)
        {
            if (_src == null || !_src.loop || !_src.isPlaying)
            {
                _followTarget = null;
                return;
            }

            CancelFadeIfAny();
            if (fadeOutSeconds <= 0f)
            {
                _src.Stop();
                _src.loop = false;
                _src.clip = null;
                _src.volume = _baseVolume;
                _followTarget = null;
            }
            else
            {
                _fadeCR = StartCoroutine(FadeAndStop(fadeOutSeconds));
            }
        }

        // ----------------- Helpers -----------------

        private void StartOrUpdateLoop(AudioClip clip, float volume, float pitch, bool retriggerIfSame)
        {
            // If already looping same clip — just update params
            if (_src.isPlaying && _src.loop && _src.clip == clip && !retriggerIfSame)
            {
                _src.volume = volume;
                _src.pitch = pitch;
                return;
            }

            // (Re)start loop
            CancelFadeIfAny();
            _src.Stop();

            _src.clip = clip;
            _src.loop = true;
            _src.volume = volume;
            _src.pitch = pitch;
            _src.Play();
        }

        private void CancelFadeIfAny()
        {
            if (_fadeCR != null)
            {
                StopCoroutine(_fadeCR);
                _fadeCR = null;
                _src.volume = _baseVolume;
            }
        }

        private IEnumerator FadeAndStop(float seconds)
        {
            var start = _src.volume;
            var t = 0f;
            while (t < seconds && _src.loop && _src.isPlaying)
            {
                t += Time.unscaledDeltaTime;
                _src.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / seconds));
                yield return null;
            }

            _src.Stop();
            _src.loop = false;
            _src.clip = null;
            _src.volume = _baseVolume;
            _followTarget = null;
            _fadeCR = null;
        }
    }
}