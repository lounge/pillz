using System.Collections.Generic;
using pillz.client.Scripts.AbilityEffects;
using pillz.client.Scripts.Constants;
using pillz.client.Scripts.ScriptableObjects;
using SpacetimeDB;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace pillz.client.Scripts
{
    public class ProjectileCollisionRelay : MonoBehaviour
    {
        [SerializeField] private string terrainTag = Tags.Terrain;

        private ProjectileController _projectile;
        private AbilityData _abilityData;
        private ProjectileConfig _config;

        public void Init(ProjectileConfig config)
        {
            _projectile = GetComponent<ProjectileController>();
            
            _config = config;
            _abilityData = new AbilityData
            {
                Effects = new List<AbilityEffect>
                {
                    new DamageEffect { MaxDamage = _config.maxDamage },
                    new KnockbackEffect { MaxForce = _config.maxForce }
                }
            };
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var hitObject = collision.gameObject;
            var contact = collision.GetContact(0);


            if (_config.collisionSound)
            {
                Log.Debug("ProjectileController: Playing collision sound.");

                AudioManager.Instance.Play(_config.collisionSound, contact.point);
            }


            if (_config.explosionPrefab)
            {
                Instantiate(_config.explosionPrefab, transform.position, Quaternion.identity);
            }

            Log.Debug("ProjectileController: Collision detected with " + hitObject.name);
            Game.Connection.Reducers.DeleteProjectile(_projectile.EntityId);


            _abilityData.ApplyExplosionAt(contact.point, _config.explosionRadius);

            if (hitObject.CompareTag(terrainTag))
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

                if (_config.explosionRadius <= 0f)
                {
                    Game.Connection.Reducers.DeleteTerrainTile(cellPos.x, cellPos.y);
                }
                else
                {
                    Game.Connection.Reducers.DeleteTerrainTiles(cellPos.x, cellPos.y, _config.explosionRadius);
                }
            }
        }
    }
}