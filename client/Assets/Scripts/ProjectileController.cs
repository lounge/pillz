using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace masks.client.Scripts
{
    public class ProjectileController : EntityController
    {
        private Rigidbody2D _rb;
        private float _lastPositionSendTimestamp;

        [NonSerialized] private Vector2 _lastPosition;

        protected override void Awake()
        {
            gameObject.SetActive(true);
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
            
            if (!_rb.linearVelocity.Equals(_lastPosition) && Time.time - _lastPositionSendTimestamp >= SendUpdatesFrequency)
            {
                Log.Debug("ProjectileController: Projectile position changed, updating velocity.");

                _lastPositionSendTimestamp = Time.time;
                GameManager.Connection.Reducers.UpdateProjectile(_rb.linearVelocity, _rb.position);
            }
            
            // if (IsOutOfBounds())
            // {
            //     GameManager.Connection.Reducers.DeleteProjectile(EntityId);
            // }

            _lastPosition = _rb.linearVelocity;
        }

        public void Spawn(Projectile projectile, PlayerController owner, Vector2 position, Vector2 velocity)
        {
            base.Spawn(projectile.EntityId, owner, position);
            _rb.linearVelocity = velocity;

            Log.Debug($"ProjectileController: Shooting projectile from with direction {velocity}.");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var hitObject = collision.gameObject;
            
            Log.Debug("ProjectileController: Collision detected with " + hitObject.name);

            if (hitObject.CompareTag("Ground"))
            {
                var contact = collision.GetContact(0);
                var hitPosition = contact.point;

                var tilemap = hitObject.GetComponentInParent<Tilemap>();
                if (!tilemap)
                {
                    Log.Error("ProjectileController: Tilemap not found on Ground collision.");
                    return;
                }

                var cellPos = tilemap.WorldToCell(hitPosition);
                Log.Debug("ProjectileController: Tile hit at cell position " + cellPos);

                GameManager.Connection.Reducers.DeleteGroundTile(cellPos.x, cellPos.y);
                //
                // Log.Debug("ProjectileController: Hit the ground, deleting projectile.");
                GameManager.Connection.Reducers.DeleteProjectile(EntityId);
            }
            
            if (hitObject.CompareTag("Mask"))
            {
                var hitMask = hitObject.GetComponent<MaskController>();
                
                Log.Debug("ProjectileController: Hit a mask, deleting projectile. Applying damage");
                GameManager.Connection.Reducers.DeleteProjectile(EntityId);
                
                hitMask.ApplyDamage(10);
            }
            
            if (collision.gameObject.CompareTag("DeathZone"))
            {
                Log.Debug("ProjectileController: Collided with death zone, deleting projectile.");
                GameManager.Connection.Reducers.DeleteProjectile(EntityId);
            }
        }
    }
}