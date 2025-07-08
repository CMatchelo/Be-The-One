using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using TMPro;
using System.Linq;

public class RaceManager : MonoBehaviour
{
    // UI Elements
    public TMP_InputField lapsInput;
    public TMP_Dropdown tireDropdown;
    public Button goToRaceBtn;
    public TMP_Text outputText;
    private DriversList driversList;
    private TracksList tracksList;
    private TeamsList teamsList;
    private Track selectedTrack;

    private bool isSimulating = false;
    private List<string> logMessages = new List<string>();

    private void Awake()
    {
        LoadUtility.LoadGame("Cicero_g15866"); // Fix id load
    }
    void Start()
    {
        // Initialize UI
        goToRaceBtn.onClick.AddListener(StartSimulation);

        // Populate tire dropdown
        ChampionshipManager.Initialize();
        tireDropdown.ClearOptions();
        tireDropdown.AddOptions(new List<string>(TireDatabase.TireParameters.Keys));

        LoadDrivers();
        LoadTracks();
        LoadTeams();

        selectedTrack = ChampionshipManager.GetNextRace();
        Debug.Log(selectedTrack.circuitName + " - " + selectedTrack.totalLaps + " Laps");
    }
    void StartSimulation()
    {
        if (isSimulating) return;

        float circuitLength = selectedTrack.circuitLength;
        int totalLaps = selectedTrack.totalLaps;
        string currentTire = tireDropdown.options[tireDropdown.value].text;

        StartCoroutine(RunRaceSimulation(circuitLength, totalLaps, currentTire));
    }

    private IEnumerator RunRaceSimulation(float circuitLength, int totalLaps, string startingTire)
    {
        isSimulating = true;
        outputText.text = "";

        float trackFactor = RaceSimulatorUtility.CalculateTrackFactor(circuitLength);

        // Cria o grid com 20 carros
        var cars = RaceSimulatorUtility.CreateInitialGrid(
            driversList.drivers,
            teamsList.teams,
            startingTire,
            selectedTrack
        );

        AddLogMessage("=== RACE STARTED ===");

        bool raceOngoing = true;

        while (raceOngoing)
        {
            raceOngoing = false;
            List<(int carIndex, string driver, float lapTime, float totalTime)> lapResults = new();
            logMessages.Clear();

            for (int i = 0; i < cars.Count; i++)
            {
                var car = cars[i];

                // Simula a volta usando a nova função
                bool completedLap = RaceSimulatorUtility.SimulateLap(
                    car,
                    trackFactor,
                    circuitLength,
                    totalLaps,
                    i,
                    AddPitMessage // função de log para pit stop
                );

                if (completedLap)
                {
                    raceOngoing = true;
                    float lapTime = car.totalTime - car.previousTotalTime;
                    car.previousTotalTime = car.totalTime;

                    lapResults.Add((i, car.driver, lapTime, car.totalTime));
                }
            }

            // Ordena e exibe resultados da volta
            var orderedLapResults = lapResults.OrderBy(r => r.totalTime).ToList();
            AddLogMessage($"=== LAP {cars[0].lapsCompleted} RESULTS ===");

            foreach (var result in orderedLapResults)
            {
                AddLogMessage($"{result.driver} -> Lap Time: {result.lapTime:F2}s | Total: {result.totalTime:F2}s");
            }

            yield return new WaitForSeconds(0.25f);
        }

        isSimulating = false;

        // Monta resultado final
        RaceResult raceResult = new RaceResult
        {
            trackName = selectedTrack.circuitName,
            date = "2025",
            results = cars.OrderBy(c => c.totalTime)
                          .Select(c => new DriverResult
                          {
                              driverName = c.driver,
                              teamName = c.team,
                              totalTime = c.totalTime
                          }).ToList()
        };

        string temp = JsonUtility.ToJson(raceResult, true);
        Debug.Log($"Resultado: {temp}");

        RaceSaveSystem.SaveRace(raceResult);

        // Atualiza o campeonato com os resultados ordenados
        RaceSaveSystem.UpdateChampionship(raceResult.results);
    }

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        outputText.text = string.Join("\n", logMessages);
    }

    private void AddPitMessage(string message)
    {
        logMessages.Add(message);
    }

    void LoadDrivers()
    {
        string path = Path.Combine(Application.persistentDataPath, "saves", "Cicero_g15866", "activeDriversList1.json"); // fix status
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            driversList = JsonUtility.FromJson<DriversList>(json);
            driversList.drivers = driversList.drivers
                .Where(driver => driver.role == 0 || driver.role == 1)
                .ToList();
            Debug.Log(driversList.drivers.Count + " drivers loaded from " + path);
        }
        else
        {
            Debug.LogWarning($"Arquivo não encontrado em: {path}");
        }
    }

    void LoadTracks()
    {
        TextAsset tracksLocal = Resources.Load<TextAsset>("TracksDatabase");
        tracksList = JsonUtility.FromJson<TracksList>(tracksLocal.text);
    }

    void LoadTeams()
    {
        TextAsset teamsLocal = Resources.Load<TextAsset>("TeamsDatabase");
        teamsList = JsonUtility.FromJson<TeamsList>(teamsLocal.text);
    }

    void UpdateLapsInput()
    {
        lapsInput.text = selectedTrack.totalLaps.ToString();
    }

}