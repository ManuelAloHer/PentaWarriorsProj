using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSound : MonoBehaviour
{
    [SerializeField] AudioClip[] characterAudios; 
    AudioSource characterAudioSource;

    public void PlayAudio(int index) 
    { 
       characterAudioSource.clip = characterAudios[index];
        characterAudioSource.Play();    
    
    }
}
