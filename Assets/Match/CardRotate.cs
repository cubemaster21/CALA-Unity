using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRotate : MonoBehaviour {
    public bool rotate = false;
    public bool canRotate = true;
    public float flipSpeed = 2f;
    public float targetRotation = -1;

    float timedFlip = 0;
    bool hasTimedFlip = false;
	// Use this for initialization
	void Start () {
       // rotatePosition.z = -7.895f;
	}
    public void setTimedFip(float timer)
    {
        hasTimedFlip = true;
        timedFlip = timer;
    }
	public void flip()
    {
        if (!canRotate) return;
        rotate = true;
        targetRotation = Mathf.FloorToInt(this.transform.eulerAngles.y) + 180;
    }
	// Update is called once per frame
	void Update () {
        timedFlip -= Time.deltaTime;
        if(timedFlip <= 0 && hasTimedFlip)
        {
            flip();
            hasTimedFlip = false;
        }
        if (rotate)
        {
            this.transform.Rotate(Vector3.up, flipSpeed);
            //if (this.transform.eulerAngles.y <= 200f && this.transform.eulerAngles.y >= 150) Debug.Log(this.transform.eulerAngles.y);
            if(Mathf.FloorToInt(this.transform.eulerAngles.y) == targetRotation % 360)
            {
                rotate = false;
                targetRotation = -1;
            }
        }

       

    }
}
