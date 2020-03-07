using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public class SoundEffects : MonoBehaviour
    {
        public AudioClip[] FootStep;
        public AudioClip[] Miss;
        public AudioClip[] Hit;

        public AudioClip OpenInventory;
        public AudioClip CloseInventory;

        public AudioClip Lantern;

        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        public void PlayClip(AudioClip clip)
        {
            source.PlayOneShot(clip);
        }

        public void PlayClip(AudioClip[] clips)
        {
            int index = Random.Range(0, clips.Length);
            source.PlayOneShot(clips[index]);
        }
    }
}