using System;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public abstract class EntityController : MonoBehaviour
    {
        [NonSerialized] private Vector3 _lastPosition;
        [NonSerialized] private Rigidbody2D _entityRb;
        [NonSerialized] public PlayerController Owner;
        [NonSerialized] public const float SendUpdatesFrequency = 1f / SendUpdatesPerSec;

        [NonSerialized] public uint EntityId;

        private const int SendUpdatesPerSec = 20;
        private const float LerpDurationSec = 0.1f;
        private float _lerpTime;
        private Vector3 _lerpTargetPosition;
        
        protected virtual void Awake()
        {
            _entityRb = GetComponent<Rigidbody2D>();
        }

        protected void Spawn(uint entityId, PlayerController owner, Vector2? initialPosition = null)
        {
            EntityId = entityId;
            Owner = owner;

            if (Owner.IsLocalPlayer)
            {
                return;
            }

            // Use provided position if available, otherwise fall back to DB
            var pos = initialPosition ??
                      (Vector2)(GameInit.Connection.Db.Entity.Id.Find(entityId)?.Position ?? Vector2.zero);
            _lerpTargetPosition = transform.position = pos;
        }

        public virtual void OnEntityUpdated(Entity newVal)
        {
            _lerpTime = 0.0f;
            _lerpTargetPosition = (Vector2)newVal.Position;
        }

        public virtual void OnDelete(EventContext context)
        {
            Destroy(gameObject);
        }

        protected virtual void Update()
        {
            if (!Owner || Owner.IsLocalPlayer)
                return;

            _lerpTime = Mathf.Min(_lerpTime + Time.deltaTime, LerpDurationSec);
            var t = _lerpTime / LerpDurationSec;
            var target = _lerpTargetPosition;
            var delta = (target - transform.position).sqrMagnitude;

            // Only correct if drifting too far
            if (delta > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, target, t);
            }

            if (_entityRb)
            {
                _entityRb.position = transform.position;
                _entityRb.linearVelocity = Vector2.zero;
            }
        }
    }
}