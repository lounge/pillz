using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public class ProjectileController : EntityController
    {
        private Camera _mainCamera;
        private Rigidbody2D _rb;

        [NonSerialized]
        private Vector2 _lastPosition;

        protected override void Awake()
        {
            gameObject.SetActive(true);
            _mainCamera = Camera.main;
            _rb = GetComponent<Rigidbody2D>();
        }
        
        public override void OnEntityUpdated(Entity newVal)
        {
            base.OnEntityUpdated(newVal);

            if (Owner.IsLocalPlayer) 
                return;
            
            transform.position = (Vector2)newVal.Position;
            _rb.position = transform.position;
            _rb.linearVelocity = Vector2.zero;

            Log.Debug($"ProjectileController: Remote synced to position {transform.position}");
        }

        public override void Update()
        {
            if (Owner && (!Owner.IsLocalPlayer || !GameManager.IsConnected()))
            {
                Log.Debug("ProjectileController: Not local player or not connected, skipping projectile update.");
                return;
            }

            if (!_mainCamera)
                return;

            var screenPoint = _mainCamera.WorldToViewportPoint(transform.position);
            if (screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
            {
                Destroy(gameObject);
            }

            // Log.Debug("ProjectileController: Updating projectile position. direction: " + _rb.linearVelocity);

            if (!_rb.linearVelocity.Equals(_lastPosition))
            {
                Log.Debug("ProjectileController: Projectile position changed, updating velocity.");
                GameManager.Connection.Reducers.UpdateProjectile(_rb.linearVelocity, _rb.position);
            }
            
            _lastPosition = _rb.linearVelocity;
        }

        public void Spawn(Projectile projectile, PlayerController owner, Vector2 position, Vector2 velocity)
        {
            base.Spawn(projectile.EntityId, owner, position);
            _rb.linearVelocity = velocity;

            Log.Debug($"ProjectileController: Shooting projectile from with direction {velocity}.");
        }
    }
}