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
        
        [NonSerialized] public Ammo Ammo;
        [NonSerialized] public int AmmoAmount;
        
        private float _lastSend;
        private Vector3 _lastSent;
        private OutOfBoundsEmitter _outOfBoundsEmitter;

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
            _outOfBoundsEmitter = GetComponent<OutOfBoundsEmitter>();

            primary.SetActive(false);
            secondary.SetActive(false);
        }
        
        public void Spawn(Ammo ammo)
        {
            Ammo = ammo;

            transform.position = new Vector3(ammo.Position.X + 0.5f, ammo.Position.Y + 2f, 0);

            switch (ammo.AmmoType)
            {
                case WeaponType.Primary:
                    primary.SetActive(true);
                    AmmoAmount = primaryAmmoAmount;
                    break;
                case WeaponType.Secondary:
                    secondary.SetActive(true);
                    AmmoAmount = secondaryAmmoAmount;
                    break;
            }
        }

        private void OnBoundsChanged(OutOfBound state)
        {
            Log.Debug("AmmoController: Out of bounds state changed: " + state);
            if (state != OutOfBound.None)
            {
                Debug.Log("AmmoController: Out of bounds detected, deleting projectile.");
                GameInit.Connection.Reducers.DeleteAmmo(Ammo.EntityId);
            }
        }

        protected override void Update()
        {
            if (Time.time - _lastSend >= SendUpdatesFrequency && !transform.position.Equals(_lastSent))
            {
                GameInit.Connection.Reducers.UpdateAmmo(Ammo.EntityId,
                    new DbVector2(transform.position.x, transform.position.y));

                _lastSend = Time.time;
                _lastSent = transform.position;
            }
        }

        public override void OnDelete(EventContext context)
        {
            Destroy(gameObject);
        }
    }
}