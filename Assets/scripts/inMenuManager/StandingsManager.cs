using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;


public class StandingManager
 : MonoBehaviour
{
    [Header("UI Canvas")]



    [Header("UI Btns and Dropdowns")]

    [Header("UI Texts")]
    public TMP_Text driversStandingsPos;
    public TMP_Text driversStandingsName;
    public TMP_Text driversStandingsTeam;
    public TMP_Text driversStandingsPts;
    public TMP_Text teamsStandingsTable;

    public DriversChampionshipStatus driversChampionshipStatus;
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
        string path = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "championship_driversStandings.json"
        );

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Arquivo não encontrado: {path}");
            //driversStandingsPos.text = "Nenhum dado encontrado.";
            return;
        }
        string json = File.ReadAllText(path);

        driversChampionshipStatus = JsonUtility.FromJson<DriversChampionshipStatus>(json);

        if (driversChampionshipStatus == null || driversChampionshipStatus.driverStandings == null || driversChampionshipStatus.driverStandings.Count == 0)
        {
            driversStandingsPos.text = "Tabela vazia.";
            return;
        }
        System.Text.StringBuilder position = new System.Text.StringBuilder();
        System.Text.StringBuilder driverName = new System.Text.StringBuilder();
        System.Text.StringBuilder teamName = new System.Text.StringBuilder();
        System.Text.StringBuilder points = new System.Text.StringBuilder();
        for (int i = 0; i < driversChampionshipStatus.driverStandings.Count; i++)
        {
            var standing = driversChampionshipStatus.driverStandings[i];
            string currentTeam = GetTeamName(standing.teamId);
            string currentDriver = GetDriverName(standing.driverId);
            position.AppendLine($"{i + 1}");
            driverName.AppendLine($"{currentDriver}");
            teamName.AppendLine($"{currentTeam}");
            points.AppendLine($"{standing.points} pts");
        }
        driversStandingsPos.text = position.ToString();
        driversStandingsName.text = driverName.ToString();
        driversStandingsTeam.text = teamName.ToString();
        driversStandingsPts.text = points.ToString();
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