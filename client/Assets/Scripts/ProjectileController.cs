using pillz.client.Scripts.AbilityEffects;
using pillz.client.Scripts.ScriptableObjects;
using SpacetimeDB;
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
        private OutOfBoundsEmitter _outOfBoundsEmitter;

        protected override void Awake()
        {
            gameObject.SetActive(true);
            _rb = GetComponent<Rigidbody2D>();

            _outOfBoundsEmitter = GetComponent<OutOfBoundsEmitter>();
            
            var collisionRelay = GetComponent<ProjectileCollisionRelay>();
            collisionRelay.Init(projectileConfig);
        }

        private void OnEnable()
        {
            _outOfBoundsEmitter.StateChanged += OnBoundsChanged;
        }

        private void OnDisable()
        {
            _outOfBoundsEmitter.StateChanged -= OnBoundsChanged;
        }

        private void OnBoundsChanged(OutOfBound state)
        {
            Log.Debug("ProjectileController: Out of bounds state changed: " + state);
            if (state != OutOfBound.None)
            {
                Debug.Log("ProjectileController: Out of bounds detected, deleting projectile."); 
                GameHandler.Connection.Reducers.DeleteProjectile(EntityId);
            }
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

            _lastPosition = _rb.linearVelocity;
        }

        public void Spawn(Projectile projectile, PlayerController owner, Vector2 position, Vector2 velocity)
        {
            base.Spawn(projectile.EntityId, owner, position);
            _rb.linearVelocity = velocity;
        }
    }
}