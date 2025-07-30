using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public abstract class EntityController : MonoBehaviour
    {
        private const float LerpDurationSec = 0.1f;

        private uint _entityId;

        private float _lerpTime;
        private Vector3 _lerpTargetPosition;


        [NonSerialized] 
        public PlayerController Owner;

        [NonSerialized]
        private Rigidbody2D _entityRb;

        protected virtual void Awake()
        {
            _entityRb = GetComponent<Rigidbody2D>();
        }

        protected void Spawn(uint entityId, PlayerController owner)
        {
            _entityId = entityId;
            Owner = owner;

            
            if (Owner.IsLocalPlayer)
            {
                return;
            }
            
            var entity = GameManager.Connection.Db.Entity.Id.Find(entityId);
            _lerpTargetPosition = transform.position = (Vector2)entity?.Position;
            transform.localScale = Vector3.one;
        }

        public void OnEntityUpdated(Entity newVal)
        {
            _lerpTime = 0.0f;
            _lerpTargetPosition = (Vector2)newVal.Position;
        }

        public void OnDelete(EventContext context)
        {
            Destroy(gameObject);
        }

        public virtual void Update()
        {
            if (Owner.IsLocalPlayer)
            {
                return;
            }

            _lerpTime = Mathf.Min(_lerpTime + Time.deltaTime, LerpDurationSec);
            var t = _lerpTime / LerpDurationSec;

            transform.position = Vector3.Lerp(transform.position, _lerpTargetPosition, t);
            if (_entityRb)
            {
                _entityRb.position = transform.position;
                _entityRb.linearVelocity = Vector2.zero;
            }

            Log.Debug($"EntityController: Update {_entityId} pos: {transform.position} target: {_lerpTargetPosition}");
        }
    }
}