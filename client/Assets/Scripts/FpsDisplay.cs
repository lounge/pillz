using UnityEngine;

namespace masks.client.Scripts
{
    public class FpsDisplay : MonoBehaviour
    {
        private float _deltaTime;

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            var style = new GUIStyle();

            var rect = new Rect(10, 10, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 50;
            style.normal.textColor = Color.yellow;

            var fps = 1.0f / _deltaTime;
            var text = $"{fps:0.} FPS";
            GUI.Label(rect, text, style);
        }
    }
}
