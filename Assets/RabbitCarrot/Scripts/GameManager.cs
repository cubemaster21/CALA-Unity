using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    
    public GameObject prefab;
    public TextAsset questionFile;
    public GameObject questionDisplay;
    public GameObject questionPrefab;
    public GameObject canvas;
    public GameObject poofPrefab;
    public GameObject scoreDisplay;
    GameObject[] answersSpawned;
    GameObject[] spawnPoints;
    public Question currentQuestion;
    QuestionBank bank;
    GameObject[] rabbits;
    int score = 0;
    // Use this for initialization
    void Start () {
        spawnPoints = GameObject.FindGameObjectsWithTag("spawnPoint");
        bank = loadJSON(questionFile.text);
        currentQuestion = null;
    }
    
	// Update is called once per frame
	void Update () {
        rabbits = GameObject.FindGameObjectsWithTag("rabbit");
        if(rabbits.Length < 1)
        {
            spawnRabbit();
        }
    }
    void spawnRabbit()
    {
        int index = Random.Range(0, spawnPoints.Length - 1);
        GameObject chosenSpawn = spawnPoints[index];
        Instantiate(prefab, chosenSpawn.transform.position, Quaternion.identity);

        currentQuestion = bank.getRandomQuestion(true);
        if(currentQuestion == null)
        {

            //all answered
        }
        TEXDraw text = questionDisplay.GetComponent<TEXDraw>();
        text.text = currentQuestion.question;




        popAnswers();
        answersSpawned = new GameObject[currentQuestion.answers.Length];

        int size = Screen.height / currentQuestion.answers.Length;
        for(int i = 0;i < currentQuestion.answers.Length; i++)
        {
            GameObject answer = Instantiate(questionPrefab, Vector2.zero, Quaternion.identity);
            answer.transform.SetParent(canvas.transform);
            RectTransform bounds = answer.GetComponent<RectTransform>();
            answer.transform.position = new Vector2(Screen.width -bounds.rect.width / 2,Screen.height -(i * size + size / 2));
            answer.GetComponent<TEXDraw>().text = currentQuestion.answers[i];
            answer.GetComponent<Draggable>().manager = this;
            answersSpawned[i] = answer;
            
        }
        Debug.Log("answers" + currentQuestion.answers.Length);
    }
    public void popRabbit()
    {
        if (rabbits == null) return;
        for(int i = 0;i < rabbits.Length; i++)
        {
            Instantiate(poofPrefab, rabbits[i].transform.position, Quaternion.identity);
            Destroy(rabbits[i]);
            
            rabbits[i] = null;
        }
        rabbits = null;
        score++;
        scoreDisplay.GetComponent<Text>().text = "" + score;
    }
    public void popAnswers()
    {
        if (answersSpawned == null) return;
        for(int i = 0;i < answersSpawned.Length; i++)
        {
            Destroy(answersSpawned[i]);
            answersSpawned[i] = null;
        }
        answersSpawned = null;
    }
    public bool isInRabbit(Vector2 position)
    {
        for(int i = 0;i < rabbits.Length; i++)
        {
            BoxCollider2D r = rabbits[i].GetComponent<BoxCollider2D>();
            if (r.bounds.Contains(position))
            {
                return true;
            }
        }
        return false;
    }
    public static QuestionBank loadJSON(string text)
    {
        QuestionBank bank = JsonUtility.FromJson<QuestionBank>(text);
        bank.init();
        return bank;
    }

}
