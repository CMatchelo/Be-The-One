using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;


public class CalendarManager
 : MonoBehaviour
{
    [Header("UI Canvas")]
    public ScrollRect calendarTable;
    public ScrollRect raceTable;


    [Header("UI Btns and Dropdowns")]

    [Header("UI Texts")]
    public Transform calendarView;
    public Transform raceResultView;
    public GameObject raceRowPrefab;
    public GameObject standingLinePrefab;



    private void Awake()
    {

    }
    private void Start()
    {
        LoadDatabases();
        PopulateCalendar();
    }

    private void LoadDatabases()
    {
        TextAsset teamsLocal = Resources.Load<TextAsset>("TeamsDatabase");
        teamsList = JsonUtility.FromJson<TeamsList>(teamsLocal.text);

        string pathDrivers = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "activeDriversList.json"
        );
        string driversLocal = File.ReadAllText(pathDrivers);
        driversList = JsonUtility.FromJson<DriversList>(driversLocal);

        string pathRaces = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "races.json"
        );
        string racesLocal = File.ReadAllText(pathRaces);
        raceResultList = JsonUtility.FromJson<RaceResultList>(racesLocal);
    }

    void PopulateCalendar()
    {
        foreach (Transform child in calendarView) Destroy(child.gameObject);

        foreach (var race in raceResultList.races)
        {
            GameObject row = Instantiate(raceRowPrefab, calendarView, false);

            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

            string winner = race.driverResults[0].driver.firstName + " " + race.driverResults[0].driver.lastName;
            //string team = race.driverResults[0].driver.team;
            string team = teamsList.teams.Find(team => team.id == race.driverResults[0].driver.teamId)?.teamName;
            string driver = race.driverResults[0].driver.firstName;

            Debug.Log(race.driverResults[0].driver.firstName);

            texts[0].text = race.trackName;
            texts[1].text = winner;
            texts[2].text = team;

            // Adicionar listener ao botão
            Button btn = row.GetComponent<Button>();
            RaceResult raceCopy = race; // evitar closure
            btn.onClick.AddListener(() => OnRaceClicked(raceCopy));
        }
    }

    void OnRaceClicked(RaceResult race)
    {
        foreach (Transform child in raceResultView) Destroy(child.gameObject);
        foreach (var driverResult in race.driverResults)
        {
            GameObject standingRow = Instantiate(standingLinePrefab, raceResultView, false);
            var texts = standingRow.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = driverResult.position.ToString();
            texts[1].text = driverResult.driver.firstName + " " + driverResult.driver.lastName;
            texts[2].text = teamsList.teams.Find(team => team.id == driverResult.driver.teamId)?.teamName;
            texts[3].text = driverResult.bestLap.ToString();
            texts[4].text = driverResult.totalTime.ToString();
            Debug.Log($"Função executada! Corrida: {race.trackName}");
        }
        calendarTable.gameObject.SetActive(false);
        raceTable.gameObject.SetActive(true);
    }

    public void CloseRaceResult()
    {
        calendarTable.gameObject.SetActive(true);
        raceTable.gameObject.SetActive(false);
    }

    string GetTeamName(int teamId)
    {
        if (teamsList == null || teamsList.teams == null) return $"Team {teamId}";

        var team = teamsList.teams.Find(t => t.id == teamId);
        return team != null ? team.teamName : $"Team {teamId}";
    }

    string GetDriverName(int driverId)
    {
        if (driversList == null || driversList.drivers == null) return $"Driver {driverId}";
        var driver = driversList.drivers.Find(d => d.id == driverId);
        return driver != null ? $"{driver.firstName} {driver.lastName}" : $"Driver {driverId}";
    }
}