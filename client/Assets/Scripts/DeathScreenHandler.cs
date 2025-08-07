using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace pillz.client.Scripts
{
    public class DeathScreenHandler : MonoBehaviour
    {
        
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private Button respawnButton;
        
        public static DeathScreenHandler Instance { get; private set; }
        
        private string _username;

        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
            respawnButton.interactable = false;
            usernameInput.onValueChanged.AddListener(OnUsernameChanged);
            respawnButton.onClick.AddListener(OnRespawnClicked);
        }

        public void Show(PlayerController player)
        {
            usernameInput.text = player.Username;
            gameObject.SetActive(true);
        }

        private void OnUsernameChanged(string text)
        {
            respawnButton.interactable = !string.IsNullOrWhiteSpace(text);
        }

        private void OnRespawnClicked()
        {
            string username = usernameInput.text.Trim();
            if (!string.IsNullOrEmpty(username))
            {
                GameHandler.Connection.Reducers.EnterGame(username, TerrainHandler.Instance.GetRandomSpawnPosition());
                gameObject.SetActive(false);
            }
        }
    }
}