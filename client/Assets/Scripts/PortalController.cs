using System.Collections.Generic;
using pillz.client.Scripts.Constants;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class PortalController : MonoBehaviour
    {
        [SerializeField] private AudioClip teleportEnterSound;
        [SerializeField] private AudioClip teleportExitSound;
        
        private List<uint> _connections;

        public void Spawn(Portal portal)
        {
            _connections = portal.Connections;

            // Set position from server correction for client placement
            transform.position = new Vector3(portal.Position.X + 1f, portal.Position.Y + 1.5f, 0);
        }

        public void OnPortalUpdated(Portal newVal)
        {
            _connections = newVal.Connections;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Tags.Pill)) 
                return;
            
            AudioManager.Instance.Play(teleportEnterSound, transform.position, 1.5f);
            
            var pill = other.GetComponent<PillController>();
            var portalState = pill.PortalState;
            if (portalState && portalState.CanTrigger)
            {
                if (pill && pill.Owner.IsLocalPlayer)
                {
                    var rng = Random.Range(0, _connections.Count);
                    var connectedPortalId = _connections[rng];
                    
                    var connectedPortal = Game.Connection.Db.Portal.Id.Find(connectedPortalId);
                    if (connectedPortal != null)
                    {
                        other.attachedRigidbody.position =
                            new Vector3(connectedPortal.Position.X, connectedPortal.Position.Y, 0);
                    }
                }

                portalState.OnTeleported();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(Tags.Pill)) return;
            var portalState = other.GetComponent<PortalState>();
            portalState?.OnPortalExit();
            
            AudioManager.Instance.Play(teleportExitSound, transform.position, 1.5f);
        }
    }
}