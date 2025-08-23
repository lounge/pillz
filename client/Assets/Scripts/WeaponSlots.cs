using pillz.client.Scripts.ScriptableObjects.Pill;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class WeaponSlots : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition primary;
        [SerializeField] private WeaponDefinition secondary;

        public WeaponController Primary { get; private set; }
        public WeaponController Secondary { get; private set; }
        private WeaponType _selected;

        public void Init(Transform owner, PlayerController player, Vector2 aimDir)
        {
            Primary = Instantiate(primary.controllerPrefab, owner).GetComponent<WeaponController>();
            Primary.Init(primary.type, owner, player, aimDir);

            Secondary = Instantiate(secondary.controllerPrefab, owner).GetComponent<WeaponController>();
            Secondary.Init(secondary.type, owner, player, aimDir);
            Secondary.Disable();

            _selected = WeaponType.Primary;
            
            Game.Connection.Reducers.InitAmmo(Primary.Ammo, Secondary.Ammo);
        }

        public void Select(WeaponType type)
        {
            if (_selected == type)
                return;

            _selected = type;

            if (type == WeaponType.Primary)
            {

                Secondary.Disable();
                Primary.Enable();
            }
            else
            {
                Primary.Disable();
                Secondary.Enable();
            }
        }

        public EntityController Shoot(Projectile p, PlayerController player, Vector2 spawn, float speed)
        {
            return _selected == WeaponType.Primary
                ? Primary.Shoot(p, player, spawn, speed)
                : Secondary.Shoot(p, player, spawn, speed);
        }

        public void SetAim(Vector2 aim)
        {
            Primary.AimDir = aim;
            Secondary.AimDir = aim;
        }
        
        public void SetAmmo(int primaryAmmo, int secondaryAmmo)
        {
            Primary.Ammo = primaryAmmo;
            Secondary.Ammo = secondaryAmmo;
        }
    }
}