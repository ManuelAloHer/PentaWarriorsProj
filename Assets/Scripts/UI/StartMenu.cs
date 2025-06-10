using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class StartMenu : MonoBehaviour
{
    [SerializeField] GameObject []tutorialSlides;
    [SerializeField] VideoClip[] clips;
    [SerializeField] int currentTutorialSlide = -1;  
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] GameObject MainPanel, tutorialSlidesPanel;
    [SerializeField] GameObject templarSword;
    public void MainMenuButton()
    {
        MainPanel.SetActive(false);
        templarSword.SetActive(false);
        tutorialSlidesPanel.SetActive(true);
        NextTutorialSlide();
    }

    public void NextTutorialSlide() 
    {
        currentTutorialSlide++;
        if (currentTutorialSlide >= 4) 
        {
            SkipToScene();
            return;
        }
        videoPlayer.Stop();
        tutorialSlides[currentTutorialSlide].SetActive(true);
        videoPlayer.clip = clips[currentTutorialSlide]; 
        videoPlayer.Play();
        
    }

    public void SkipToScene() 
    {
        currentTutorialSlide = -1;
        for (int i = 0; i < tutorialSlides.Length; i++) 
        {
            tutorialSlides[i].SetActive(false);
        }
        tutorialSlidesPanel.SetActive(false);
        templarSword.SetActive(true);
        SceneManager.LoadScene(1);
    }

    public void QuitButton()
    {
        SkipToScene();
        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }
}
