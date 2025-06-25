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
        /* int selectedTeamId,
        TMP_Dropdown companionDropdown, */
        DriversList activeDriversList,
        DriversList inactiveDriversList,
        TeamsList teamsList)
    {
        /* List<Driver> teamDrivers = activeDriversList.drivers.FindAll(d => d.teamId == selectedTeamId);
        Driver selectedDriver = teamDrivers[companionDropdown.value];
        Driver unselectedDriver = teamDrivers.First(d => d.id != selectedDriver.id);
        unselectedDriver.active = false;
        unselectedDriver.teamId = 11;
        inactiveDriversList.drivers.Add(unselectedDriver);
        activeDriversList.drivers.RemoveAll(d => d.id == unselectedDriver.id);
        */
        activeDriversList.drivers.Add(player);

        GameData gameData = new GameData()
        {
            profile = profile,
            playerFirstName = player.firstName,
            playerLastName = player.lastName,
            /* teamId = selectedTeamId,
            companionId = selectedDriver.id */
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

    public static void UpdateProfile()
    {
        var gameData = SaveSession.CurrentGameData;
        string saveFolder = Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId);
        string savePath = Path.Combine(saveFolder, "savegame.json");
        File.WriteAllText(savePath, JsonUtility.ToJson(gameData, true));
    }

    public static void UpdateDrivers(Driver updatedDriver)
    {
        Debug.Log(
            updatedDriver.role
            + " // " +
            updatedDriver.teamId
            + " // " +
            updatedDriver.active
        );
        string path = Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId, "activeDriversList.json");
        if (!File.Exists(path))
        {
            Debug.LogError("Save file não encontrado: " + path);
            return;
        }
        string json = File.ReadAllText(path);
        DriversList driversList = JsonUtility.FromJson<DriversList>(json);

        var sameTeamDrivers = driversList.drivers.Where(d => d.teamId == updatedDriver.teamId).ToList();
        var currentDriver1 = sameTeamDrivers.FirstOrDefault(d => d.role == 0);
        var currentDriver2 = sameTeamDrivers.FirstOrDefault(d => d.role == 1);

        if (updatedDriver.role == 0)
        {
            if (currentDriver1 != null) currentDriver1.role = 1;

            if (currentDriver2 != null)
            {
                currentDriver2.teamId = 0;
                currentDriver2.role = 3;
                currentDriver2.active = false;
            }
        }
        else if (updatedDriver.role == 1)
        {
            if (currentDriver2 != null)
            {
                currentDriver2.teamId = 0;
                currentDriver2.role = 3;
                currentDriver2.active = false;
            }
        }

        var indexSec = driversList.drivers.FindIndex(d => d.id == currentDriver2.id);
        if (indexSec != -1)
        {
            driversList.drivers[indexSec] = currentDriver2;
        }
        var indexFirst = driversList.drivers.FindIndex(d => d.id == currentDriver1.id);
        if (indexFirst != -1)
        {
            driversList.drivers[indexFirst] = currentDriver1;
        }
        var indexUpdated = driversList.drivers.FindIndex(d => d.id == updatedDriver.id);
        if (indexUpdated != -1)
        {
            driversList.drivers[indexUpdated] = updatedDriver;
        }

        string saveFolder = Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId);
        string activeDriversPath = Path.Combine(saveFolder, "activeDriversList1.json");
        File.WriteAllText(activeDriversPath, JsonUtility.ToJson(driversList, true));
    }
}
