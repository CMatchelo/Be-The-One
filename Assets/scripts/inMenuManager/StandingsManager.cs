using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;


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
    public TMP_Text teamsStandingsPos;
    public TMP_Text teamsStandingsTeam;
    public TMP_Text teamsStandingsPts;

    public DriversChampionshipStatus driversChampionshipStatus;
    public TeamsChampionshipStatus teamsChampionshipStatus;
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
        string pathDrivers = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "championship_driversStandings.json"
        );
        string pathTeams = Path.Combine(
            Application.persistentDataPath,
            "saves",
            SaveSession.CurrentSaveId,
            "championship_teamsStandings.json"
        );

        if (!File.Exists(pathDrivers))
        {
            Debug.LogWarning($"Arquivo não encontrado: {pathDrivers}");
            //driversStandingsPos.text = "Nenhum dado encontrado.";
            return;
        }
        if (!File.Exists(pathTeams))
        {
            Debug.LogWarning($"Arquivo não encontrado: {pathTeams}");
            //driversStandingsPos.text = "Nenhum dado encontrado.";
            return;
        }

        string jsonDrivers = File.ReadAllText(pathDrivers);
        string jsonTeams = File.ReadAllText(pathTeams);

        driversChampionshipStatus = JsonUtility.FromJson<DriversChampionshipStatus>(jsonDrivers);
        teamsChampionshipStatus = JsonUtility.FromJson<TeamsChampionshipStatus>(jsonTeams);

        if (driversChampionshipStatus == null || driversChampionshipStatus.driverStandings == null || driversChampionshipStatus.driverStandings.Count == 0)
        {
            driversStandingsName.text = "Tabela vazia.";
            return;
        }
        if (teamsChampionshipStatus == null || teamsChampionshipStatus.teamStandings == null || teamsChampionshipStatus.teamStandings.Count == 0)
        {
            teamsStandingsTeam.text = "Tabela vazia.";
            return;
        }

        var posDriver = new System.Text.StringBuilder();
        var nameDriver = new System.Text.StringBuilder();
        var teamDriver = new System.Text.StringBuilder();
        var ptsDriver = new System.Text.StringBuilder();

        driversChampionshipStatus.driverStandings.Sort(
            (a, b) => b.points.CompareTo(a.points)
        );
        teamsChampionshipStatus.teamStandings.Sort(
            (a, b) => b.points.CompareTo(a.points)
        );

        foreach (var (standing, i) in driversChampionshipStatus.driverStandings.Select((s, i) => (s, i)))
        {
            posDriver.AppendLine($"{i + 1}");
            nameDriver.AppendLine(GetDriverName(standing.driverId));
            teamDriver.AppendLine(GetTeamName(standing.teamId));
            ptsDriver.AppendLine($"{standing.points} pts");
        }

        var posTeam = new System.Text.StringBuilder();
        var team = new System.Text.StringBuilder();
        var ptsTeam = new System.Text.StringBuilder();

        foreach (var (standing, i) in teamsChampionshipStatus.teamStandings.Select((s, i) => (s, i)))
        {
            posTeam.AppendLine($"{i + 1}");
            team.AppendLine(GetTeamName(standing.teamId));
            ptsTeam.AppendLine($"{standing.points} pts");
        }

        driversStandingsPos.text = posDriver.ToString();
        driversStandingsName.text = nameDriver.ToString();
        driversStandingsTeam.text = teamDriver.ToString();
        driversStandingsPts.text = ptsDriver.ToString();

        teamsStandingsPos.text = posTeam.ToString();
        teamsStandingsTeam.text = team.ToString();
        teamsStandingsPts.text = ptsTeam.ToString();

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