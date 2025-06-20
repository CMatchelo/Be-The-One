using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using TMPro;
using System.Linq;

public class MenuManager : MonoBehaviour
{
    // UI Elements
    public TMP_InputField circuitLengthInput;
    public TMP_InputField lapsInput;
    public TMP_Dropdown tireDropdown;
    public TMP_Dropdown trackDropdown;
    public Button startButton;
    public TMP_Text outputText;
    public TMP_Text pitText;
    public TMP_Text lapResume;
    private DriversList driversList;
    private TracksList tracksList;
    private TeamsList teamsList;
    private Track selectedTrack;
    private Coroutine _simulationCoroutine;
    private string _currentTireType;

    private bool isSimulating = false;
    private List<string> logMessages = new List<string>();

    void Start()
    {
        // Initialize UI
        startButton.onClick.AddListener(StartSimulation);

        // Populate tire dropdown
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

        float trackFactor = CalculateTrackFactor(circuitLength);
        List<CarSimulationState> cars = new List<CarSimulationState>();
        for (int i = 0; i < 20; i++)
        {
            Driver driver = driversList.drivers[i];
            Team team = teamsList.teams.Find(t => t.id == driver.teamId);
            Debug.Log($"Driver: {driver.firstName} {driver.lastName}, Team: {team.teamName}");
            float carFactor = PerformanceCalculator.CalculateCarFactor(driver, team, selectedTrack);
            cars.Add(new CarSimulationState(startingTire, carFactor, driver.lastName, team.teamName));
        }

        AddLogMessage("=== RACE STARTED ===");

        bool raceOngoing = true;

        while (raceOngoing)
        {

            raceOngoing = false;
            List<(int carIndex, string driver, float lapTime, float totalTime)> lapResults = new();
            logMessages.Clear();
            for (int i = 0; i < cars.Count; i++)
            {

                CarSimulationState car = cars[i];
                if (car.lapsCompleted >= totalLaps)
                    continue;

                raceOngoing = true;

                float lapWear = TireDatabase.TireParameters[car.currentTire]["WearMod"] *
                                (1 + Mathf.Pow(circuitLength / 5300f, 4)) *
                                Mathf.Pow(car.totalLapTyre + 1, TireDatabase.TireParameters[car.currentTire]["WearProg"]);
                car.totalWear += lapWear;

                float tireFactor = ((car.totalLapTyre + 1) * TireDatabase.TireParameters[car.currentTire]["BaseWear"]) +
                                 TireDatabase.TireParameters[car.currentTire]["WearCoef"] * trackFactor * car.totalLapTyre * (car.totalLapTyre + 1) /
                                 TireDatabase.TireParameters[car.currentTire]["TireCoef"] +
                                 TireDatabase.TireParameters[car.currentTire]["StartDelta"];

                float lapTime = car.carFactor + tireFactor + UnityEngine.Random.Range(-0.5f, 0.5f);
                car.totalTime += lapTime;

                if (car.totalWear >= 100f && car.lapsCompleted < totalLaps - 1)
                {
                    /* yield return new WaitForSeconds(0.1f); */ // delay dram�tico
                    AddPitMessage($"Car {i + 1} ({car.driver}) is pitting for tire change at lap {car.lapsCompleted + 1}!");
                    // Ajsutar estrategias
                    if (car.currentTire == "Soft") car.currentTire = "Medium";
                    else if (car.currentTire == "Medium") car.currentTire = "Soft";
                    else car.currentTire = "Soft";

                    car.totalWear = 0f;
                    car.totalLapTyre = -1;
                    car.totalTime += 30f; // tempo de pit stop
                }

                car.totalLapTyre++;
                car.lapsCompleted++;
                lapResults.Add((carIndex: i, driver: car.driver, lapTime: lapTime, totalTime: car.totalTime));
            }
            var orderedLapResults = lapResults.OrderBy(r => r.totalTime).ToList();
            AddLogMessage($"=== LAP {cars[0].lapsCompleted} RESULTS ===");
            foreach (var result in orderedLapResults)
            {
                string driverName = $"Car {result.carIndex + 1}";
                AddLogMessage($"{result.driver} -> Lap Time: {result.lapTime:F2}s | Total: {result.totalTime:F2}s");
            }
            yield return new WaitForSeconds(0.25f); // tempo entre voltas de todos os carros
        }
        isSimulating = false;

        RaceResult raceResult = new RaceResult
        {
            trackName = selectedTrack.circuitName,
            date = "2025",
            results = new List<DriverResult>()
        };
        foreach (var car in cars.OrderBy(c => c.totalTime))
        {

            raceResult.results.Add(new DriverResult
            {
                driverName = car.driver,
                teamName = car.team,
                totalTime = car.totalTime
            });
        }
        string temp = JsonUtility.ToJson(raceResult, true);
        Debug.Log($"Reesultado: {temp}");

        RaceSaveSystem.SaveRace(raceResult);

        // Atualiza o campeonato (ordene os pilotos pela posição de chegada)
        var orderedResults = raceResult.results.OrderBy(r => r.totalTime).ToList();
        RaceSaveSystem.UpdateChampionship(orderedResults);
    }


    private float CalculateTrackFactor(float circuitLength)
    {
        return (1 + (circuitLength / 5300f)) / 10f;
    }

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        outputText.text = string.Join("\n", logMessages);
    }

    private void AddPitMessage(string message)
    {
        logMessages.Add(message);
        pitText.text = string.Join("\n", logMessages);
    }

    void LoadDrivers()
    {
        string path = Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId, "activeDriversList.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            driversList = JsonUtility.FromJson<DriversList>(json);
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