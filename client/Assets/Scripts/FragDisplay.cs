using UnityEngine;

namespace pillz.client.Scripts
{
    public class FragDisplay : MonoBehaviour
    {
        private float _frags;
        
        public void SetFrags(float frags)
        {
            _frags = frags;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            var style = new GUIStyle();

            var rect = new Rect(10, 90, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 50;
            style.normal.textColor = Color.red;

            var text = $"FRAGS {_frags:0.}";
            GUI.Label(rect, text, style);
        }
    }
}