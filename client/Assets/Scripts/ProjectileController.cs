using System;
using System.Collections.Generic;
using pillz.client.Scripts.AbilityEffects;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace pillz.client.Scripts
{
    public class ProjectileController : EntityController
    {
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private float maxForce = 50f;
        [SerializeField] private uint maxDamage = 20;
        [SerializeField] private float explosionRadius = 3.5f;

        private Rigidbody2D _rb;
        private float _lastPositionSendTimestamp;
        private AbilityData _abilityData;

        [NonSerialized] private Vector2 _lastPosition;

        protected override void Awake()
        {
            gameObject.SetActive(true);
            _rb = GetComponent<Rigidbody2D>();

            _abilityData = new AbilityData
            {
                Effects = new List<AbilityEffect>
                {
                    new DamageEffect { Damage = maxDamage },
                    new KnockbackEffect { Force = maxForce, ExplosionRadius = explosionRadius}
                }
            };
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

        protected override void Update()
        {
            if (Owner && (!Owner.IsLocalPlayer || !GameHandler.IsConnected()))
            {
                Log.Debug("ProjectileController: Not local player or not connected, skipping projectile update.");
                return;
            }

            if (!_rb.linearVelocity.Equals(_lastPosition) &&
                Time.time - _lastPositionSendTimestamp >= SendUpdatesFrequency)
            {
                Log.Debug("ProjectileController: Projectile position changed, updating velocity.");

                _lastPositionSendTimestamp = Time.time;
                GameHandler.Connection.Reducers.UpdateProjectile(_rb.linearVelocity, _rb.position);
            }

            if (IsOutOfBounds() != OutOfBound.None)
            {
                GameHandler.Connection.Reducers.DeleteProjectile(EntityId);
            }

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
            var contact = collision.GetContact(0);


            if (explosionPrefab)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            Log.Debug("ProjectileController: Collision detected with " + hitObject.name);
            GameHandler.Connection.Reducers.DeleteProjectile(EntityId);

            if (hitObject.CompareTag(Tags.Terrain))
            {
                var hitPosition = contact.point;

                var tilemap = hitObject.GetComponentInParent<Tilemap>();
                if (!tilemap)
                {
                    Log.Error("ProjectileController: Tilemap not found on Ground collision.");
                    return;
                }

                var cellPos = tilemap.WorldToCell(hitPosition);
                Log.Debug("ProjectileController: Tile hit at cell position " + cellPos);

                if (explosionRadius <= 0f)
                {
                    GameHandler.Connection.Reducers.DeleteTerrainTile(cellPos.x, cellPos.y);
                }
                else
                {
                    GameHandler.Connection.Reducers.DeleteTerrainTiles(cellPos.x, cellPos.y, explosionRadius);
                }
            }

            if (hitObject.CompareTag(Tags.Pill))
            {
                Log.Debug("ProjectileController: Hit a pill. Applying effects");
                
                var hitPill = hitObject.GetComponent<PillController>();

                foreach (var effect in _abilityData.Effects)
                {
                    effect.Execute(hitPill.Owner.PlayerId, hitPill.GetComponent<Rigidbody2D>(), contact.point);
                }
            }
        }
    }
}