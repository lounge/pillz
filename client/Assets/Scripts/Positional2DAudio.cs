using System.Collections;
using UnityEngine;

namespace pillz.client.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class Positional2DAudio : MonoBehaviour
    {
        [Header("References")]
        public Transform listener;                 // Defaults to main camera

        [Header("Distance / Rolloff")]
        [Tooltip("Distance (world units) where the sound is full volume.")]
        public float minDistance = 1f;
        [Tooltip("Distance where the sound is fully faded out.")]
        public float maxDistance = 50f;

        // Volume rolloff curve: x = normalized distance (0..1), y = volume (0..1)
        public AnimationCurve distanceRolloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Stereo Panning")]
        [Tooltip("How strongly X affects left/right pan (-1..1 clamp applied).")]
        public float xPanStrength = 1f;
        [Tooltip("How strongly Y also contributes to pan (front/back cue into LR).")]
        public float yPanStrength = 0.35f;

        [Tooltip("World units at which X/Y cause max pan before clamping.")]
        public float panSaturationDistance = 6f;

        [Header("Height (Y) Loudness Bias")]
        [Tooltip("Extra volume bias for above/below. 0 = none, 1 = strong.")]
        public float yLoudnessBias; // >0 makes 'in front/above' a bit louder, 'behind/below' softer

        [Header("Optional Doppler (2D)")]
        public bool useSimpleDoppler = false;
        [Range(0f, 1f)] public float dopplerAmount = 0.25f;
        public float dopplerScale = 10f; // higher = stronger pitch change

        
        private Transform _followTarget;
        private Coroutine _fadeCR;

        // Multiplier you can tweak when starting loops (also used by fades).
        private float _userVolume = 1f;
        private float _userPitch  = 1f;
        private AudioSource _src;
        private Vector2 _lastRel;
        private float _lastTime;

        private void Awake()
        {
            _src = GetComponent<AudioSource>();
            if (!_src)
            {
                _src = gameObject.AddComponent<AudioSource>();
            }
            
            _src.playOnAwake = false;
            _src.loop = false;
            _src.spatialBlend = 0f;   // keep Unity in 2D; we do custom panning/rolloff
            _src.dopplerLevel = 0f;

            if (!listener && Camera.main)
            {
                listener = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            // Follow target for loops (cheap, no parenting required)
            if (_followTarget) transform.position = _followTarget.position;

            // If no listener, skip positional math
            if (!listener || !_src) return;

            // Relative pos in listener's space so rotation matters
            Vector3 rel3 = listener.InverseTransformPoint(transform.position);
            Vector2 rel = new Vector2(rel3.x, rel3.y);
            float dist = rel.magnitude;

            // -------- Volume (distance rolloff + Y bias) --------
            float t = Mathf.InverseLerp(minDistance, maxDistance, dist);
            float baseVol = distanceRolloff != null ? distanceRolloff.Evaluate(Mathf.Clamp01(t)) : 1f - Mathf.Clamp01(t);
            float yBias = 1f + Mathf.Clamp(rel.y / Mathf.Max(0.001f, maxDistance), -1f, 1f) * yLoudnessBias;
            float finalVol = Mathf.Clamp01(baseVol * yBias) * _userVolume;

            // -------- Stereo Pan (X + Y contribution) -----------
            float panX = Mathf.Clamp(rel.x / Mathf.Max(0.001f, panSaturationDistance), -1f, 1f) * xPanStrength;
            float panY = Mathf.Clamp(rel.y / Mathf.Max(0.001f, panSaturationDistance), -1f, 1f) * yPanStrength;
            float finalPan = Mathf.Clamp(panX + panY, -1f, 1f);

            // -------- Optional simple Doppler in 2D -------------
            float finalPitch = _userPitch;
            if (useSimpleDoppler)
            {
                float dt = Mathf.Max(0.0001f, Time.unscaledDeltaTime);
                Vector2 vel = (rel - _lastRel) / dt; // relative velocity in listener space
                float approachSpeed = -Vector2.Dot(vel.normalized, rel.normalized) * vel.magnitude;
                float doppler = Mathf.Clamp(approachSpeed / Mathf.Max(0.001f, dopplerScale), -1f, 1f);
                finalPitch = Mathf.Clamp(_userPitch + (doppler * dopplerAmount), 0.5f, 1.5f);

                _lastRel = rel;
                _lastTime = Time.unscaledTime;
            }

            // -------- Apply to source ---------------------------
            _src.panStereo = finalPan;
            _src.volume    = finalVol;   // note: affects all clips on this AudioSource
            _src.pitch     = finalPitch; // shared for all clips on this AudioSource
        }

        // -------------------- One-shots --------------------

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

        // -------------------- Loops (moved here) --------------------

        /// <summary>
        /// Start (or update) a looping clip at a fixed world position.
        /// If the same clip is already looping, updates volume/pitch unless retriggerIfSame = true.
        /// </summary>
        public void PlayLoop(AudioClip clip, Vector3 worldPos, float volume = 1f, float pitch = 1f, bool retriggerIfSame = false)
        {
            if (!clip || !_src) return;

            _followTarget = null;
            transform.position = worldPos;
            StartOrUpdateLoop(clip, volume, pitch, retriggerIfSame);
        }

        /// <summary>
        /// Start (or update) a looping clip that follows a Transform.
        /// If the same clip is already looping, updates volume/pitch unless retriggerIfSame = true.
        /// </summary>
        public void PlayLoop(AudioClip clip, Transform follow, float volume = 1f, float pitch = 1f, bool retriggerIfSame = false)
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

            if (fadeOutSeconds <= 0f)
            {
                CancelFadeIfAny();
                _src.Stop();
                _src.loop = false;
                _src.clip = null;
                _userVolume = 1f;  // reset
                _followTarget = null;
            }
            else
            {
                CancelFadeIfAny();
                _fadeCR = StartCoroutine(FadeAndStop(fadeOutSeconds));
            }
        }

        // -------------------- Helpers --------------------

        void StartOrUpdateLoop(AudioClip clip, float volume, float pitch, bool retriggerIfSame)
        {
            if (_src.isPlaying && _src.loop && _src.clip == clip && !retriggerIfSame)
            {
                _userVolume = Mathf.Clamp01(volume);
                _userPitch  = pitch;
                return;
            }

            CancelFadeIfAny();
            _src.Stop();

            _src.clip   = clip;
            _src.loop   = true;
            _userVolume = Mathf.Clamp01(volume);
            _userPitch  = pitch;
            _src.Play();
        }

        void CancelFadeIfAny()
        {
            if (_fadeCR != null)
            {
                StopCoroutine(_fadeCR);
                _fadeCR = null;
            }
        }

        IEnumerator FadeAndStop(float seconds)
        {
            float start = _userVolume;
            float t = 0f;
            while (t < seconds && _src.loop && _src.isPlaying)
            {
                t += Time.unscaledDeltaTime;
                _userVolume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / seconds));
                yield return null;
            }

            _src.Stop();
            _src.loop = false;
            _src.clip = null;
            _userVolume = 1f;
            _followTarget = null;
            _fadeCR = null;
        }
    }
}
