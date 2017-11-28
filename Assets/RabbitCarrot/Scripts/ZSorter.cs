using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZSorter : MonoBehaviour {
    Vector3 stoPos;
	// Use this for initialization
	void Start () {
        stoPos = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        stoPos = gameObject.transform.position;
        stoPos.z = stoPos.y - 5;
        gameObject.transform.position = stoPos;
	}
}
