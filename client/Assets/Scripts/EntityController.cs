using SpacetimeDB.Types;
using UnityEngine;

namespace masks.client.Scripts
{
    public abstract class EntityController : MonoBehaviour
    {
        private const float LerpDurationSec = 0.1f;

        // private static readonly int ShaderColorProperty = Shader.PropertyToID("_Color");


        protected uint EntityId;

        private float _lerpTime;
        private Vector3 _lerpStartPosition;
        private Vector3 _lerpTargetPosition;
        // private Vector3 _targetScale;

        protected virtual void Spawn(uint entityId)
        {
            EntityId = entityId;

            var entity = GameManager.Connection.Db.Entity.Id.Find(entityId);
            _lerpStartPosition = _lerpTargetPosition = transform.position = (Vector2)entity?.Position;
            transform.localScale = Vector3.one;
            // _targetScale = MassToScale(entity.Mass);
        }

        // public void SetColor(Color color)
        // {
        //     GetComponent<SpriteRenderer>().material.SetColor(ShaderColorProperty, color);
        // }

        public virtual void OnEntityUpdated(Entity newVal)
        {
            _lerpTime = 0.0f;
            _lerpStartPosition = transform.position;
            _lerpTargetPosition = (Vector2)newVal.Position;
            // _targetScale = MassToScale(newVal.Mass);
        }

        public virtual void OnDelete(EventContext context)
        {
            Destroy(gameObject);
        }

        public virtual void Update()
        {
            // Interpolate position and scale
            _lerpTime = Mathf.Min(_lerpTime + Time.deltaTime, LerpDurationSec);
            transform.position = Vector3.Lerp(_lerpStartPosition, _lerpTargetPosition, _lerpTime / LerpDurationSec);
            // transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * 8);
        }

        // public static Vector3 MassToScale(uint mass)
        // {
        //     var diameter = MassToDiameter(mass);
        //     return new Vector3(diameter, diameter, 1);
        // }
        //
        // public static float MassToRadius(uint mass) => Mathf.Sqrt(mass);
        // public static float MassToDiameter(uint mass) => MassToRadius(mass) * 2;
    }
}