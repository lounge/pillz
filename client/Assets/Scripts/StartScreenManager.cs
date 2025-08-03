using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace masks.client.Scripts
{
    public class StartScreenManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private Button playButton;

        private void Awake()
        {
            playButton.interactable = false;
            usernameInput.onValueChanged.AddListener(OnUsernameChanged);
            playButton.onClick.AddListener(OnPlayClicked);
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
                GameManager.Connection.Reducers.EnterGame(username);
                gameObject.SetActive(false);
            }
        }
    }
}