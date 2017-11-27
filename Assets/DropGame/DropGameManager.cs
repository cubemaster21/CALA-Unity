using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropGameManager : MonoBehaviour {

    int lives = 3;
    List<GameObject> fallingObjects;
    string currentEntry;
    public GameObject entryDisplay;

    public GameObject objectPrefab;
    static float spawnFullTimer = 2f;
    float spawnTimer = spawnFullTimer;
	// Use this for initialization
	void Start () {
        fallingObjects = new List<GameObject>();
        currentEntry = "";

	}
	//adapteed from original ios code
    public GameObject getRandomQuestion()
    {
        int a = Random.Range(1, 10);
        int b = Random.Range(2, 10);

        if(a % b == 0)
        {
            return getRandomQuestion();
        }
        int g = GCD(a, b);
        a /= g;
        b /= g;

        int style = Random.Range(0, 3);
        if(b == a)
        {
            style = 1;
        }

        string latex = "";
        string answer = "";
        string u = "x";
        switch (style)
        {
            case 0:
                if(a == 1)
                {
                    latex = "\\sqrt" + (b == 2 ? "" : ("[" + b + "]")) + "{" + u + "}";
                } else
                {
                    latex = "\\sqrt" + (b == 2 ? "" : ("[" + b + "]")) + "{" + u + "^{" + a + "}}";
                }
                answer = a + "/" + b;
                break;
            case 1:
                if (a == 1)
                {
                    latex = "\\frac{1}{" + u + "}";
                }
                else
                {
                    latex = "\\frac{1}{" + u + "^{" + a + "}}";
                }
                answer = "-" + a;
                break;
            case 2:
                if (a == 1)
                {
                    latex = "\\frac{1}{\\sqrt" + (b == 2 ? "" : ("[" + b + "]")) + "{" + u + "}";
                }
                else
                {
                    latex = "\\frac{1}{\\sqrt" + (b == 2 ? "" : ("[" + b + "]")) + "{" + u + "^{" + a + "}}";
                }
                answer = "-" + a + "/" + b;
                break;
        }
        float range = 2.4f - -8f;
        GameObject newObj = Instantiate(objectPrefab, new Vector3(Random.value * range - range / 2, 6, -2), Quaternion.identity);
        newObj.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TEXDraw>().text = latex;
        newObj.GetComponent<FallingObjectSync>().answer = answer;
        fallingObjects.Add(newObj);
        return newObj;
    }
    public void appendAnswer(string s)
    {
        currentEntry = currentEntry + s;
        entryDisplay.GetComponent<TEXDraw>().text = "x^{" + currentEntry + "}";
        checkAnswers();
    }
    public void negateAnswer()
    {
        if (currentEntry.StartsWith("-"))
        {
            currentEntry = currentEntry.Substring(1);
        } else
        {
            currentEntry = "-" + currentEntry;
        }
        entryDisplay.GetComponent<TEXDraw>().text = "x^{" + currentEntry + "}";
        checkAnswers();
    }
    public void checkAnswers()
    {
        bool shouldClear = false;
        for(int i = 0;i < fallingObjects.Count; i++)
        {
            FallingObjectSync fos = fallingObjects[i].GetComponent<FallingObjectSync>();
            if (fos.answer.Equals(currentEntry))
            {
                Destroy(fallingObjects[i]);
                fallingObjects.RemoveAt(i);
                shouldClear = true;
            }
        }
        if (shouldClear)
            clearEntry();
    }
    public void clearEntry()
    {
        currentEntry = "";
        entryDisplay.GetComponent<TEXDraw>().text = "x^{" + currentEntry + "}";
    }

	// Update is called once per frame
	void Update () {
        spawnTimer -= Time.deltaTime;
        if(spawnTimer <= 0)
        {
            spawnTimer = spawnFullTimer;
            getRandomQuestion();
        }
	}
    static int GCD(int a, int b)
    {
        int Remainder;

        while (b != 0)
        {
            Remainder = a % b;
            a = b;
            b = Remainder;
        }

        return a;
    }
}
