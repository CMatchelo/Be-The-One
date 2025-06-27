using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Question
{
    public string question;
    public List<string> answers;
}

[Serializable]
public class QuestionList
{
    public List<Question> questions;
}

[Serializable]
public class DriverTypeGroup
{
    public QuestionList race;
    public QuestionList test;
}

[Serializable]
public class ResultsGroup
{
    public DriverTypeGroup badResults;
    public DriverTypeGroup averageResults;
    public DriverTypeGroup goodResults;
    public DriverTypeGroup excelentResults;
}

[Serializable]
public class TeamResults
{
    public ResultsGroup midfieldRunners;
    public ResultsGroup fightingForPoints;
    public ResultsGroup fightingForPodium;
    public ResultsGroup fightingForWins;
}

[Serializable]
public class OnSignContractQuestions
{
    public TeamResults teamResults;
}