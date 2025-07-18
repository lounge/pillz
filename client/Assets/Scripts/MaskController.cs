using SpacetimeDB.Types;

namespace masks.client.Scripts
{
    public class MaskController : EntityController
    {
        private PlayerController _owner;
        
        public void Spawn(Mask mask, PlayerController owner)
        {
            base.Spawn(mask.EntityId);
            // SetColor(ColorPalette[circle.PlayerId % ColorPalette.Length]);


            _owner = owner;
            // GetComponentInChildren<TMPro.TextMeshProUGUI>().text = owner.Username;
        }

        // public override void OnDelete(EventContext context)
        // {
        //     base.OnDelete(context);
        //     // Owner.OnCircleDeleted(this);
        // }

    }
}