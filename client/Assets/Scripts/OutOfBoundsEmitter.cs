using System;
using pillz.client.Scripts.Constants;
using UnityEngine;
using UnityEngine.Events;

namespace pillz.client.Scripts
{
    public class OutOfBoundsEmitter : MonoBehaviour
    {
        [Header("Target")] 
        [SerializeField] private Transform target; // if null, uses this.transform

        [Header("Bounds Source")] 
        [SerializeField] private BoundsSource source = BoundsSource.TerrainHandler;

        [Tooltip("Used if source = Manual")] 
        [SerializeField] private float minX = -100f, maxX = 100f, minY = -50f;

        [Tooltip("Used if source = Collider2D (reads collider.bounds)")] 
        [SerializeField] private Collider2D boundsCollider;

        [Header("Tick")] 
        [SerializeField] private TickMode tick = TickMode.Update;

        private OutOfBound _currentState = OutOfBound.None;

        public event Action<OutOfBound> StateChanged;

        [Serializable]
        public class OutOfBoundEvent : UnityEvent<OutOfBound>
        {
        }

        private void Awake()
        {
            if (!target)
            {
                target = transform;
            }
            _currentState = Evaluate(target.position);
        }

        private void Update()
        {
            if (tick == TickMode.Update)
            {
                Tick();
            }
        }

        private void FixedUpdate()
        {
            if (tick == TickMode.FixedUpdate)
            {
                Tick();
            }
        }

        private void Tick()
        {
            var next = Evaluate(target.position);
            if (next == _currentState) return;

            _currentState = next;
            StateChanged?.Invoke(_currentState);
        }


        private OutOfBound Evaluate(Vector3 pos)
        {
            GetBounds(out float bxMin, out float bxMax, out float byMin);

            if (pos.x < bxMin) return OutOfBound.Left;
            if (pos.x > bxMax) return OutOfBound.Right;
            if (pos.y < byMin) return OutOfBound.Bottom;
            return OutOfBound.None;
        }

        private void GetBounds(out float bxMin, out float bxMax, out float byMin)
        {
            switch (source)
            {
                case BoundsSource.Manual:
                    bxMin = minX;
                    bxMax = maxX;
                    byMin = minY;
                    return;

                case BoundsSource.Collider2D:
                    if (boundsCollider)
                    {
                        var b = boundsCollider.bounds;
                        bxMin = b.min.x;
                        bxMax = b.max.x;
                        byMin = b.min.y;
                        return;
                    }

                    bxMin = minX;
                    bxMax = maxX;
                    byMin = minY;
                    return;

                case BoundsSource.TerrainHandler:
                default:
                    bxMin = TerrainHandler.Instance.MinX;
                    bxMax = TerrainHandler.Instance.MaxX;
                    byMin = TerrainHandler.Instance.MinY;
                    return;
            }
        }

        public void SetManual(float newMinX, float newMaxX, float newMinY)
        {
            source = BoundsSource.Manual;
            minX = newMinX;
            maxX = newMaxX;
            minY = newMinY;
        }

        public void SetCollider(Collider2D col)
        {
            source = BoundsSource.Collider2D;
            boundsCollider = col;
        }
    }
}