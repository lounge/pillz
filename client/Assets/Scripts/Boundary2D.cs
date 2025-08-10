using UnityEngine;

namespace pillz.client.Scripts
{
    public class Boundary2D : MonoBehaviour
    {
        [Header("Behavior")] 
        [SerializeField] private bool wrapX = true;
        [SerializeField] private float wrapPadding = 1f;
        [SerializeField] private bool clampBottom = true;

        public void Tick(Rigidbody2D rb)
        {
            var p = rb.position;

            if (wrapX)
            {
                if (p.x < TerrainHandler.Instance.MinX)
                {
                    p.x = TerrainHandler.Instance.MaxX - wrapPadding;
                }
                else if (p.x > TerrainHandler.Instance.MaxX)
                {
                    p.x = TerrainHandler.Instance.MinX + wrapPadding;
                }
            }
            else
            {
                p.x = Mathf.Clamp(p.x, TerrainHandler.Instance.MinX, TerrainHandler.Instance.MaxX);
            }

            if (clampBottom && p.y < TerrainHandler.Instance.MinY)
            {
                p.y = TerrainHandler.Instance.MinY;
            }

            rb.position = p;
        }
    }
}