using UnityEngine;

namespace masks.client.Scripts
{
    public class DmgDisplay : MonoBehaviour
    {
        private float _dmg;
        
        public void SetDmg(float dmg)
        {
            _dmg = dmg;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            var style = new GUIStyle();

            var rect = new Rect(10, 50, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 50;
            style.normal.textColor = Color.orange;

            var text = $"DMG {_dmg:0.}";
            GUI.Label(rect, text, style);
        }
    }
}