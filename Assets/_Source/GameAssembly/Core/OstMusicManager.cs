using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.Core
{
    public class OstMusicManager : MonoBehaviour
    {
        [SerializeField] private AudioSource ostSource;

        [Inject] private PlayerState _playerState;

        private void Start() => _playerState.OnShelterStateChanged += OnShelterStateChanged;

        private void OnDestroy() => _playerState.OnShelterStateChanged -= OnShelterStateChanged;

        private void OnShelterStateChanged()
        {
            if(_playerState.IsInShelter())
                ostSource.Stop();
            else
                ostSource.Play();
        }
    }
}