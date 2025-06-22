using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialoguePhrases
{
    public List<string> firstPhrase;
}

[System.Serializable]
public class DriverProfiles
{
    public DialoguePhrases Rising_star;
    public DialoguePhrases Experienced_driver;
    public DialoguePhrases Legendary_veteran;
    public DialoguePhrases Established_champion;
    
    // Unity não desserializa propriedades corretamente, então usamos campos públicos
    public DialoguePhrases GetProfile(string profileName)
    {
        switch (profileName)
        {
            case "Rising star": return Rising_star;
            case "Experienced driver": return Experienced_driver;
            case "Legendary veteran": return Legendary_veteran;
            case "Established champion": return Established_champion;
            default: return null;
        }
    }
}

[System.Serializable]
public class ContractNegotiationDialogue
{
    public DriverProfiles profiles;
}
public class ContractDialogueManager : MonoBehaviour
{  
    private ContractNegotiationDialogue dialogueData;

    private void Awake()
    {
        LoadDialogueData();
    }

    private void LoadDialogueData()
    {
        TextAsset dialoguesJson = Resources.Load<TextAsset>("contractNegotiationPhrases");
        if (dialoguesJson != null)
        {
            dialogueData = JsonUtility.FromJson<ContractNegotiationDialogue>(dialoguesJson.text);
            Debug.Log("Dados de diálogo carregados com sucesso!");
        }
        else
        {
            Debug.LogError("Arquivo JSON não atribuído no Inspector!");
        }
    }

    // Exemplo de como acessar as frases
    public string GetRandomFirstPhrase(string profileType)
    {
        if (dialogueData == null || dialogueData.profiles == null)
        {
            Debug.LogError("Dados de diálogo não carregados!");
            return "Default negotiation phrase.";
        }

        var profile = dialogueData.profiles.GetProfile(profileType);
        if (profile == null || profile.firstPhrase == null || profile.firstPhrase.Count == 0)
        {
            Debug.LogError($"Perfil ou frases não encontrados para: {profileType}");
            return "Default phrase for " + profileType;
        }

        int randomIndex = Random.Range(0, profile.firstPhrase.Count);
        return profile.firstPhrase[randomIndex];
    }
}