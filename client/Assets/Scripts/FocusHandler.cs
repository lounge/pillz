using UnityEngine;

namespace pillz.client.Scripts
{
    public class FocusHandler : MonoBehaviour
    {
        public static bool HasFocus;
        public void OnApplicationFocus(bool hasFocus)
        {
            HasFocus = hasFocus;
            Debug.Log(hasFocus ? "Application has focus" : "Application lost focus");
        }
    }
}