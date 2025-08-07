using UnityEngine;

namespace pillz.client.Scripts
{
    public class FocusHandler : MonoBehaviour
    {
        public static bool HasFocus { get; private set; }
        public void OnApplicationFocus(bool hasFocus)
        {
            HasFocus = hasFocus;
            Debug.Log(HasFocus ? "Application has focus" : "Application lost focus");
        }
    }
}