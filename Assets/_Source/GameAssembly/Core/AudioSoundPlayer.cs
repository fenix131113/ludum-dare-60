using UnityEngine;

namespace GameAssembly.Core
{
    public class AudioSoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource source;

        public void PlaySound(AudioClip clip, float volume) => source.PlayOneShot(clip, volume);
    }
}