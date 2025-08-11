using pillz.client.Scripts.Constants;
using UnityEngine;

namespace pillz.client.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class PillCollisionRelay : MonoBehaviour
    {
        [SerializeField] private string deathZoneTag = Tags.DeathZone;

        private PillController _pill;

        private void Awake() => _pill = GetComponent<PillController>();

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag(deathZoneTag))
            {
                GameInit.Connection.Reducers.DeletePill(_pill.Owner.PlayerId);
            }
            // _pill.Kill();
        }
    }
}