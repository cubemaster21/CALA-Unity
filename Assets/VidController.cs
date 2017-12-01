using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class VidController : MonoBehaviour {
    UnityEngine.Video.VideoPlayer vidPlayer;
	// Use this for initialization
	void Start () {
        vidPlayer = gameObject.GetComponent<UnityEngine.Video.VideoPlayer>();
        vidPlayer.loopPointReached += EndReached;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
