using SpacetimeDB;
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var hitObject = collision.gameObject;

            Log.Debug("PortalController: Collision detected with " + hitObject.name);

            if (hitObject.CompareTag(Tags.Pill))
            {
                var pillController = hitObject.GetComponent<PillController>();
                if (pillController && pillController.Owner.IsLocalPlayer && !pillController.InPortal && pillController.PortalCoolDown <= 0f)
                {
                    Log.Debug("PortalController: Pill entered portal, teleporting...");
                    var connectedPortal = GameHandler.Connection.Db.Portal.Id.Find(_connectedPortalId);
                    if (connectedPortal != null)
                    {
                        pillController.transform.position = new Vector3(connectedPortal.Position.X, connectedPortal.Position.Y, 0);
                        pillController.InPortal = true;
                        pillController.PortalCoolDown = portalCoolDown; 
                        Log.Debug("PortalController: Pill teleported to " + pillController.transform.position);
                    }
                    else
                    {
                        Log.Error("PortalController: Connected portal not found!");
                    }
                }
            }
        }
    }
}