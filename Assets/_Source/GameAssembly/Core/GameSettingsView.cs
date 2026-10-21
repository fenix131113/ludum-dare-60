using GameAssembly.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using VContainer;

namespace GameAssembly.Core
{
    public class GameSettingsView : MonoBehaviour
    {
        private const string SENSITIVITY_KEY = "Settings.CameraSensitivity";
        private const string MASTER_KEY = "Settings.MasterVolume";
        private const string MUSIC_KEY = "Settings.MusicVolume";
        private const string SOUNDS_KEY = "Settings.SoundsVolume";

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TMP_Text sensitivityLabel;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private TMP_Text masterLabel;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private TMP_Text musicLabel;
        [SerializeField] private Slider soundsSlider;
        [SerializeField] private TMP_Text soundsLabel;

        [Inject] private PlayerCamera _playerCamera;

        private bool _hasChanges;

        private void Start()
        {
            sensitivitySlider.SetValueWithoutNotify(Mathf.Clamp(PlayerPrefs.GetFloat(SENSITIVITY_KEY, _playerCamera.Sensitivity),
                sensitivitySlider.minValue, sensitivitySlider.maxValue));
            masterSlider.SetValueWithoutNotify(Mathf.Clamp01(PlayerPrefs.GetFloat(MASTER_KEY, 1f)));
            musicSlider.SetValueWithoutNotify(Mathf.Clamp01(PlayerPrefs.GetFloat(MUSIC_KEY, 1f)));
            soundsSlider.SetValueWithoutNotify(Mathf.Clamp01(PlayerPrefs.GetFloat(SOUNDS_KEY, 1f)));

            OnSensitivityChanged(sensitivitySlider.value);
            OnMasterChanged(masterSlider.value);
            OnMusicChanged(musicSlider.value);
            OnSoundsChanged(soundsSlider.value);
            _hasChanges = false;

            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            soundsSlider.onValueChanged.AddListener(OnSoundsChanged);
        }

        private void OnSensitivityChanged(float value)
        {
            _playerCamera.SetSensitivity(value);
            sensitivityLabel.text = value.ToString("0.00");
            PlayerPrefs.SetFloat(SENSITIVITY_KEY, value);
            _hasChanges = true;
        }

        private void OnMasterChanged(float value) => SetVolume("MasterVolume", MASTER_KEY, value, masterLabel);
        private void OnMusicChanged(float value) => SetVolume("MusicVolume", MUSIC_KEY, value, musicLabel);
        private void OnSoundsChanged(float value) => SetVolume("SoundsVolume", SOUNDS_KEY, value, soundsLabel);

        private void SetVolume(string parameter, string key, float value, TMP_Text label)
        {
            value = Mathf.Clamp01(value);
            var decibels = value <= 0f ? -80f : Mathf.Max(-80f, Mathf.Log10(value) * 20f);

            if (audioMixer && !audioMixer.SetFloat(parameter, decibels))
                Debug.LogError($"[GameSettingsView] Expose {parameter} in the AudioMixer!");

            label.text = $"{Mathf.RoundToInt(value * 100f)}%";
            PlayerPrefs.SetFloat(key, value);
            _hasChanges = true;
        }

        public void Save()
        {
            if (!_hasChanges)
                return;

            PlayerPrefs.Save();
            _hasChanges = false;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                Save();
        }

        private void OnApplicationQuit() => Save();

        private void OnDestroy()
        {
            Save();

            if (sensitivitySlider)
                sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
            if (masterSlider)
                masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
            if (musicSlider)
                musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            if (soundsSlider)
                soundsSlider.onValueChanged.RemoveListener(OnSoundsChanged);
        }
    }
}
