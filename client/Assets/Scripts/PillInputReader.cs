using pillz.client.Assets.Input;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    [RequireComponent(typeof(PlayerInputActions))]
    public class PillInputReader : MonoBehaviour
    {
        private PillIntent _current;

        private PlayerInputActions _actions;

        private void Awake()
        {
            Log.Debug("PillInputReader: Initializing input reader.");
            _actions = new PlayerInputActions();
            _actions.Player.Move.performed += ctx => _current.Move = ctx.ReadValue<Vector2>();
            _actions.Player.Move.canceled += _ => _current.Move = Vector2.zero;
            _actions.Player.Jump.started += _ => _current.JumpHeld = true;
            _actions.Player.Jump.canceled += _ => _current.JumpHeld = false;
            _actions.Player.Jetpack.performed += _ => _current.ToggleJetpack = true;
            _actions.Player.PrimaryWeapon.performed += _ => _current.SelectWeapon = WeaponType.Primary;
            _actions.Player.SecondaryWeapon.performed += _ => _current.SelectWeapon = WeaponType.Secondary;
        }

        private void OnEnable() => _actions.Enable();
        private void OnDisable() => _actions.Disable();

        public PillIntent ConsumeFrameIntent()
        {
            var intent = _current;
            
            intent.ToggleJetpack = _current.ToggleJetpack;
            _current.ToggleJetpack = false;
            
            intent.SelectWeapon = _current.SelectWeapon != WeaponType.None ? _current.SelectWeapon : WeaponType.Primary;;
            
            return intent;
        }
    }
}