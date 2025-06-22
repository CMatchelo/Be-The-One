using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// Novo arquivo: SaveUtility.cs
public static class SaveUtility
{
    public static void CreateNewSave(
        PlayerProfile profile,
        Driver player,
        int selectedTeamId,
        TMP_Dropdown companionDropdown,
        DriversList activeDriversList,
        DriversList inactiveDriversList,
        TeamsList teamsList)
    {
        List<Driver> teamDrivers = activeDriversList.drivers.FindAll(d => d.teamId == selectedTeamId);
        Driver selectedDriver = teamDrivers[companionDropdown.value];
        Driver unselectedDriver = teamDrivers.First(d => d.id != selectedDriver.id);
        unselectedDriver.active = false;
        unselectedDriver.teamId = 11;
        inactiveDriversList.drivers.Add(unselectedDriver);
        activeDriversList.drivers.RemoveAll(d => d.id == unselectedDriver.id);

        activeDriversList.drivers.Add(player);

        GameData gameData = new GameData()
        {
            profile = profile,
            playerFirstName = player.firstName,
            playerLastName = player.lastName,
            teamId = selectedTeamId,
            companionId = selectedDriver.id
        };

        // Save code

        string randomString = RandomStringGenerator.GenerateRandomString();
        SaveSession.CurrentSaveId = $"{player.firstName}_{randomString}";
        string saveFolder = Path.Combine(Application.persistentDataPath, "saves", $"{player.firstName}_{randomString}");

        Directory.CreateDirectory(saveFolder);

        string savePath = Path.Combine(saveFolder, "savegame.json");
        File.WriteAllText(savePath, JsonUtility.ToJson(gameData, true));

        string activeDriversPath = Path.Combine(saveFolder, "activeDriversList.json");
        File.WriteAllText(activeDriversPath, JsonUtility.ToJson(activeDriversList, true));

        string inactiveDriversPath = Path.Combine(saveFolder, "inactiveDriversList.json");
        File.WriteAllText(inactiveDriversPath, JsonUtility.ToJson(inactiveDriversList, true));

        string teamsPath = Path.Combine(saveFolder, "teamsList.json");
        File.WriteAllText(teamsPath, JsonUtility.ToJson(teamsList, true));

        SceneManager.LoadScene("MenuScene");
    }
}
