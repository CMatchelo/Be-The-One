using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialoguePhrases
{
    public List<string> firstPhrase;
    public List<string> positivePhrase;
    public List<string> negativePhrase;
    public List<string> failedPhrase;
}

[System.Serializable]
public class DriverProfiles
{
    public DialoguePhrases Rising_star;
    public DialoguePhrases Experienced_driver;
    public DialoguePhrases Legendary_veteran;
    public DialoguePhrases Established_champion;
    
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

    public string GetRandomPositivePhrase(string profileType)
    {
        if (dialogueData == null || dialogueData.profiles == null)
        {
            Debug.LogError("Dados de diálogo não carregados!");
            return "Default negotiation phrase.";
        }

        var profile = dialogueData.profiles.GetProfile(profileType);
        if (profile == null || profile.positivePhrase == null || profile.positivePhrase.Count == 0)
        {
            Debug.LogError($"Perfil ou frases não encontrados para: {profileType}");
            return "Default phrase for " + profileType;
        }

        int randomIndex = Random.Range(0, profile.positivePhrase.Count);
        return profile.positivePhrase[randomIndex];
    }

    public string GetRandomNegativePhrase(string profileType)
    {
        if (dialogueData == null || dialogueData.profiles == null)
        {
            Debug.LogError("Dados de diálogo não carregados!");
            return "Default negotiation phrase.";
        }

        var profile = dialogueData.profiles.GetProfile(profileType);
        if (profile == null || profile.negativePhrase == null || profile.negativePhrase.Count == 0)
        {
            Debug.LogError($"Perfil ou frases não encontrados para: {profileType}");
            return "Default phrase for " + profileType;
        }

        int randomIndex = Random.Range(0, profile.negativePhrase.Count);
        return profile.negativePhrase[randomIndex];
    }
    
    public string GetRandomFailedPhrase(string profileType)
    {
        if (dialogueData == null || dialogueData.profiles == null)
        {
            Debug.LogError("Dados de diálogo não carregados!");
            return "Default negotiation phrase.";
        }

        var profile = dialogueData.profiles.GetProfile(profileType);
        if (profile == null || profile.failedPhrase == null || profile.failedPhrase.Count == 0)
        {
            Debug.LogError($"Perfil ou frases não encontrados para: {profileType}");
            return "Default phrase for " + profileType;
        }

        int randomIndex = Random.Range(0, profile.failedPhrase.Count);
        return profile.failedPhrase[randomIndex];
    }
}