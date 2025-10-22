using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;


public class MenuManager : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject MenuPanel;
    public GameObject StandingsPanel;
    public GameObject CalendarPanel;
    public GameObject PracticePanel;
    public GameObject PersonalLifePanel;
    public GameObject ContractNegotiationPanel;
    public GameObject SponsorNegotiationPanel;
    public SponsorshipManager sponsorshipManager;


    [Header("UI Btns and Dropdowns")]
    public List<Button> GoToPageBtn;
    public Button backStandingsBtn;
    public Button backCalendarBtn;
    public Button backPracticeBtn;
    public Button backPersonalLifeBtn;
    public Button startRaceButton;
    public TMP_Text nextRaceText;

    [Header("UI Texts")]



    private Dictionary<string, GameObject> panels;

    private List<string> logMessages = new List<string>();

    private void Awake()
    {
        LoadUtility.LoadGame(SaveSession.CurrentSaveId); // Fix id load
        //LoadUtility.LoadGame("Chays_c4wIYT");
        panels = new Dictionary<string, GameObject>
        {
            { "StandingsPanel", StandingsPanel },
            { "CalendarPanel", CalendarPanel },
            { "PracticePanel", PracticePanel },
            { "PersonalLifePanel", PersonalLifePanel }
        };
        CheckNegotiations();
    }
    private void Start()
    {
        ChampionshipManager.Initialize();
        UpdateNextRaceInfo();
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

    public void CheckNegotiations()
    {
        if (SaveSession.CurrentGameData.profile.driver.yearsOfContract <= 0)
        {
            ContractNegotiationPanel.SetActive(true);
            MenuPanel.SetActive(false);
        }
        else if (SaveSession.CurrentGameData.profile.sponsorMaster.remainingRaces <= 0)
        {
            sponsorshipManager.LoadDatabase();
            SponsorNegotiationPanel.SetActive(true);
            MenuPanel.SetActive(false);
        }
    }

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        nextRaceText.text = string.Join("\n", logMessages);
    }
}