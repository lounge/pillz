using UnityEngine;

namespace pillz.client.Scripts.ScriptableObjects.Pill
{
    [CreateAssetMenu(menuName = "Pill/MovementConfig")]
    public class MovementConfig :
        ScriptableObject
    {
        public float moveSpeed = 10f;
        public float jumpForce = 10f;
        public float smoothing = 0.15f;
        public float airDragFactor = 0.95f;
        public LayerMask groundLayer;
        public float groundCheckRadius = 0.2f;
        public float jetpackAccelerate = 30f;
        public float jetpackBrake = 40f;
        public float jetpackStrafe = 9f;
        
    }
}