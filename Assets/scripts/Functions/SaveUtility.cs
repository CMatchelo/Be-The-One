using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SaveUtility
{
    public static void CreateNewSave(
        PlayerProfile profile,
        Driver player,
        DriversList activeDriversList,
        DriversList inactiveDriversList,
        TeamsList teamsList)
    {
        activeDriversList.drivers.Add(player);

        string randomString = RandomStringGenerator.GenerateRandomString();

        GameData gameData = new GameData()
        {
            profile = profile,
            playerFirstName = player.firstName,
            playerLastName = player.lastName,
            saveId = $"{player.firstName}_{randomString}"
        };

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
        string saveFolder = Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId);
        string activePath = Path.Combine(saveFolder, "activeDriversList.json");
        string inactivePath = Path.Combine(saveFolder, "inactiveDriversList.json");

        if (!File.Exists(activePath))
        {
            Debug.LogError("Save file não encontrado: " + activePath);
            return;
        }

        // Carregar ativos
        var activeJson = File.ReadAllText(activePath);
        var activeList = JsonUtility.FromJson<DriversList>(activeJson);

        // Carregar (ou criar) inativos
        DriversList inactiveList;
        if (File.Exists(inactivePath))
            inactiveList = JsonUtility.FromJson<DriversList>(File.ReadAllText(inactivePath));
        else
            inactiveList = new DriversList { drivers = new List<Driver>() };

        var drivers = activeList.drivers;

        // Quem está no mesmo time
        var sameTeam = drivers.Where(d => d.teamId == updatedDriver.teamId).ToList();
        var role0 = sameTeam.FirstOrDefault(d => d.role == 0);
        var role1 = sameTeam.FirstOrDefault(d => d.role == 1);

        // Se o novo piloto for Titular (role 0)
        if (updatedDriver.role == 0 && role0 != null && role0.id != updatedDriver.id)
        {
            MoveToInactive(role0, activeList, inactiveList);
        }

        // Se o novo piloto for Segundo Piloto (role 1)
        if (updatedDriver.role == 1 && role1 != null && role1.id != updatedDriver.id)
        {
            MoveToInactive(role1, activeList, inactiveList);
        }

        // Garantir que ninguém do time tenha o mesmo role duplicado
        foreach (var d in sameTeam)
        {
            if (d.id != updatedDriver.id && d.role == updatedDriver.role)
            {
                MoveToInactive(d, activeList, inactiveList);
            }
        }

        // Atualizar o updatedDriver na lista de ativos
        int idx = activeList.drivers.FindIndex(d => d.id == updatedDriver.id);
        if (idx != -1)
            activeList.drivers[idx] = updatedDriver;
        else
            activeList.drivers.Add(updatedDriver);

        // Salvar tudo
        File.WriteAllText(activePath, JsonUtility.ToJson(activeList, true));
        File.WriteAllText(inactivePath, JsonUtility.ToJson(inactiveList, true));
    }

    // ------- Função Auxiliar --------
    private static void MoveToInactive(Driver driver, DriversList activeList, DriversList inactiveList)
    {
        driver.teamId = 0;
        driver.role = 3;
        driver.active = false;

        // Remove da lista de ativos
        activeList.drivers.RemoveAll(d => d.id == driver.id);

        // Adiciona na lista de inativos se ainda não existir
        if (!inactiveList.drivers.Any(d => d.id == driver.id))
            inactiveList.drivers.Add(driver);
    }

}
