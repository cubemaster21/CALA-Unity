using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QuestionBank
{
    public List<Question> answered = new List<Question>();
    public Question[] list;
    public Question getRandomQuestion(bool mustBeUnanswered)
    {
        if (mustBeUnanswered && answered.Count == list.Length) return null;
        int tries = 0;
        Debug.Log("Picking non-answered: " + answered.Count + "/" + list.Length);
        Question q;
        do
        {
            q = list[Random.Range(0, list.Length)];
            tries++;
        } while (answered.Contains(q) && mustBeUnanswered && tries < 100);
        Debug.Log("Tries: " + tries);
        return q;
    }
    public void init()
    {
        for(int i = 0;i < list.Length; i++)
        {
            list[i].init();
        }
    }
}
