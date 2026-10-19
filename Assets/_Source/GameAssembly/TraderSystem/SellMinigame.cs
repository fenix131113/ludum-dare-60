using System;
using System.Collections.Generic;
using GameAssembly.ItemsSystem.Minigames;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GameAssembly.TraderSystem
{
    public class SellMinigame : AMinigame
    {
        private const int MAX_INFLUENCE = 100;

        [SerializeField] private List<DialogueItem> dialogueItems;
        [SerializeField] private GameObject minigamePanel;
        [SerializeField] private Transform dialoguesParent;
        [SerializeField] private Image positiveFiller;
        [SerializeField] private Image negativeFiller;
        [SerializeField] private Image gameTimerFiller;
        [SerializeField] private SellDialogueItem dialoguePrefab;
        [SerializeField] private float minTimeToSpawnDialogue;
        [SerializeField] private float maxTimeToSpawnDialogue;
        [SerializeField] private float gameTime;
        [SerializeField] private int maxDialogueCount;

        public override event Action<bool> OnMinigameEnded;

        public bool IsDefaultPrice { get; private set; }

        private int _currentInfluence;
        private bool _isPlaying;
        private float _spawnTimer;
        private float _gameTimer;
        private readonly List<SellDialogueItem> _spawnedDialogues = new();

        public void ApplyInfluence(int influence)
        {
            _currentInfluence += influence;
            _currentInfluence = Mathf.Clamp(_currentInfluence, -MAX_INFLUENCE, MAX_INFLUENCE);

            positiveFiller.fillAmount = Mathf.Clamp((float)_currentInfluence, 0, MAX_INFLUENCE) / MAX_INFLUENCE;
            negativeFiller.fillAmount =
                Mathf.Abs(Mathf.Clamp((float)_currentInfluence, -MAX_INFLUENCE, 0)) / MAX_INFLUENCE;

            switch (_currentInfluence)
            {
                case MAX_INFLUENCE:
                    WinMinigame();
                    return;
                case -MAX_INFLUENCE:
                    LooseMinigame();
                    break;
            }
        }

        public void RemoveDialogue(SellDialogueItem item) => _spawnedDialogues.Remove(item);

        public override void StartMinigame()
        {
            minigamePanel.SetActive(true);
            ResetMinigame();
            _isPlaying = true;
        }

        public override void ResetMinigame()
        {
            _currentInfluence = 0;
            ApplyInfluence(0);
            IsDefaultPrice = true;
            _spawnedDialogues.ForEach(x => Destroy(x.gameObject));
            _spawnedDialogues.Clear();
            RandomizeSpawnTimer();
            _gameTimer = gameTime;
        }

        public override void WinMinigame()
        {
            _isPlaying = false;
            IsDefaultPrice = false;
            minigamePanel.SetActive(false);
            OnMinigameEnded?.Invoke(true);
        }

        public override void LooseMinigame()
        {
            _isPlaying = false;
            IsDefaultPrice = false;
            minigamePanel.SetActive(false);
            OnMinigameEnded?.Invoke(false);
        }

        public override bool IsPlaying() => _isPlaying;

        private void EndMinigameWithDefault()
        {
            _isPlaying = false;
            minigamePanel.SetActive(false);
            OnMinigameEnded?.Invoke(false);
        }
        
        private void Update()
        {
            if(!_isPlaying)
                return;
            
            _gameTimer -= Time.deltaTime;
            gameTimerFiller.fillAmount = Mathf.Clamp(_gameTimer, 0, gameTime) / gameTime;
            
            if(_gameTimer <= 0f)
            {
                EndMinigameWithDefault();
                return;
            }

            if (_spawnedDialogues.Count >= maxDialogueCount)
                return;

            _spawnTimer -= Time.deltaTime;

            if (_spawnTimer > 0f)
                return;

            SpawnDialogue();
            RandomizeSpawnTimer();
        }

        private void SpawnDialogue()
        {
            var index = Random.Range(0, dialogueItems.Count);
            var item = dialogueItems[index];

            var spawned = Instantiate(dialoguePrefab, dialoguesParent);

            var xMax = Screen.width / 4;
            var yMax = Screen.height / 4;

            spawned.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(Random.Range(-xMax, xMax), Random.Range(-yMax, yMax));

            spawned.Initialize(item.Text, item.Influence, this);
            _spawnedDialogues.Add(spawned);
        }

        private void RandomizeSpawnTimer() =>
            _spawnTimer = Random.Range(minTimeToSpawnDialogue, maxTimeToSpawnDialogue);
    }

    [Serializable]
    public class DialogueItem
    {
        [field: SerializeField] public string Text { get; private set; }
        [field: SerializeField] public int Influence { get; private set; }
    }
}