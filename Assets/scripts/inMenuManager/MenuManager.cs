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
    public Button startRaceButton;
    public TMP_Text nextRaceText;
    public TMP_Text driversStandings;
    

    private List<string> logMessages = new List<string>();

    private void Awake()
    {
        LoadUtility.LoadGame(SaveSession.CurrentGameData.saveId); // Fix id load
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
        for (int i = 0; i < GoToPageBtn.Count; i++)
        {
            int capturedIndex = i + 1;
            GoToPageBtn[i].onClick.AddListener(() => GoToPage(capturedIndex));
        }
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

    void GoToPage(int eventNumber)
    {
        if (eventNumber == 0)
        {
            MenuPanel.SetActive(false);
            StandingsPanel.SetActive(true);
        }
        else if (eventNumber == 1)
        {
            MenuPanel.SetActive(false);
            CalendarPanel.SetActive(true);
        }
        else if (eventNumber == 2)
        {
            MenuPanel.SetActive(false);
            PracticePanel.SetActive(true);
        }
        else if (eventNumber == 3)
        {
            MenuPanel.SetActive(false);
            PersonalLifePanel.SetActive(true);
        }
    }

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        nextRaceText.text = string.Join("\n", logMessages);
    }
}