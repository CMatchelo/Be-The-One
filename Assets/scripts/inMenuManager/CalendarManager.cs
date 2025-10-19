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



    [Header("UI Btns and Dropdowns")]

    [Header("UI Texts")]
    public TMP_Text gpName;
    public TMP_Text firstPlace;
    public TMP_Text fastestLap;
    public TMP_Text driverFastestLap;

    public TeamsList teamsList;
    public DriversList driversList;


    private void Awake()
    {

    }
    private void Start()
    {
        LoadDatabases();
        PopulateStandings();
    }

    private void LoadDatabases()
    {
        TextAsset teamsLocal = Resources.Load<TextAsset>("TeamsDatabase");
        teamsList = JsonUtility.FromJson<TeamsList>(teamsLocal.text);

        string path = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "activeDriversList.json"
        );
        string driversLocal = File.ReadAllText(path);
        driversList = JsonUtility.FromJson<DriversList>(driversLocal);
    }

    private void PopulateStandings()
    {
        

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