using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace pillz.client.Scripts
{
    public class StartScreenHandler : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private Button playButton;
        
        public static StartScreenHandler Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
            playButton.interactable = false;
            usernameInput.onValueChanged.AddListener(OnUsernameChanged);
            playButton.onClick.AddListener(OnPlayClicked);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        private void OnUsernameChanged(string text)
        {
            playButton.interactable = !string.IsNullOrWhiteSpace(text);
        }

        private void OnPlayClicked()
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