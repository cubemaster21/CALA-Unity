using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void tutorialButtonPressed()
    {

    }
    public void commonMistakesButtonPressed()
    {
        Debug.Log("clicked"); 
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

}
