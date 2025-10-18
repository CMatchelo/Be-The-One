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

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        nextRaceText.text = string.Join("\n", logMessages);
    }
}