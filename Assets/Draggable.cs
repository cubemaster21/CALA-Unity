using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler {

    public bool snapBack = false;
    Vector2 snapBackPosition;
    public GameManager manager;

    public void OnDrag(PointerEventData eventData)
    {

        this.transform.position = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }
    public void OnEndDrag(PointerEventData eventData)
    {
      
        //int index = manager.currentQuestion.solution;
        if (manager.currentQuestion.answers[manager.currentQuestion.solution[0]] == gameObject.GetComponent<TEXDraw>().text)
        {
            Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 0));
            if (manager.isInRabbit(new Vector2(p.x, p.y))) 
                manager.popRabbit();
        }


        if (snapBack) this.transform.position = snapBackPosition;
    }
    // Use this for initialization
    void Start () {
        snapBackPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
