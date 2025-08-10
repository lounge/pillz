using pillz.client.Scripts.Constants;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class PortalController : MonoBehaviour
    {
        [SerializeField] private float portalCoolDown = 2f;

        private uint _connectedPortalId;

        public void Spawn(Portal portal)
        {
            _connectedPortalId = portal.ConnectedPortalId;

            // Set position from server correction for client placement
            transform.position = new Vector3(portal.Position.X + 1f, portal.Position.Y + 1.5f, 0);
        }

        public void OnPortalUpdated(Portal newVal)
        {
            _connectedPortalId = newVal.ConnectedPortalId;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Tags.Pill)) 
                return;
            
            var pill = other.GetComponent<PillController>();
            var portalState = pill.GetPortalState();
            if (portalState && portalState.CanTrigger)
            {
                if (pill && pill.Owner.IsLocalPlayer)
                {
                    var connectedPortal = GameHandler.Connection.Db.Portal.Id.Find(_connectedPortalId);
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
        }
    }
}