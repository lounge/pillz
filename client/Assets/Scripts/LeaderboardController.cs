using System;
using System.Linq;
using pillz.client.Assets.Input;
using UnityEngine;
using UnityEngine.UI;

namespace pillz.client.Scripts
{
    public class LeaderboardController : MonoBehaviour
    {
        [Header("Window")] [SerializeField] private GameObject windowRoot;
        [Header("List")] [SerializeField] private VerticalLayoutGroup listLayout;
        [SerializeField] private GameObject rowPrefab;
        [SerializeField] private Color evenRowColor = Color.white;
        [SerializeField] private Color oddRowColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private bool _visible;
        private PlayerInputActions _actions;

        private const int MaxRowCount = 10;
        private readonly LeaderboardRow[] _rows = new LeaderboardRow[MaxRowCount];

        private void OnEnable() => _actions.Enable();
        private void OnDisable() => _actions.Disable();

        private readonly Ranking.Weights _weights = new(100, 1, 50);


        private void Awake()
        {
            _actions = new PlayerInputActions();
            _actions.Player.Leaderboard.started += _ => _visible = true;
            _actions.Player.Leaderboard.canceled += _ => _visible = false;

            SetVisible(_visible);
        }

        private void Start()
        {
            for (var i = 0; i < MaxRowCount; i++)
            {
                var go = Instantiate(rowPrefab, listLayout.transform);
                var rowComp = go.GetComponent<LeaderboardRow>();
                go.gameObject.SetActive(false);

                _rows[i] = rowComp;
            }
        }


        private void Update()
        {
            SetVisible(_visible);

            if (_visible)
            {
                PopulateFromData();
            }
        }


        private void PopulateFromData()
        {
            var players = Game.Players.Where(x => x.Value.PlayerId != UInt32.MaxValue).Take(MaxRowCount)
                .Select(x => x.Value.Stats).ToList();
            var scoreFunc = Ranking.MakeScorer(_weights);

            var ranked = players
                .OrderByDescending(scoreFunc)
                .ThenByDescending(p => p.Frags)
                .ThenByDescending(p => p.Dmg)
                .ThenBy(p => p.Deaths)
                .ToList();

            int i;
            for (i = 0; i < ranked.Count; i++)
            {
                var player = ranked[i];
                var row = _rows[i];
                var score = scoreFunc(player);

                row.SetData(player.Username, player.Frags, player.Dmg, player.Deaths, score);

                row.SetBackgroundColor(i % 2 == 0 ? evenRowColor : oddRowColor);

                row.gameObject.SetActive(true);
            }
            for (; i < MaxRowCount; i++)
            {
                _rows[i].gameObject.SetActive(false);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)listLayout.transform);
        }

        private void SetVisible(bool visible)
        {
            if (windowRoot)
                windowRoot.SetActive(visible);
        }
    }
}