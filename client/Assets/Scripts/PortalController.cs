using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public class PortalController : MonoBehaviour
    {
        private uint _connectedPortalId;

        public void Spawn(Portal portal)
        {
            _connectedPortalId = portal.ConnectedPortalId;
            transform.position = new Vector3(portal.Position.X, portal.Position.Y, 0);
        }
        
        public void OnPortalUpdated(Portal newVal)
        {
            _connectedPortalId = newVal.ConnectedPortalId;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var hitObject = collision.gameObject;

            Log.Debug("PortalController: Collision detected with " + hitObject.name);

            if (hitObject.CompareTag("Mask"))
            {
                var maskController = hitObject.GetComponent<MaskController>();
                if (maskController && maskController.Owner.IsLocalPlayer && !maskController.InPortal && maskController.PortalCoolDown <= 0f)
                {
                    Log.Debug("PortalController: Mask entered portal, teleporting...");
                    var connectedPortal = GameManager.Connection.Db.Portal.Id.Find(_connectedPortalId);
                    if (connectedPortal != null)
                    {
                        maskController.transform.position = new Vector3(connectedPortal.Position.X, connectedPortal.Position.Y, 0);
                        maskController.InPortal = true;
                        maskController.PortalCoolDown = 2f; 
                        Log.Debug("PortalController: Mask teleported to " + maskController.transform.position);
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