using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public static class RaceSaveSystem
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "races.json");

    [System.Serializable]
    public class RaceDataWrapper
    {
        public List<RaceResult> races = new();
    }

    public static void SaveRace(RaceResult newResult)
    {
        string temp = JsonUtility.ToJson(newResult, true);
        Debug.Log($"Resultado1: {temp}");
        Debug.Log("Salvando corrida...");

        // Carrega os dados existentes
        RaceDataWrapper wrapper = LoadAll();

        Debug.Log($"Dados carregados: {wrapper.races?.Count} corridas existentes");

        // Se a lista de corridas for nula, inicializa
        if (wrapper.races == null)
        {
            wrapper.races = new List<RaceResult>();
            Debug.Log("Lista de corridas inicializada (era nula)");
        }

        // Adiciona o novo resultado
        wrapper.races.Add(newResult);
        Debug.Log($"Nova corrida adicionada. Total agora: {wrapper.races.Count}");

        // Converte para JSON
        string json = JsonUtility.ToJson(wrapper, true);
        Debug.Log($"JSON gerado: {json}");

        // Salva no arquivo
        File.WriteAllText(FilePath, json);
        Debug.Log("Corrida salva com sucesso!");
    }

    public static RaceDataWrapper LoadAll()
    {
        if (!File.Exists(FilePath)) return new RaceDataWrapper();
        string json = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<RaceDataWrapper>(json);
    }

    private static string ChampionshipDriversPath => Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId, "championship_driversStandings.json");
    private static string ChampionshipTeamsPath => Path.Combine(Application.persistentDataPath, "saves", SaveSession.CurrentSaveId, "championship_teamsStandings.json");

    public static void UpdateChampionship(List<DriverResult> raceResults)
    {
        int[] pointsSystem = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        DriversChampionshipStatus driversChampionship = LoadDriversChampionship();
        TeamsChampionshipStatus teamsChampionship = LoadTeamsChampionship();

        for (int i = 0; i < Math.Min(raceResults.Count, pointsSystem.Length); i++)
        {
            var driver = raceResults[i].driver;
            int points = pointsSystem[i];

            // Verifica se já existe o piloto na lista
            var existingDriver = driversChampionship.driverStandings.Find(s => s.driverId == driver.id);
            if (existingDriver != null)
            {
                existingDriver.points += points;
            }
            else
            {
                driversChampionship.driverStandings.Add(new DriverStanding(driver.id, driver.teamId, points));
            }
            var existingTeam = teamsChampionship.teamStandings.Find(t => t.teamId == driver.teamId);
            Debug.Log("1");
            if (existingTeam != null)
            {
                Debug.Log("2");
                existingTeam.points += points;
            }
            else
            {
                Debug.Log("3");
                teamsChampionship.teamStandings.Add(new TeamStanding(driver.teamId, points));
            }
            Debug.Log("4");
        }
        for (int i = 0; i < teamsChampionship.teamStandings.Count; i++)
        {
            Debug.Log(teamsChampionship.teamStandings[i].teamId + " // " + teamsChampionship.teamStandings[i].points);
        }
        SaveChampionship(driversChampionship, teamsChampionship);
    }

    public static TeamsChampionshipStatus LoadTeamsChampionship()
    {
        if (!File.Exists(ChampionshipTeamsPath)) return new TeamsChampionshipStatus();
        string json = File.ReadAllText(ChampionshipTeamsPath);
        return JsonUtility.FromJson<TeamsChampionshipStatus>(json);
    }

    public static DriversChampionshipStatus LoadDriversChampionship()
    {
        if (!File.Exists(ChampionshipDriversPath)) return new DriversChampionshipStatus();
        string json = File.ReadAllText(ChampionshipDriversPath);
        return JsonUtility.FromJson<DriversChampionshipStatus>(json);
    }

    private static void SaveChampionship(DriversChampionshipStatus driversChampionship, TeamsChampionshipStatus teamsChampionship)
    {
        string jsonDrivers = JsonUtility.ToJson(driversChampionship, true);
        File.WriteAllText(ChampionshipDriversPath, jsonDrivers);

        string jsonTeams = JsonUtility.ToJson(teamsChampionship, true);
        File.WriteAllText(ChampionshipTeamsPath, jsonTeams);
    }
}
