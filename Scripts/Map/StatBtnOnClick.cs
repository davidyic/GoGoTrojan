﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;
public class StatBtnOnClick : MonoBehaviour
{
    private bool pauseFlag;
    public Button leaveBtn, againBtn;
    public StatImageManager statManager;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LeaveButtonOnClick()
    {
        if (pauseFlag)
        {
            //become resume game
            setPauseFlag(false);
            statManager.setPauseFlag(false);
            statManager.gameObject.SetActive(false);
            Time.timeScale = 1;
            foreach (AudioSource audio in FindObjectsOfType<AudioSource>())
            {
                audio.Play();
            }
            return;
        }
        //clean Car flags on leaving la_large scene; 
        Car.cleanCarFlags();
        Debug.Log("waitForStartFlag" + Car.waitForStartFlag);
        SceneManager.LoadScene("RaceSummary");
    }

    public void PlayAgainOnClick()
    {
        if (pauseFlag)
        {
            //become leave game
            Time.timeScale = 1;
            setPauseFlag(false);
            statManager.setPauseFlag(false);
            statManager.gameObject.SetActive(false);

            SceneManager.LoadScene("RaceSummary");
            PlayerPrefs.SetInt("Ranking",999);
            foreach (AudioSource audio in FindObjectsOfType<AudioSource>())
            {
                audio.Play();
            }
            return;
        }
        //clean Car flags on leaving la_large scene; 
        Car.cleanCarFlags();
        Debug.Log("waitForStartFlag" + Car.waitForStartFlag);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }


    public void setPauseFlag(bool flag)
    {
        pauseFlag = flag;
    }
}
