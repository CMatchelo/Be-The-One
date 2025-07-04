using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class ChampionshipManager
{
    [System.Serializable]
    public class ChampionshipProgress
    {
        public List<string> completedRaces = new List<string>();
        public string nextRace;
    }

    private static string ProgressPath => Path.Combine(Application.persistentDataPath, "championship_progress.json");
    private static TracksList tracksData;

    public static void Initialize()
    {
        // Carrega os dados das pistas
        TextAsset jsonFile = Resources.Load<TextAsset>("TracksDatabase");
        tracksData = JsonUtility.FromJson<TracksList>(jsonFile.text);

        // Se não houver progresso salvo, define a primeira corrida
        if (!File.Exists(ProgressPath))
        {
            var progress = new ChampionshipProgress
            {
                nextRace = tracksData.tracks[0].circuitName
            };
            SaveProgress(progress);
        }
    }

    public static Track GetNextRace()
    {
        var progress = LoadProgress();
        Debug.Log("Proxima corrida: " + progress.nextRace);
        if (tracksData == null)
        {
            Debug.LogError("tracksData is null");
            return null;
        }
        if (tracksData.tracks == null)
        {
            Debug.LogError("tracksData.tracks is null");
            return null;
        }
        return tracksData.tracks.Find(t => t.circuitName == progress.nextRace);
    }

    public static void CompleteCurrentRace()
    {
        var progress = LoadProgress();
        progress.completedRaces.Add(progress.nextRace);

        // Define a próxima corrida
        int currentIndex = tracksData.tracks.FindIndex(t => t.circuitName == progress.nextRace);
        if (currentIndex < tracksData.tracks.Count - 1)
        {
            progress.nextRace = tracksData.tracks[currentIndex + 1].circuitName;
        }
        else
        {
            progress.nextRace = "FIM_DO_CAMPEONATO";
        }

        SaveProgress(progress);
    }

    private static ChampionshipProgress LoadProgress()
    {
        if (!File.Exists(ProgressPath)) return new ChampionshipProgress();
        string json = File.ReadAllText(ProgressPath);
        return JsonUtility.FromJson<ChampionshipProgress>(json);
    }

    private static void SaveProgress(ChampionshipProgress progress)
    {
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(ProgressPath, json);
    }
}