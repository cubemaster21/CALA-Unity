using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DollDraggable : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{

    public bool snapBack = false;
    Vector2 snapBackPosition;
    public NestingManager manager;

    public void OnDrag(PointerEventData eventData)
    {

        this.transform.position = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 0));
        p.z = 0;
        if (Vector3.Distance(p, Vector3.zero) < 3)
        {
            manager.onAnswerDragged(gameObject);
        }
       
        //int index = manager.currentQuestion.solution;
        

        if (snapBack) this.transform.position = snapBackPosition;
    }
    // Use this for initialization
    void Start()
    {
        snapBackPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
