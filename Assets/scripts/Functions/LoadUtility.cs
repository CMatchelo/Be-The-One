using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// Novo arquivo: SaveUtility.cs
public static class LoadUtility
{
    public static void LoadGame(string saveId)
    {
        string path = Path.Combine(Application.persistentDataPath, "saves", saveId, "savegame.json");
        if (!File.Exists(path))
        {
            Debug.LogError("Save file não encontrado: " + path);
            return;
        }
        string json = File.ReadAllText(path);
        GameData gameData = JsonUtility.FromJson<GameData>(json);
        // Define o save atual
        SaveSession.CurrentSaveId = saveId;
        SaveSession.CurrentGameData = gameData;
    }
}
