using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public abstract class EntityController : MonoBehaviour
    {
        private const float LerpDurationSec = 0.1f;
        private float _lerpTime;
        private Vector3 _lerpTargetPosition;
        private Camera _mainCamera;
        
        
        [NonSerialized]
        private Vector3 _lastPosition;
       
        [NonSerialized] 
        private Rigidbody2D _entityRb;
        
        [NonSerialized] 
        public PlayerController Owner;

        
        
        protected uint EntityId;
        
        protected const int SendUpdatesPerSec = 20;
        protected const float SendUpdatesFrequency = 1f / SendUpdatesPerSec;


        protected virtual void Awake()
        {
            
            _entityRb = GetComponent<Rigidbody2D>();
        }
        
        protected void Spawn(uint entityId, PlayerController owner, Vector2? initialPosition = null)
        {
            EntityId = entityId;
            Owner = owner;
            _mainCamera = Camera.main;


            if (Owner.IsLocalPlayer)
            {
                return;
            }

            // Use provided position if available, otherwise fall back to DB
            var pos = initialPosition ?? (Vector2)(GameManager.Connection.Db.Entity.Id.Find(entityId)?.Position ?? Vector2.zero);
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

        public virtual void Update()
        {
            if (!Owner || Owner.IsLocalPlayer)
            {
                Log.Debug("EntityController: Not updating local player entity.");
                return;
            }

            _lerpTime = Mathf.Min(_lerpTime + Time.deltaTime, LerpDurationSec);
            var t = _lerpTime / LerpDurationSec;
            transform.position = Vector3.Lerp(transform.position, _lerpTargetPosition, t);
           
            if (_entityRb && !transform.position.Equals(_lastPosition))
            {
                _entityRb.position = transform.position;
                _entityRb.linearVelocity = Vector2.zero;
                
                Log.Debug($"EntityController: TYPE: {this} Update {EntityId} pos: {transform.position} target: {_lerpTargetPosition}");
            }
        }
        
        protected bool IsOutOfBounds()
        {
            if (!_mainCamera)
            {
                
                Log.Debug("EntityController: No main camera found, cannot check out of bounds.");
                return false;
                
            }
            
            var screenPoint = _mainCamera.WorldToViewportPoint(transform.position);

            Log.Debug($"{screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1}");
            return screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;
        }
    }
}