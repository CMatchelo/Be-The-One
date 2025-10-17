using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class InterviewManager : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject MenuPanel;
    public GameObject InterviewPanel;

    [Header("UI Btns")]
    public Button[] interviewAnswers;

    [Header("UI Texts")]
    public TMP_Text interviewQuestion;
    public TMP_Text reporterInfo;

    private OnSignContractQuestions onSignContractQuestions;
    private PressList pressList;
    private List<Question> selected = new();
    private List<int> indices = new();
    private int currentQuestionIndex = 0;

    void Start()
    {
        LoadDatabases();
        GetRandomQuestions("fightingForPoints", SaveSession.CurrentGameData.profile.lastResults, SaveSession.CurrentGameData.profile.driver.role);
        for (int i = 0; i < interviewAnswers.Length; i++)
        {
            int buttonIndex = i;
            interviewAnswers[i].onClick.AddListener(() => QuestionAnswer(buttonIndex));
        }
    }

    void QuestionAnswer(int index)
    {
        int[] traits = new int[3];
        traits[index] += 2;

        for (int i = 0; i < traits.Length; i++)
        {
            if (i != index)
                traits[i] -= 1;
        }

        var profile = SaveSession.CurrentGameData.profile;

        profile.ambitious = Mathf.Clamp(profile.ambitious + traits[0], 0, 20);
        profile.charismatic = Mathf.Clamp(profile.charismatic + traits[1], 0, 20);
        profile.loyal = Mathf.Clamp(profile.loyal + traits[2], 0, 20);

        SaveSession.CurrentGameData.profile = profile;

        currentQuestionIndex += 1;
        PopulateQuestionUI(currentQuestionIndex);
    }

    void PopulateQuestionUI(int index)
    {
        if (currentQuestionIndex >= selected.Count)
        {
            SaveUtility.UpdateProfile();
            InterviewPanel.SetActive(false);
            MenuPanel.SetActive(true);
            StartCoroutine(DisableGameObject());
            return;
        }
        int pressIndex = RandomNumberGenerator.GetRandomBetween(0, 9);
        int reporterIndex = RandomNumberGenerator.GetRandomBetween(0, 2);
        string pressOutlet = pressList.pressOutlet[pressIndex].outlet;
        string reporterName = pressList.pressOutlet[pressIndex].reporters[reporterIndex];

        reporterInfo.text = reporterName + " from " + pressOutlet;
        interviewQuestion.text = selected[index].question;
        for (int i = 0; i < interviewAnswers.Length; i++)
        {
            TextMeshProUGUI buttonText = interviewAnswers[i].GetComponentInChildren<TextMeshProUGUI>();
            switch (i)
            {
                case 0: buttonText.text = selected[index].answers[i]; break;
                case 1: buttonText.text = selected[index].answers[i]; break;
                case 2: buttonText.text = selected[index].answers[i]; break;
            }
        }
    }
    void GetRandomQuestions(string teamStatus, int lastResult, int driverType)
    {

        if (onSignContractQuestions == null)
        {
            Debug.LogError("Dados de perguntas não carregados.");
            return;
        }
        if (onSignContractQuestions.teamResults == null)
        {
            Debug.LogError("Dados de perguntas nao tem teamResults");
            return;
        }

        // Obter o grupo de resultados da equipe
        ResultsGroup group = teamStatus switch
        {
            "midfieldRunners" => onSignContractQuestions.teamResults.midfieldRunners,
            "fightingForPoints" => onSignContractQuestions.teamResults.fightingForPoints,
            "fightingForPodium" => onSignContractQuestions.teamResults.fightingForPodium,
            "fightingForWins" => onSignContractQuestions.teamResults.fightingForWins,
            _ => null
        };

        if (group == null)
        {
            Debug.LogWarning($"Perfil de equipe '{teamStatus}' não encontrado.");
            return;
        }

        // Classifica o resultado em uma das faixas
        DriverTypeGroup driverGroup;
        if (lastResult <= 25)
            driverGroup = group.badResults;
        else if (lastResult <= 50)
            driverGroup = group.averageResults;
        else if (lastResult <= 75)
            driverGroup = group.goodResults;
        else
            driverGroup = group.excelentResults;

        if (driverGroup == null)
        {
            Debug.LogWarning("Grupo de resultados não encontrado.");
            return;
        }

        // Seleciona a lista correta: race ou test
        var questionList = driverType == 2
            ? driverGroup.test?.questions
            : driverGroup.race?.questions;

        if (questionList == null || questionList.Count < 2)
        {
            Debug.LogWarning("Lista de perguntas vazia ou com menos de 2 itens.");
            return;
        }

        while (selected.Count < 2)
        {
            int idx = UnityEngine.Random.Range(0, questionList.Count);
            if (!indices.Contains(idx))
            {
                selected.Add(questionList[idx]);
                indices.Add(idx);
            }
        }
        PopulateQuestionUI(currentQuestionIndex);
    }
    void LoadDatabases()
    {
        TextAsset onSignContractQuestionsJson = Resources.Load<TextAsset>("SignContractQuestions");
        onSignContractQuestions = JsonUtility.FromJson<OnSignContractQuestions>(onSignContractQuestionsJson.text);

        TextAsset pressListJson = Resources.Load<TextAsset>("PressOutlets");
        pressList = JsonUtility.FromJson<PressList>(pressListJson.text);
    }
    private IEnumerator DisableGameObject()
    {
        yield return null; // Espera 1 frame
        gameObject.SetActive(false);
    }
}
