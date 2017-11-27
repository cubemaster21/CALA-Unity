using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class Question
{
    public string question;
    public string[] answers;
    public int[] solution;
    public string solutionArray;
    public void init()
    {
        string[] nodes  = solutionArray.Split(',');
        solution = Array.ConvertAll<string, int>(nodes, int.Parse);
    }
}
