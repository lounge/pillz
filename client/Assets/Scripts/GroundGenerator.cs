using UnityEngine;

namespace masks.client.Scripts
{
    public class GroundGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private float heightCoverage = 0.25f; // 25%

        private void Awake()
        {
            GenerateGroundGrid();
        }

        private void GenerateGroundGrid()
        {
            var worldHeight = Camera.main!.orthographicSize * 2f;
            var worldWidth = worldHeight * Camera.main.aspect; 
            var groundHeight = worldHeight * heightCoverage;

            var rows = Mathf.CeilToInt(groundHeight / tileSize);
            var cols = Mathf.CeilToInt(worldWidth / tileSize);

            var startX = -worldWidth / 2f + tileSize / 2f;
            var startY = -Camera.main.orthographicSize + tileSize / 2f;

            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < cols; x++)
                {
                    var pos = new Vector2(
                        startX + x * tileSize,
                        startY + y * tileSize
                    );

                    Instantiate(groundPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }
}