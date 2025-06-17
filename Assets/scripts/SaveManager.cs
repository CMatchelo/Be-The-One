using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_Dropdown teamDropdown;
    public TMP_Dropdown companionDropdown;

    private List<Team> teams = new List<Team>();
    private int selectedTeamId = -1;
    private int selectedTeammateId = -1;
    private DriversList driversList;
    private TeamsList teamsList;

    void Start()
    {
        LoadDatabases();
        PopulateTeamDropdown();

        teamDropdown.onValueChanged.AddListener(OnTeamSelected);
        companionDropdown.onValueChanged.AddListener(OnCompanionSelected);

        // Força seleção inicial do primeiro time e preenche companheiros
        teamDropdown.value = 0;
        OnTeamSelected(0); 

        // Força seleção inicial do primeiro companheiro
        companionDropdown.value = 0;
        OnCompanionSelected(0);
    }

    void LoadDatabases()
    {
        TextAsset teamsJson = Resources.Load<TextAsset>("TeamsDatabase");
        teamsList = JsonUtility.FromJson<TeamsList>(teamsJson.text);

        TextAsset driversJson = Resources.Load<TextAsset>("driversDatabase");
        driversList = JsonUtility.FromJson<DriversList>(driversJson.text);
    }

    void PopulateTeamDropdown()
    {
        teamDropdown.ClearOptions();

        List<string> teamNames = new List<string>();
        foreach (Team team in teamsList.teams)
        {
            teamNames.Add(team.teamName);
        }
        teamDropdown.AddOptions(teamNames);
        selectedTeamId = teamsList.teams[0].id;
    }
    void PopulateCompanionDropdown(int teamId)
    {
        companionDropdown.ClearOptions();

        List<Driver> teamDrivers = driversList.drivers.FindAll(d => d.teamId == teamId);
        List<string> options = new List<string>();

        foreach (Driver driver in teamDrivers)
        {
            options.Add(new string(driver.firstName + " " + driver.lastName));
        }

        companionDropdown.AddOptions(options);
    }
    void OnTeamSelected(int index)
    {
        if (index < 0 || index >= teamsList.teams.Count) return;
        selectedTeamId = teamsList.teams[index].id;
        PopulateCompanionDropdown(selectedTeamId);
    }

    void OnCompanionSelected(int index)
    {
        selectedTeammateId = teamsList.teams[index].id;
    }


    public void CreateNewSave()
    {
        if (string.IsNullOrEmpty(nameInput.text) || selectedTeamId == -1 || companionDropdown.value == -1)
        {
            Debug.LogError("Preencha todos os campos!");
            return;
        }

        List<Driver> teamDrivers = driversList.drivers.FindAll(d => d.teamId == selectedTeamId);
        Driver selectedDriver = teamDrivers[companionDropdown.value];
        Driver unselectedDriver = teamDrivers.First(d => d.id != selectedDriver.id);

        unselectedDriver.firstName = nameInput.text;

        GameData gameData = new GameData()
        {
            playerName = nameInput.text,
            teamId = selectedTeamId,
            companionId = selectedDriver.id
        };

        // Save code
        string randomString = RandomStringGenerator.GenerateRandomString();
        string saveFolder = Path.Combine(Application.persistentDataPath, "saves", $"{nameInput.text}_{randomString}");

        Directory.CreateDirectory(saveFolder);

        string savePath = Path.Combine(saveFolder, "savegame.json");
        File.WriteAllText(savePath, JsonUtility.ToJson(gameData, true));

        string modifiedDriversPath = Path.Combine(saveFolder, "newDriversList.json");
        File.WriteAllText(modifiedDriversPath, JsonUtility.ToJson(driversList, true));

        SceneManager.LoadScene("MenuScene");
    }
}