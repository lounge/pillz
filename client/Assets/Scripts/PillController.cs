using pillz.client.Scripts.ScriptableObjects.Pill;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

namespace pillz.client.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PillController : EntityController
    {
        [Header("Data")] 
        [SerializeField] private MovementConfig movementConfig;
        [SerializeField] private JetpackConfig jetpackConfig;

        [Header("Modules")] 
        [SerializeField] private PillInputReader inputReader;
        [SerializeField] private Mover2D mover;
        [SerializeField] private Boundary2D boundary;
        [SerializeField] private JetpackModule jetpack;
        [SerializeField] private WeaponSlots weapons;
        [SerializeField] private PortalState portalState;
        
        [Header("UI")]
        [SerializeField] private PillHud pillHud;

        private Rigidbody2D _rb;
        private Camera _cam;
        private PlayerInput _lastSent;
        private ScoreDisplay _scoreDisplay;
        private GameObject _gameHud;
        private AmmoDisplay _ammoDisplay;
        
        private float _lastSend;

        public void Spawn(Pill pill, PlayerController owner)
        {
            base.Spawn(pill.EntityId, owner);
            
            _cam = Camera.main;
            _rb = GetComponent<Rigidbody2D>();

            transform.position = new Vector3(pill.Position.X + 0.5f, pill.Position.Y + 2f, 0);
            
            var hudCanvas = GameObject.Find("Pill HUD");
            pillHud = Instantiate(pillHud, hudCanvas.transform);
            pillHud.AttachTo(transform);
            pillHud.SetUsername(owner.Username);
            pillHud.SetHp(pill.Hp);
            pillHud.SetFuel(jetpack.Fuel);
            
            mover.Init(movementConfig, _rb);
            jetpack.Init(jetpackConfig, pillHud);
            weapons.Init(transform, owner, pill.AimDir);

            if (Owner && (!Owner.IsLocalPlayer || !GameInit.IsConnected()))
            {
                Log.Debug("PillMovement: Not local player or not connected, skipping movement init.");
                return;
            }

            _gameHud = GameObject.Find("Game HUD");
            _gameHud = Instantiate(Owner.GetHud(), _gameHud.transform);

            _scoreDisplay = _gameHud.GetComponentInChildren<ScoreDisplay>();
            _scoreDisplay.SetDmg(pill.Dmg);
            _scoreDisplay.SetFrags(pill.Frags);
            
            _ammoDisplay = _gameHud.GetComponentInChildren<AmmoDisplay>();
            _ammoDisplay.SetPrimary(weapons.GetPrimary().GetAmmo());
            _ammoDisplay.SetSecondary(weapons.GetSecondary().GetAmmo());

            _cam?.GetComponent<CameraFollow>()?.SetTarget(transform);
        }

        private void FixedUpdate()
        {
            if (!GameInit.IsConnected() || !Owner.IsLocalPlayer) return;

            var intent = inputReader.ConsumeFrameIntent();
            if (intent.ToggleJetpack)
                jetpack.Toggle();

            var jetActive = jetpack.Enabled;
            if (jetActive && intent.JumpHeld)
            {
                jetpack.ThrottleOn();
                mover.JetpackLift(jetpackConfig.force);
            }
            else
            {
                jetpack.ThrottleOff();
            }

            jetpack.Tick();
            portalState.Tick();
            boundary.Tick(_rb);
            mover.Tick(intent, jetActive, jetpack.Throttling);
            
            pillHud.SetFuel(jetpack.Fuel);

            var p = new PlayerInput(_rb.linearVelocity, _rb.position, !FocusHandler.HasFocus, intent.SelectWeapon);
            if (Time.time - _lastSend >= SendUpdatesFrequency && !p.Equals(_lastSent))
            {
                GameInit.Connection.Reducers.UpdatePlayer(p);
                _lastSent = p;
                _lastSend = Time.time;
            }
        }

        public override void OnDelete(EventContext ctx)
        {
            base.OnDelete(ctx);
            Destroy(pillHud?.gameObject);
            Destroy(_gameHud?.gameObject);
            Destroy(Owner.gameObject);
            
            if (Owner.IsLocalPlayer)
            {
                DeathScreen.Instance.Show(Owner);
            }
        }

        public void OnPillUpdated(Pill newVal)
        {
            pillHud.SetHp(newVal.Hp);
            weapons.SetAim(newVal.AimDir);
            weapons.Select(newVal.SelectedWeapon);
            weapons.SetAmmo(newVal.PrimaryWeapon.Ammo, newVal.SecondaryWeapon.Ammo);

            jetpack.OnJetpackUpdated(newVal.Jetpack);
            
            _scoreDisplay?.SetDmg(newVal.Dmg);
            _scoreDisplay?.SetFrags(newVal.Frags);
            
            _ammoDisplay?.SetPrimary(newVal.PrimaryWeapon.Ammo);
            _ammoDisplay?.SetSecondary(newVal.SecondaryWeapon.Ammo);
            
            Log.Debug($"AMMO CHECK: Primary={newVal.PrimaryWeapon.Ammo} Secondary={newVal.SecondaryWeapon.Ammo}");

            if (newVal.Force is not null)
            {
                _rb.AddForce(new Vector2(newVal.Force.X, newVal.Force.Y), ForceMode2D.Impulse);
                GameInit.Connection.Reducers.ForceApplied(Owner.PlayerId);
            }
        }
        
        public WeaponSlots GetWeapons()
        {
            return weapons;
        }

        public PortalState GetPortalState()
        {
            return portalState;
        }
    }
}