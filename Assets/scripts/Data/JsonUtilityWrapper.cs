using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityWrapper
{
    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }

    public static List<T> FromJsonList<T>(string json)
    {
        // Adiciona uma chave externa para o Unity entender como "lista"
        string newJson = "{\"Items\":" + json + "}";
        return JsonUtility.FromJson<Wrapper<T>>(newJson).Items;
    }
}
