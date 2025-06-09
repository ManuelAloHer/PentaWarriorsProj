using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSound : MonoBehaviour
{
    [SerializeField] AudioClip[] characterComunAudios; 
    AudioSource characterAudioSource;
    [SerializeField] AudioClip[] characterSpecificAudios;

    private void Awake()
    {
        characterAudioSource = GetComponentInParent<AudioSource>();
    }
    public void PlayCommunAudio(int index) 
    { 
       characterAudioSource.clip = characterComunAudios[index];
       characterAudioSource.Play();    
    }
    public void PlaySpecificAudio(int index)
    {
        characterAudioSource.clip = characterSpecificAudios[index];
        characterAudioSource.Play();
    }
}
