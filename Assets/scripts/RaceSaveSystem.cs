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

    private static string ChampionshipPath => Path.Combine(Application.persistentDataPath, "championship.json");

    public static void UpdateChampionship(List<DriverResult> raceResults)
    {
        // Sistema de pontuação (exemplo: 25-18-15-12-10-8-6-4-2-1)
        int[] pointsSystem = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        var championship = LoadChampionship();

        for (int i = 0; i < Math.Min(raceResults.Count, pointsSystem.Length); i++)
        {
            var driver = raceResults[i].driver;
            championship.AddPoints(driver.id, driver.teamId, pointsSystem[i]);
        }

        SaveChampionship(championship);
    }

    public static ChampionshipStatus LoadChampionship()
    {
        if (!File.Exists(ChampionshipPath)) return new ChampionshipStatus();
        string json = File.ReadAllText(ChampionshipPath);
        return JsonUtility.FromJson<ChampionshipStatus>(json);
    }

    private static void SaveChampionship(ChampionshipStatus status)
    {
        string json = JsonUtility.ToJson(status, true);
        File.WriteAllText(ChampionshipPath, json);
    }
}
