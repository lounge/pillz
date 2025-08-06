using TMPro;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class FpsDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fpsText;
        
        private float _deltaTime;

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            var fps = 1.0f / _deltaTime;
            fpsText.text = $"FPS {fps:0.}";
        }
    }
}
