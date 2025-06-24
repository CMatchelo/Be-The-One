using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class ContractManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject ContractNegotiationPanel;
    public GameObject MenuPanel;
    public TMP_Dropdown teamDropdown;
    public TMP_Dropdown statusOfferedDropdown;
    public TMP_Text teamDialogueBoxText;
    public TMP_Text salaryOfferedText;
    public Button negotiateBtn;
    public Button signBtn;
    public Button minusSalaryBtn;
    public Button plusSalaryBtn;


    [Header("Settings")]
    private TeamsList teamsList;
    private List<Team> sortedTeams;
    private int selectedTeamIndex = 0;
    private int statusOffered;
    private int status;
    private float salaryOffered;
    private float salary;
    private int difficulty = 10;

    [Header("Dialogues")]
    private ContractNegotiationDialogue dialogueData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateTeamDropdown();
        OnTeamSelected(0);
        teamDropdown.onValueChanged.AddListener(OnTeamSelected);
        statusOfferedDropdown.onValueChanged.AddListener((int index) => ChangeStatus());
        negotiateBtn.onClick.AddListener(NegotiateContract);
        signBtn.onClick.AddListener(SignContract);
        minusSalaryBtn.onClick.AddListener(() => ChangeSalary(-0.05f));
        plusSalaryBtn.onClick.AddListener(() => ChangeSalary(0.05f));
        negotiateBtn.interactable = false;
    }

    void SignContract()
    {
        ContractNegotiationPanel.SetActive(false);
        MenuPanel.SetActive(true);
        Debug.Log(SaveSession.CurrentGameData.teamId);
        SaveSession.CurrentGameData.teamId = 111;
        Debug.Log(SaveSession.CurrentGameData.teamId);
        SaveUtility.UpdateProfile();
    }

    void NegotiateContract()
    {
        int distance = Mathf.Abs(statusOfferedDropdown.value - statusOffered);
        difficulty = 10 + 5 * distance;
        if (salary == 0f) return;
        float percentDiff = Mathf.Abs((salaryOffered - salary) / salary) * 100f;
        difficulty += Mathf.RoundToInt(percentDiff / 5f);
        Debug.Log("Dificuldade: " + difficulty);
        string result = CalculateThrow.CalculateD20(difficulty, "charisma", 12);
        Debug.Log(result);
        ContractDialogueManager dialogueManager = FindFirstObjectByType<ContractDialogueManager>();
        if (result == "fail")
        {
            teamDialogueBoxText.text = dialogueManager.GetRandomNegativePhrase("Rising star"); // Fix status
        }
        else if (result == "critFail")
        {
            teamDialogueBoxText.text = dialogueManager.GetRandomFailedPhrase("Rising star"); // Fix status
        }
        else
        {
            teamDialogueBoxText.text = dialogueManager.GetRandomPositivePhrase("Rising star"); // Fix status
            negotiateBtn.interactable = false;
            signBtn.interactable = true;
        }
    }

    void ChangeStatus()
    {
        Debug.Log(statusOfferedDropdown.value + "//" + statusOffered);
        if (statusOfferedDropdown.value != statusOffered)
        {
            negotiateBtn.interactable = true;
            signBtn.interactable = false;
        }
        else
        {
            negotiateBtn.interactable = false;
            signBtn.interactable = true;
        }
    }

    void ChangeSalary(float percentChange)
    {
        negotiateBtn.interactable = true;
        signBtn.interactable = false;
        plusSalaryBtn.interactable = true;
        minusSalaryBtn.interactable = true;
        float changeAmount = salaryOffered * percentChange;
        // Calcula o novo salário
        float newSalary = salary + changeAmount;
        // Define os limites superiores
        float upperLimit1 = salaryOffered + 5000000f;
        float upperLimit2 = salaryOffered * 1.5f;
        float bottonLimit = salaryOffered * 0.5f;

        // Verifica se excede algum dos limites
        if (percentChange > 0 && (newSalary >= upperLimit1 || newSalary >= upperLimit2))
        {
            plusSalaryBtn.interactable = false;
            return; // Não permite aumentar mais
        }
        if (percentChange < 0 && (newSalary <= bottonLimit || newSalary < 0))
        {
            minusSalaryBtn.interactable = false;
            return; // Não permite aumentar mais
        }
        salary = newSalary;
        salaryOfferedText.text = "Annual sallary: " + salary;
    }

    void OnTeamSelected(int teamIndex)
    {
        negotiateBtn.interactable = false;
        signBtn.interactable = true;
        selectedTeamIndex = teamIndex;
        ContractDialogueManager dialogueManager = FindFirstObjectByType<ContractDialogueManager>();
        string phrase = dialogueManager.GetRandomFirstPhrase("Rising star"); // Fix status
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
            statusOfferedDropdown.value = 0; // Define o índice selecionado
            statusOfferedDropdown.RefreshShownValue();
        }
        else if (SaveSession.CurrentGameData.profile.Average >= secondMin)
        {
            statusOfferedDropdown.value = 1; // Define o índice selecionado
            statusOfferedDropdown.RefreshShownValue();
        }
        else
        {
            statusOfferedDropdown.value = 2; // Define o índice selecionado
            statusOfferedDropdown.RefreshShownValue();
        }
        statusOffered = statusOfferedDropdown.value;
        status = statusOffered;
        CalculateOfferedSalary(status, teamIndex);
    }

    void CalculateOfferedSalary(int offerType, int teamIndex)
    {
        teamIndex = Mathf.Clamp(teamIndex, 0, 9);
        float t = teamIndex / 9f;

        salary = offerType switch
        {
            0 => Mathf.RoundToInt(Mathf.Lerp(30000000f, 5000000f, t)),// De 30M (melhor) a 5M (pior)
            1 => Mathf.RoundToInt(Mathf.Lerp(13000000f, 1000000f, t)),// De 13M (melhor) a 1M (pior)
            2 => Mathf.RoundToInt(Mathf.Lerp(1000000f, 150000f, t)),// De 1M (melhor) a 150k (pior)
            _ => 0,// erro, tipo desconhecido
        };
        salary *= (SaveSession.CurrentGameData.profile.lastResults / 100f);
        salaryOffered = Mathf.Round(salary / 10000f) * 10000f;
        salary = salaryOffered;
        salaryOfferedText.text = "Annual sallary: " + salary;
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
