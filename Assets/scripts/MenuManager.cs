using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;


public class RaceSimulator : MonoBehaviour
{
    // UI Elements
    public Button startRaceButton;
    public TMP_Text nextRaceText;
    public TMP_Text driversStandings;

    private List<string> logMessages = new List<string>();

    private void Start()
    {
        ChampionshipManager.Initialize();
        UpdateNextRaceInfo();
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

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        nextRaceText.text = string.Join("\n", logMessages);
    }
}