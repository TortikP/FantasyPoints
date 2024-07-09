using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialWindowLogic : MonoBehaviour
{
    public GameObject[] tutorialPanels;
    private int currentPanel = 0;
    public Button nextButton;
    public Button backButton;
    public GameInterfaceLogic gameInterface;
    public VideoClip[] tutorialClips;
    public VideoPlayer videoPlayer;

    public void ChangePanel(int buttonValue)
    {
        tutorialPanels[currentPanel].SetActive(false);
        if (buttonValue == -1)
        {
            currentPanel--;
            videoPlayer.clip = tutorialClips[currentPanel];
            if (currentPanel <= 0)
            {
                backButton.interactable = false;
            }
            if (nextButton.interactable == false)
            {
                nextButton.interactable = true;
            }
        }
        else if(buttonValue == 1) 
        {
            currentPanel++;
            videoPlayer.clip = tutorialClips[currentPanel];
            if (currentPanel >= tutorialPanels.Length - 1)
            {
                nextButton.interactable = false;
            }
            if (backButton.interactable == false)
            {
                backButton.interactable = true;
            }
        }
        tutorialPanels[currentPanel].SetActive(true);
    }

    public void OpenTutorial()
    {
        gameObject.SetActive(true);
        gameInterface.TogglePause();
    }

    public void CloseTutorial()
    {
        gameObject.SetActive(false);
        gameInterface.TogglePause();
    }

}
