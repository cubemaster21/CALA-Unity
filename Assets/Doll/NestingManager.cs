using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NestingManager : MonoBehaviour {
    QuestionBank questions;

    int currentStage;
    public Question currentQuestion;

    public GameObject questionDisplay;
    public GameObject canvas;
    public GameObject questionPrefab;
    GameObject[] answersSpawned;
    public TextAsset questionFile;
    public GameObject dollPrefab;
    Stack<GameObject> dolls = new Stack<GameObject>();
    Vector3 dollSpawn = new Vector3(-2, -1, 0);
    List<Timer> timers = new List<Timer>();

    public GameObject scoreCounter;
    // Use this for initialization
    void Start () {
        questions = loadJSON(questionFile.text);
        pickQuestion();
	}
	public void pickQuestion()
    {
        currentStage = 0;
        currentQuestion = questions.getRandomQuestion(true);
        if(currentQuestion == null)
        {
            //all answered
            return;
        }
        questionDisplay.GetComponent<TEXDraw>().text = currentQuestion.question;


        popAnswers();
        answersSpawned = new GameObject[currentQuestion.answers.Length];
        int size = Screen.height / currentQuestion.answers.Length;
        for (int i = 0; i < currentQuestion.answers.Length; i++)
        {
            GameObject answer = Instantiate(questionPrefab, Vector2.zero, Quaternion.identity);
            answer.transform.SetParent(canvas.transform);
            RectTransform bounds = answer.GetComponent<RectTransform>();
            answer.transform.position = new Vector2(Screen.width - bounds.rect.width / 2, Screen.height - (i * size + size / 2));
            answer.GetComponent<TEXDraw>().text = currentQuestion.answers[i];
            answer.GetComponent<DollDraggable>().manager = this;
            answersSpawned[i] = answer;

        }
        scoreCounter.GetComponent<Text>().text = "" + questions.answered.Count;

    }
    public void popAnswers()
    {
        if (answersSpawned == null) return;
        for (int i = 0; i < answersSpawned.Length; i++)
        {
            Destroy(answersSpawned[i]);
            answersSpawned[i] = null;
        }
        answersSpawned = null;
    }
    // Update is called once per frame
    void Update () {
        for(int i = 0;i < timers.Count;i++)
        {
            Timer t = timers[i];
            if (t.Update()) timers.Remove(t);
        }
     
    }
    public void onAnswerDragged(GameObject obj) {


        
        if (currentQuestion.answers[currentQuestion.solution[currentStage]] == obj.GetComponent<TEXDraw>().text)
        {
            if(dolls.Count > 0) dolls.Peek().GetComponent<DollAnimator>().disableLabel();
            GameObject newDoll = Instantiate(dollPrefab, dollSpawn, Quaternion.identity);
            DollAnimator da = newDoll.GetComponent<DollAnimator>();
            da.setScale(Mathf.Pow(1.2f, dolls.Count));
            da.canvas = canvas;
            da.labelText = obj.GetComponent<TEXDraw>().text;
            da.close(incrementStage);
            dolls.Push(newDoll);

           
        }

    }
    public void incrementStage()
    {
        currentStage++;
        if (currentStage >= currentQuestion.solution.Length)
        {

            timers.Add(new Timer(3, cleanUpQuestion));
            
        }
    }
    public void cleanUpQuestion()
    {
        popAnswers();

        while (dolls.Count > 0)
        {
            GameObject d = dolls.Pop();
            Destroy(d);
            
        }
        questions.answered.Add(currentQuestion);

        pickQuestion();
    }
    public static QuestionBank loadJSON(string text)
    {
        QuestionBank bank = JsonUtility.FromJson<QuestionBank>(text);
        bank.init();
        return bank;
    }
}
