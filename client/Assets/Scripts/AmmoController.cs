using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class AmmoController : EntityController
    {
        [SerializeField] private GameObject primary;
        [SerializeField] private GameObject secondary;
        [SerializeField] private int primaryAmmoAmount = 35;
        [SerializeField] private int secondaryAmmoAmount = 15;
        [field: SerializeField] public AudioClip PickupSound { get; private set; }


        [NonSerialized] public Ammo Ammo;
        [NonSerialized] public int AmmoAmount;

        private OutOfBoundsEmitter _outOfBoundsEmitter;
        private Rigidbody2D _rb;
        private ItemMovement _lastSent;
        private float _lastSend;

        protected override bool IsLocallySimulated => Game.IsSimulator;


        private void OnEnable()
        {
            _outOfBoundsEmitter.StateChanged += OnBoundsChanged;
        }

        private void OnDisable()
        {
            _outOfBoundsEmitter.StateChanged -= OnBoundsChanged;
        }

        protected override void Awake()
        {
            base.Awake();
            _outOfBoundsEmitter = GetComponent<OutOfBoundsEmitter>();
            _rb = GetComponent<Rigidbody2D>();

            primary.SetActive(false);
            secondary.SetActive(false);
        }

        public void Spawn(Ammo ammo, PlayerController owner)
        {
            base.Spawn(ammo.EntityId, owner);
        
            Ammo = ammo;
        
            if (!_rb)
            {
                _rb = GetComponent<Rigidbody2D>();
            }
        
            _rb.position = new Vector2(ammo.Position.X, ammo.Position.Y + 2f);
            if (Game.IsSimulator)
            {
                _rb.linearVelocity = new Vector2(Ammo.Direction.X, Ammo.Direction.Y);
            }
            
            primary.SetActive(ammo.AmmoType == WeaponType.Primary);
            secondary.SetActive(ammo.AmmoType == WeaponType.Secondary);
            AmmoAmount = ammo.AmmoType == WeaponType.Primary ? primaryAmmoAmount : secondaryAmmoAmount;
        }
        
        private void OnBoundsChanged(OutOfBound state)
        {
            Log.Debug("AmmoController: Out of bounds state changed: " + state);
            if (state != OutOfBound.None)
            {
                Log.Debug("AmmoController: Out of bounds detected, deleting ammo.");
                Game.Connection.Reducers.DeleteAmmo(Ammo.EntityId);
            }
        }
        protected void FixedUpdate()
        {
            if (!Game.IsConnected() || !Game.IsSimulator || !_rb)
            {
                Log.Debug("[AmmoController] Not connected or not observer, skipping ammo updates.");
                return;
            }

            var mov = new ItemMovement(_rb.position, _rb.linearVelocity);
            if (Time.time - _lastSend >= SendUpdatesFrequency && !mov.Equals(_lastSent))
            { 
                Game.Connection.Reducers.UpdateAmmo(Ammo.EntityId, mov);
                _lastSend = Time.time;
                _lastSent = mov;
            }
        }
    }
}