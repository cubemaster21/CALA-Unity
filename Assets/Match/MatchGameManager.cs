using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchGameManager : MonoBehaviour {
    public GameObject cardPrefab;
    public Texture[] cardFaces;
    public GameObject gameOverScreen;
    GameObject[] cards;
    GameObject selectedCard;
    int pairsFound = 0;
    bool gameOver = false;
    bool allowCardSelect = true;
    float disableCardSelectTimer;

    public float gameSpeed = 1.5f; // controls timers
	// Use this for initialization
	void Start () {
        float size = Camera.main.orthographicSize;
        float height = size * 2;
        float width = height * Camera.main.aspect;
        cards = new GameObject[20];

        float xInterval = (width) / 6;
        float yInterval = (height) / 5;
        for(int i = 0;i < 20; i++)
        {
            GameObject card = Instantiate(cardPrefab, new Vector3(((i % 5) + 1) * xInterval - width / 2,(Mathf.FloorToInt(i / 5) + 1) * yInterval - height / 2, -1), Quaternion.identity);
            card.transform.Rotate(new Vector3(0, 180, 0));
            card.GetComponent<CardID>().id = i + 1;
            cards[i] = card;
            setCardFace(card, i);
        }
	}

    // Update is called once per frame
    int count;
	void Update () {
        if (!allowCardSelect)
        {
            disableCardSelectTimer -= Time.deltaTime;
            if(disableCardSelectTimer <= 0)
            {
                allowCardSelect = true;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (gameOver)
            {
                //Lets get outta here
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && allowCardSelect)
            {
                GameObject card = hit.collider.gameObject;
                if (card == selectedCard || !card.GetComponent<CardRotate>().canRotate) return;
                if(selectedCard == null)
                {
                    selectedCard = card;
                    card.GetComponent<CardRotate>().flip();
                } else
                {
                    if(selectedCard.GetComponent<CardID>().id + card.GetComponent<CardID>().id == 21)
                    {
                        card.GetComponent<CardRotate>().flip();
                        card.GetComponent<CardRotate>().canRotate = false;
                        selectedCard.GetComponent<CardRotate>().canRotate = false;
                        //play correct sound
                        Debug.Log("pair found");
                        pairsFound++;
                        if(pairsFound == 10)
                        {
                            Instantiate(gameOverScreen, Vector3.zero, Quaternion.identity);
                            gameOver = true;
                        }
                        selectedCard = null;
                        

                    } else
                    {
                        card.GetComponent<CardRotate>().flip();
                        card.GetComponent<CardRotate>().setTimedFip(gameSpeed);
                        selectedCard.GetComponent<CardRotate>().setTimedFip(gameSpeed);
                        disableCardSelectTimer = gameSpeed;
                        allowCardSelect = false;
                        selectedCard = null;
                        Debug.Log("failed");
                    }
                }


                
            }
        }
    }
    void makeRandomCard()
    {
        GameObject card = Instantiate(cardPrefab, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), -1), Quaternion.identity);
        setCardFace(card, Random.Range(0, 20));
        
    }
    
    public void setCardFace(GameObject card, int cardFace)
    {
        GameObject front = card.transform.GetChild(1).gameObject;
        MeshRenderer rend = front.GetComponent<MeshRenderer>();
        rend.material.mainTexture = cardFaces[cardFace];
    }
}
