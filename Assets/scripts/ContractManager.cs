using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class ContractManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown teamDropdown;
    public TMP_Text teamDialogueBoxText;
    public TMP_Text statusOffered;
    public TMP_Text salaryOffered;


    [Header("Settings")]
    private TeamsList teamsList;
    private List<Team> sortedTeams;
    private int selectedTeamIndex = 0;
    private string status;
    private int salary;

    [Header("Dialogues")]
    private ContractNegotiationDialogue dialogueData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateTeamDropdown();
        OnTeamSelected(0);
        teamDropdown.onValueChanged.AddListener(OnTeamSelected);
    }

    void OnTeamSelected(int teamIndex)
    {
        selectedTeamIndex = teamIndex;
        ContractDialogueManager dialogueManager = FindFirstObjectByType<ContractDialogueManager>();
        string phrase = dialogueManager.GetRandomFirstPhrase("Rising star");
        teamDialogueBoxText.text = phrase;
        CalculateOfferedStatus(teamIndex);
    }

    void CalculateOfferedStatus(int teamIndex)
    {
        teamIndex = Mathf.Clamp(teamIndex, 0, 9);

        // Interpolar limites baseado na posição do time (0 = melhor, 9 = pior)
        float firstMin = Mathf.Lerp(95, 80, teamIndex / 9f);
        float secondMin = Mathf.Lerp(88, 73, teamIndex / 9f);

        if (SaveSession.CurrentGameData.profile.Average >= firstMin)
        {
            status = "First driver";
            salary = 1000000;
        }
        else if (SaveSession.CurrentGameData.profile.Average >= secondMin)
        {
            status = "Second driver";
            salary = 500000;
        }
        else
        {
            status = "Test Driver";
            salary = 250000;
        }
        statusOffered.text = "Status offered: " + status;
        GetSalary(status, teamIndex);
    }

    void GetSalary(string offerType, int teamIndex)
    {
        teamIndex = Mathf.Clamp(teamIndex, 0, 9);
        float t = teamIndex / 9f;

        salary = offerType switch
        {
            "First driver" => Mathf.RoundToInt(Mathf.Lerp(50000000f, 10000000f, t)),// De 50M (melhor) a 10M (pior)
            "Second driver" => Mathf.RoundToInt(Mathf.Lerp(15000000f, 3000000f, t)),// De 15M (melhor) a 3M (pior)
            "Test driver" => Mathf.RoundToInt(Mathf.Lerp(1500000f, 250000f, t)),// De 1.5M (melhor) a 250k (pior)
            _ => 0,// erro, tipo desconhecido
        };
        salaryOffered.text = "Annual sallary: " + salary;
    }




    void PopulateTeamDropdown()
    {
        string path = Path.Combine(Application.persistentDataPath, "saves", "Cicero_UYoD6K", "teamsList.json"); // Fix id load

        if (!File.Exists(path))
        {
            Debug.LogError("Save file não encontrado: " + path);
            return;
        }

        string jsonContent = File.ReadAllText(path); // Lê o JSON do arquivo físico
        teamsList = JsonUtility.FromJson<TeamsList>(jsonContent); // Desserializa

        if (teamsList?.teams == null) // Verifica se teamsList e teams existem
        {
            Debug.LogError("Falha ao carregar ou lista de times vazia!");
            return;
        }

        sortedTeams = teamsList.teams.OrderByDescending(team => team.Average).ToList();

        teamDropdown.ClearOptions();
        List<string> teamNames = new List<string>();
        foreach (Team team in sortedTeams)
        {
            teamNames.Add(team.teamName);
        }
        teamDropdown.AddOptions(teamNames);
    }

    void Update()
    {

    }
}
