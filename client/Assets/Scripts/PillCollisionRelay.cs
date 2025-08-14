using pillz.client.Scripts.Constants;
using SpacetimeDB;
using UnityEngine;

namespace pillz.client.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class PillCollisionRelay : MonoBehaviour
    {
        [SerializeField] private string deathZoneTag = Tags.DeathZone;

        private PillController _pill;

        private void Awake()
        {
            _pill = GetComponent<PillController>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag(deathZoneTag))
            {
                GameInit.Connection.Reducers.DeletePill(_pill.Owner.PlayerId);
            }
        }
        
        protected void OnCollisionEnter2D(Collision2D col)
        {
            Log.Debug("AmmoController: Collision detected with " + col.gameObject.name);
            var hitObject = col.gameObject;
        
            if (hitObject.CompareTag(Tags.Ammo))
            {
                var ammoController = col.gameObject.GetComponent<AmmoController>();
                
                AudioManager.Instance.Play(ammoController.PickupSound, col.transform.position);
            
                Log.Debug($"Pill {_pill.EntityId} picked up ammo {ammoController.Ammo.EntityId}");
                
                
                GameInit.Connection.Reducers.IncreaseAmmo(ammoController.AmmoAmount, ammoController.Ammo.AmmoType);
                GameInit.Connection.Reducers.DeleteAmmo(ammoController.Ammo.EntityId);
            }
        }
    }
}