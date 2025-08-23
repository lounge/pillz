using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace pillz.client.Scripts
{
    public class DeathScreen : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private Button respawnButton;
        [SerializeField] private List<AudioClip> deathSounds;


        public static DeathScreen Instance { get; private set; }

        private string _username;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            gameObject.SetActive(false);
            respawnButton.interactable = false;
            usernameInput.onValueChanged.AddListener(OnUsernameChanged);
            respawnButton.onClick.AddListener(OnRespawnClicked);
        }

        public void Show(PlayerController player)
        {
            PlayAudioClip();
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
                Game.Connection.Reducers.EnterGame(username, TerrainHandler.Instance.GetRandomSpawnPosition());
                gameObject.SetActive(false);
            }
        }

        private void PlayAudioClip()
        {
            var rng = Random.Range(0, deathSounds.Count);
            if (deathSounds.Count > 0 && deathSounds[rng])
            {
                AudioManager.Instance.PlayGlobal(deathSounds[rng], 2F);
            }
        }
    }
}