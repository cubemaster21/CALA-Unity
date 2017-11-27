using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollAnimator : MonoBehaviour {
    private const int OPEN = 1,
        CLOSED = 2,
        OPENING = 3,
        CLOSING = 4;

    public int state = OPEN;

    public float scale = 1.0f;
    public float closeY = 1;
    public float openY = 6;
    public float y = 1;
    public GameObject top, bottom;
    public float moveTimer = 5;
    public float timeLeft = 2;

    public delegate void Action();
    public Action onAnimEnd;

    public GameObject latexLabel;
    public GameObject canvas;
    private GameObject personalLabel;
    public string labelText;

    public void open(Action end)
    {
        if (state == OPEN || state == OPENING) return;
        state = OPENING;
        onAnimEnd = end;
        timeLeft = moveTimer;
    }
    public void close(Action end)
    {
        if (state == CLOSED || state == CLOSING) return;
        state = CLOSING;
        onAnimEnd = end;
        timeLeft = moveTimer;
    }
    
	// Use this for initialization
	void Start () {
        scale = top.transform.localScale.x;
        
    }
	
	// Update is called once per frame
	void Update () {
        if(personalLabel == null)
        {
            personalLabel = Instantiate(latexLabel);
            personalLabel.transform.parent = canvas.transform;
            personalLabel.GetComponent<TEXDraw>().text = labelText;
        }
        switch (state)
        {
            case OPEN:
                y = openY * scale;
                break;
            case CLOSED:
                y = closeY * scale;
                break;
            case OPENING:
                //y += Time.deltaTime * moveSpeed;

                timeLeft -= Time.deltaTime;
                y = closeY * scale + Easings.BounceEaseOut((moveTimer - timeLeft) / moveTimer) * (openY * scale - closeY * scale);
                // if(y >= openY * scale)
                if (timeLeft <= 0)
                {
                    y = openY * scale;
                    state = OPEN;
                    if (onAnimEnd != null) onAnimEnd();
                }
                break;
            case CLOSING:
                //y -= Time.deltaTime * moveSpeed;
                timeLeft -= Time.deltaTime;
                y = openY * scale - Easings.BounceEaseOut((moveTimer - timeLeft) / moveTimer) * (openY * scale - closeY * scale);
                //if (y <= closeY * scale)

                if (timeLeft <= 0)
                {
                    y = closeY * scale;
                    state = CLOSED;
                    if (onAnimEnd != null) onAnimEnd();
                }
                break;
        }
        Vector3 pos = top.transform.localPosition;
        pos.y = y;
        top.transform.localPosition = pos;

        pos = bottom.transform.localPosition;
        pos.y = -y;
        bottom.transform.localPosition = pos;
        
        //latexLabel.transform.position = Camera.main.WorldToScreenPoint(pos);
        RectTransform rt = personalLabel.GetComponent<RectTransform>();


        pos += gameObject.transform.position;

        rt.position = Camera.main.WorldToScreenPoint(pos);


    }
    public void setScale(float scale)
    {
        Vector3 newScale = new Vector3(scale, scale, 1);
        top.transform.localScale = newScale;
        bottom.transform.localScale = newScale;
        this.scale = scale;
    }
    public void disableLabel() {
        personalLabel.GetComponent<TEXDraw>().enabled = false;
    }
    public void OnDestroy()
    {
        Destroy(personalLabel);
    }
}
