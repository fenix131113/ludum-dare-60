using System;
using System.Collections.Generic;
using GameAssembly.Player;
using GameAssembly.Player.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using VContainer;
using PlayerInput = GameAssembly.Player.Input.PlayerInput;
using Random = UnityEngine.Random;

namespace GameAssembly.ItemsSystem.Minigames.PipeMinigame
{
    public class PipeMinigame : AMinigame
    {
        private const string ALLOWED_CHARS = "QWERASDFZXCV";
            
        [SerializeField] private GameObject minigamePanel;
        [SerializeField] private GameObject spaceHelper;
        [SerializeField] private RectTransform charObjectsContainer;
        [SerializeField] private TMP_Text charObjectPrefab;
        [SerializeField] private int sequenceCount;
        [SerializeField] private int charsInSequence;
        [SerializeField] private List<TMP_Text> charObjects;
        [SerializeField] private Image ratImage;
        [SerializeField] private Sprite[] ratSprites;
        
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerInput _input;

        private string _currentSequence = "";
        private bool _isPlaying;
        private int _sequencesLeft;
        private int _currentCharIndex;
        private int _currentRatIndex;
        private bool _canAcceptInput;
        private bool _isBind;

        public override event Action<bool> OnMinigameEnded;

        public override void StartMinigame()
        {
            _playerState.SetCurrentInteractionState(PlayerInteractStates.LOCKED);
            _isPlaying = true;
            minigamePanel.SetActive(true);
            ResetMinigame();
            
            Invoke(nameof(EnableInput), 0.1f);
        }

        public override void ResetMinigame()
        {
            _currentRatIndex = 0;
            _canAcceptInput = false;
            _sequencesLeft = sequenceCount;
            spaceHelper.SetActive(false);
            ratImage.sprite = ratSprites[0];
            
            GenerateNewSequence();
        }

        public override void WinMinigame()
        {
            _playerState.SetCurrentInteractionState(PlayerInteractStates.FREE);
            minigamePanel.SetActive(false);
            _isPlaying = false;
            OnMinigameEnded?.Invoke(true);
        }

        public override void LooseMinigame()
        {
            _playerState.SetCurrentInteractionState(PlayerInteractStates.FREE);
            minigamePanel.SetActive(false);
            _isPlaying = false;
            OnMinigameEnded?.Invoke(false);
        }

        public override bool IsPlaying() => _isPlaying;
        
        private void EnableInput() => _canAcceptInput = true;
        
        private void Start()
        {
            InputSystem.onEvent += OnInputSystemEvent;

            _input.OnJumpPerformed += OnJump;
            
            _isBind = true;
        }

        private void OnDestroy()
        {
            if(!_isBind)
                return;
            
            InputSystem.onEvent -= OnInputSystemEvent;
            
            _input.OnJumpPerformed -= OnJump;
        }

        private void OnJump()
        {
            if (_currentSequence.Length != 0 || !_isPlaying)
                return;
            
            if(_sequencesLeft > 0)
            {
                _sequencesLeft--;
                _currentRatIndex++;
            }

            ratImage.sprite = ratSprites[_currentRatIndex];
            GenerateNewSequence();
            spaceHelper.SetActive(false);
        }

        private void OnInputSystemEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (device is not Keyboard)
                return;
            
            if (!_isPlaying || !_canAcceptInput || _playerState.IsMenuOpen())
                return;

            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
            
            foreach (var control in eventPtr.EnumerateChangedControls(device))
            {
                if (control is not UnityEngine.InputSystem.Controls.KeyControl keyControl)
                    continue;

                if (!(keyControl.ReadValueFromEvent(eventPtr) > 0.5f))
                    continue;
                
                var pressedKey = keyControl.keyCode;
                
                ProcessPhysicsKey(pressedKey);
            }
        }
        
        private void ProcessPhysicsKey(Key key)
        {
            var pressedKeyStr = key.ToString();

            if (_currentSequence.Length > 0 && pressedKeyStr.Length == 1 && pressedKeyStr == _currentSequence[0].ToString())
            {
                _currentSequence = _currentSequence[1..];
                charObjects[_currentCharIndex].gameObject.SetActive(false);
                _currentCharIndex++;
            }
            
            switch (_currentSequence.Length)
            {
                case 0 when _sequencesLeft == 1:
                    WinMinigame();
                    return;
                case 0 when _sequencesLeft > 0:
                    spaceHelper.SetActive(true);
                    break;
            }
        }

        private void GenerateNewSequence()
        {
            _currentSequence = string.Empty;
            _currentCharIndex = 0;
            
            for (var i = 0; i < charsInSequence; i++)
            {
                var randomIndex = Random.Range(0, ALLOWED_CHARS.Length);
                _currentSequence += ALLOWED_CHARS[randomIndex];
                
                if(i + 1 > charObjects.Count)
                    charObjects.Add(Instantiate(charObjectPrefab, charObjectsContainer));

                charObjects[i].text = ALLOWED_CHARS[randomIndex].ToString();
            }
            
            foreach (var obj in charObjects)
                obj.gameObject.SetActive(true);
        }
    }
}
