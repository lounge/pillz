using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace pillz.client.Scripts
{
    public class StartScreenManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private Button playButton;
        
        public static StartScreenManager Instance { get; private set; }

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
                GameManager.Connection.Reducers.EnterGame(username, TerrainManager.Instance.GetRandomSpawnPosition());
                gameObject.SetActive(false);
            }
        }
    }
}