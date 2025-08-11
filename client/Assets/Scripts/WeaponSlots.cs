using pillz.client.Scripts.ScriptableObjects.Pill;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class WeaponSlots : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition primary;
        [SerializeField] private WeaponDefinition secondary;

        private WeaponController _primary, _secondary;
        private WeaponType _selected;

        public void Init(Transform owner, PlayerController player, Vector2 aimDir)
        {
            _primary = Instantiate(primary.controllerPrefab, owner).GetComponent<WeaponController>();
            _primary.Init(primary.type, owner, player, aimDir);

            _secondary = Instantiate(secondary.controllerPrefab, owner).GetComponent<WeaponController>();
            _secondary.Init(secondary.type, owner, player, aimDir);
            _secondary.Disable();

            _selected = WeaponType.Primary;
            
            GameInit.Connection.Reducers.InitAmmo(_primary.GetAmmo(), _secondary.GetAmmo());
        }

        public void Select(WeaponType type)
        {
            if (_selected == type)
                return;

            _selected = type;

            if (type == WeaponType.Primary)
            {

                _secondary.Disable();
                _primary.Enable();
            }
            else
            {
                _primary.Disable();
                _secondary.Enable();
            }
        }

        public EntityController Shoot(Projectile p, PlayerController player, Vector2 spawn, float speed)
        {
            return _selected == WeaponType.Primary
                ? _primary.Shoot(p, player, spawn, speed)
                : _secondary.Shoot(p, player, spawn, speed);
        }

        public void SetAim(Vector2 aim)
        {
            _primary?.SetAimDir(aim);
            _secondary?.SetAimDir(aim);
        }
        
        public void SetAmmo(int primaryAmmo, int secondaryAmmo)
        {
            _primary.SetAmmo(primaryAmmo);
            _secondary.SetAmmo(secondaryAmmo);
        }

        public WeaponController GetPrimary()
        {
            return _primary;
        }
        
        public WeaponController GetSecondary()
        {
            return _secondary;
        }
    }
}