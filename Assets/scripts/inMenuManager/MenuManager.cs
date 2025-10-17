using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;


public class RaceSimulator : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject MenuPanel;
    public GameObject StandingsPanel;
    public GameObject CalendarPanel;
    public GameObject PracticePanel;
    public GameObject PersonalLifePanel;
    public GameObject ContractNegotiationPanel;


    [Header("UI Btns and Dropdowns")]
    public List<Button> GoToPageBtn;
    public Button backStandingsBtn;
    public Button backCalendarBtn;
    public Button backPracticeBtn;
    public Button backPersonalLifeBtn;
    public Button startRaceButton;
    public TMP_Text nextRaceText;

    [Header("UI Texts")]
    public TMP_Text driversStandingsTable;
    public TMP_Text teamsStandingsTable;


    private Dictionary<string, GameObject> panels;

    private List<string> logMessages = new List<string>();

    private void Awake()
    {
        LoadUtility.LoadGame(SaveSession.CurrentGameData.saveId); // Fix id load
        panels = new Dictionary<string, GameObject>
        {
            { "StandingsPanel", StandingsPanel },
            { "CalendarPanel", CalendarPanel },
            { "PracticePanel", PracticePanel },
            { "PersonalLifePanel", PersonalLifePanel }
        };
        if (SaveSession.CurrentGameData.profile.driver.yearsOfContract <= 0)
        {
            ContractNegotiationPanel.SetActive(true);
            MenuPanel.SetActive(false);
        }
    }
    private void Start()
    {
        ChampionshipManager.Initialize();
        UpdateNextRaceInfo();
        PopulateStandings();
        GoToPageBtn[0].onClick.AddListener(() => GoToPage("StandingsPanel"));
        GoToPageBtn[1].onClick.AddListener(() => GoToPage("CalendarPanel"));
        GoToPageBtn[2].onClick.AddListener(() => GoToPage("PracticePanel"));
        GoToPageBtn[3].onClick.AddListener(() => GoToPage("PersonalLifePanel"));
        backStandingsBtn.onClick.AddListener(() => BackToMenu("StandingsPanel"));
        backCalendarBtn.onClick.AddListener(() => BackToMenu("CalendarPanel"));
        backPracticeBtn.onClick.AddListener(() => BackToMenu("PracticePanel"));
        backPersonalLifeBtn.onClick.AddListener(() => BackToMenu("PersonalLifePanel"));
    }
    void StartSimulation()
    {

    }

    private void UpdateNextRaceInfo()
    {
        var nextRace = ChampionshipManager.GetNextRace();

        if (nextRace != null)
        {
            AddLogMessage($"PRÓXIMA CORRIDA: {nextRace.circuitName.ToUpper()} - {nextRace.totalLaps} VOLTAS");
            startRaceButton.interactable = true;
        }
        else
        {
            nextRaceText.text = "CAMPEONATO COMPLETO!";
            startRaceButton.interactable = false;
        }
    }

    public void StartRace()
    {
        Debug.Log("Iniciando corrida...");
        SceneManager.LoadScene("RaceScene");
    }

    void GoToPage(string panel)
    {
        if (panels.ContainsKey(panel))
        {
            MenuPanel.SetActive(false);
            panels[panel].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Painel '{panel}' não encontrado!");
        }
    }

    void BackToMenu(string panel)
    {
        if (panels.ContainsKey(panel))
        {
            MenuPanel.SetActive(true);
            panels[panel].SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Painel '{panel}' não encontrado!");
        }
    }

    private void PopulateStandings()
    {
        /* string path = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "championship_driversStandings.json"
        );

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Arquivo não encontrado: {path}");
            driversStandingsTable.text = "Nenhum dado encontrado.";
            return;
        }
        string json = File.ReadAllText(path);

        TextAsset teamsLocal = Resources.Load<TextAsset>("TeamsDatabase");
        teamsList = JsonUtility.FromJson<TeamsList>(teamsLocal.text);

        // Caso o JSON seja uma lista pura (ex: [ { driverId: 1, ... }, { ... } ])
        List<DriverStanding> standings = JsonUtilityWrapper.FromJsonList<DriverStanding>(json);
        // Montar o texto
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < standings.Count; i++)
        {
            var s = standings[i];
            Debug.Log(s.driverId);
            sb.AppendLine($"{i + 1} - {s.driverId} - {s.teamId} - {s.points}");
        }
        driversStandingsTable.text = sb.ToString(); */
    }

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        nextRaceText.text = string.Join("\n", logMessages);
    }
}