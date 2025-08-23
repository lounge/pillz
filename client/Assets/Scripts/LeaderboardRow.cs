using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace pillz.client.Scripts
{
    public class LeaderboardRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text fragsText;
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text deathsText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Image background; // <-- reference to row background


        public void SetData(string username, int frags, int dmg, int deaths, int score)
        {
            if (nameText) nameText.text = username;
            if (fragsText) fragsText.text = frags.ToString();
            if (damageText) damageText.text = dmg.ToString();
            if (deathsText) deathsText.text = deaths.ToString();
            if (scoreText) scoreText.text = score.ToString();
        }
        
        
        public void SetBackgroundColor(Color color)
        {
            if (background) background.color = color;
        }
    }
}