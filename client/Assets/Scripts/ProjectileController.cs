using pillz.client.Scripts.AbilityEffects;
using pillz.client.Scripts.ScriptableObjects;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class ProjectileController : EntityController
    {
        [Header("Data")]
        [SerializeField] private ProjectileConfig projectileConfig;
        
        private Rigidbody2D _rb;
        private AbilityData _abilityData;
        private Vector2 _lastPosition;
        private float _lastPositionSendTimestamp;

        protected override void Awake()
        {
            gameObject.SetActive(true);
            _rb = GetComponent<Rigidbody2D>();
            
            var collisionRelay = GetComponent<ProjectileCollisionRelay>();
            collisionRelay.Init(projectileConfig);
        }

        public override void OnEntityUpdated(Entity newVal)
        {
            base.OnEntityUpdated(newVal);

            if (Owner.IsLocalPlayer)
                return;

            transform.position = (Vector2)newVal.Position;
            _rb.position = transform.position;
            _rb.linearVelocity = Vector2.zero;
        }

        protected override void Update()
        {
            if (Owner && (!Owner.IsLocalPlayer || !GameHandler.IsConnected()))
            {
                return;
            }

            if (!_rb.linearVelocity.Equals(_lastPosition) &&
                Time.time - _lastPositionSendTimestamp >= SendUpdatesFrequency)
            {
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
        }
    }
}