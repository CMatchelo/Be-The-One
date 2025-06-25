using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class NegotiationLimit
{
    public float maxSalary = float.MaxValue;
    public int maxYears = int.MaxValue;
    public int maxStatus = int.MinValue; // 0 = melhor status (primeiro piloto)
    public bool negotiating = true;
    public bool dealClosed = false;
}

public class ContractManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject ContractNegotiationPanel;
    public GameObject MenuPanel;
    public TMP_Dropdown teamDropdown;
    public TMP_Dropdown statusOfferedDropdown;
    public TMP_Text teamDialogueBoxText;
    public TMP_Text salaryOfferedText;
    public TMP_Text yearsOfferedText;
    public Button negotiateBtn;
    public Button signBtn;
    public Button minusSalaryBtn;
    public Button plusSalaryBtn;
    public Button minusYearsBtn;
    public Button plusYearsBtn;


    [Header("Settings")]
    private TeamsList teamsList;
    private List<Team> sortedTeams;
    private int selectedTeamIndex = 0;
    private int statusOffered;
    private int status;
    private float salaryOffered;
    private float salary;
    private int yearsOffered;
    private int years;
    private int difficulty = 11;
    List<int> listDifficulty = Enumerable.Repeat(11, 10).ToList();

    [Header("Dialogues")]
    private ContractNegotiationDialogue dialogueData;

    private Dictionary<int, NegotiationLimit> negotiationLimits = new Dictionary<int, NegotiationLimit>();


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
        minusYearsBtn.onClick.AddListener(() => ChangeYears(-1));
        plusYearsBtn.onClick.AddListener(() => ChangeYears(1));
        negotiateBtn.interactable = false;
    }

    void SignContract()
    {
        SaveSession.CurrentGameData.profile.driver.role = statusOfferedDropdown.value;
        SaveSession.CurrentGameData.profile.driver.teamId = sortedTeams[selectedTeamIndex].id;
        SaveSession.CurrentGameData.profile.driver.yearsOfContract = yearsOffered;
        SaveSession.CurrentGameData.profile.driver.active = true;
        SaveUtility.UpdateDrivers(SaveSession.CurrentGameData.profile.driver);
        ContractNegotiationPanel.SetActive(false);
        MenuPanel.SetActive(true);
        SaveUtility.UpdateProfile();
    }

    void NegotiateContract()
    {
        //Calculate Status difficulty
        int distance = Mathf.Abs(statusOfferedDropdown.value - statusOffered);
        difficulty = listDifficulty[selectedTeamIndex] + 4 * distance;
        
        //Calculate Salary difficulty
        if (salary == 0f) return;
        float maxImpact = 10f; // até 10 pontos de mudança na dificuldade
        float salaryDiff = salaryOffered - salary;
        float percentDiff = salaryDiff / salaryOffered;
        int diffChange = Mathf.RoundToInt(percentDiff * maxImpact);
        difficulty -= diffChange;

        //Calculate Years difficulty
        int yearsDif = Mathf.Abs(years - yearsOffered);
        difficulty += 2 * yearsDif;

        string result = CalculateThrow.CalculateD20(difficulty, "charisma", 12);
        ContractDialogueManager dialogueManager = FindFirstObjectByType<ContractDialogueManager>();
        result = "faill";
        
        listDifficulty[selectedTeamIndex] += 2;
        if (result == "critFail" | (listDifficulty[selectedTeamIndex] == 17 && result == "fail"))
        {
            teamDialogueBoxText.text = dialogueManager.GetRandomFailedPhrase("Rising star"); // Fix status
            if (!negotiationLimits.ContainsKey(selectedTeamIndex))
                negotiationLimits[selectedTeamIndex] = new NegotiationLimit();
            negotiationLimits[selectedTeamIndex].negotiating = false;
            ChangeBtns(false);
        }
        else if (result == "fail")
        {
            teamDialogueBoxText.text = dialogueManager.GetRandomNegativePhrase("Rising star"); // Fix status
            RegisterProposal(selectedTeamIndex, salary, years, statusOfferedDropdown.value);
        }
        else
        {
            teamDialogueBoxText.text = dialogueManager.GetRandomPositivePhrase("Rising star"); // Fix status
            if (!negotiationLimits.ContainsKey(selectedTeamIndex))
                negotiationLimits[selectedTeamIndex] = new NegotiationLimit();
            RegisterProposal(selectedTeamIndex, salary, years, statusOfferedDropdown.value, true);
            negotiationLimits[selectedTeamIndex].negotiating = false;
            negotiationLimits[selectedTeamIndex].dealClosed = true;
            ChangeBtns(false);
            signBtn.interactable = true;
        }
    }

    void ChangeYears(int qty)
    {
        minusYearsBtn.interactable = true;
        plusYearsBtn.interactable = true;
        years += qty;
        if (negotiationLimits.ContainsKey(selectedTeamIndex))
        {
            var limit = negotiationLimits[selectedTeamIndex];
            if (years > limit.maxYears)
            {
                Debug.Log("Esse número de anos já foi negado por esse time.");
                plusYearsBtn.interactable = false;
                return;
            }
        }
        if (years <= 1)
        {
            minusYearsBtn.interactable = false;
            years = 1;
        }
        if (years >= 5)
        {
            plusYearsBtn.interactable = false;
            years = 5;
        }
        yearsOfferedText.text = "Years offered: " + years;
        Debug.Log("1");
        VerifiesChanges();
    }

    void ChangeStatus()
    {
        if (negotiationLimits.ContainsKey(selectedTeamIndex))
        {
            var limit = negotiationLimits[selectedTeamIndex];
            if (statusOfferedDropdown.value < limit.maxStatus)
            {
                statusOfferedDropdown.value = statusOffered; // Volta pro anterior
                statusOfferedDropdown.RefreshShownValue();
                return;
            }
        }
        Debug.Log("2");
        VerifiesChanges();
    }

    void ChangeSalary(float percentChange)
    {
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
        if (negotiationLimits.ContainsKey(selectedTeamIndex))
        {
            var limit = negotiationLimits[selectedTeamIndex];
            if (newSalary > limit.maxSalary)
            {
                Debug.Log("Esse salário já foi negado por esse time.");
                plusSalaryBtn.interactable = false;
                return;
            }
        }
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
        Debug.Log("3");
        VerifiesChanges();
    }

    void VerifiesChanges()
    {
        if (salary <= salaryOffered && years <= yearsOffered && statusOfferedDropdown.value >= statusOffered)
        {
            negotiateBtn.interactable = false;
            signBtn.interactable = true;
        }
        else
        {
            negotiateBtn.interactable = true;
            signBtn.interactable = false;
        }
    }

    void OnTeamSelected(int teamIndex)
    {
        if (negotiationLimits.ContainsKey(teamIndex) && !negotiationLimits[teamIndex].negotiating && !negotiationLimits[teamIndex].dealClosed)
        {
            teamDialogueBoxText.text = "Essa equipe recusou negociar com você.";
            ChangeBtns(false);
            return; // Não continua com a negociação
        }
        if (negotiationLimits.ContainsKey(teamIndex) && negotiationLimits[teamIndex].dealClosed)
        {
            teamDialogueBoxText.text = "Voce ja tem um acordo com essa equipe";
            salary = negotiationLimits[teamIndex].maxSalary;
            salaryOfferedText.text = "Annual sallary: " + salary;
            status = negotiationLimits[teamIndex].maxStatus;
            statusOfferedDropdown.value = status; // Define o índice selecionado
            statusOfferedDropdown.RefreshShownValue();
            years = negotiationLimits[teamIndex].maxYears;
            yearsOfferedText.text = "Years offered: " + years;
            ChangeBtns(false);
            signBtn.interactable = true;
            return; // Não continua com a negociação
        }
        selectedTeamIndex = teamIndex;
        ContractDialogueManager dialogueManager = FindFirstObjectByType<ContractDialogueManager>();
        string phrase = dialogueManager.GetRandomFirstPhrase("Experienced driver"); // Fix status
        teamDialogueBoxText.text = phrase;
        CalculateOfferedStatus(teamIndex);
        ChangeBtns(true);
        negotiateBtn.interactable = false;
    }

    void CalculateOfferedStatus(int teamIndex)
    {
        teamIndex = Mathf.Clamp(teamIndex, 0, 9);

        // Interpolar limites baseado na posição do time (0 = melhor, 9 = pior)
        float firstMin = Mathf.Lerp(95, 80, teamIndex / 9f);
        float secondMin = Mathf.Lerp(85, 75, teamIndex / 9f);
        if (SaveSession.CurrentGameData.profile.driver.Average >= firstMin)
        {
            statusOfferedDropdown.value = 0; // Define o índice selecionado
            statusOfferedDropdown.RefreshShownValue();
        }
        else if (SaveSession.CurrentGameData.profile.driver.Average >= secondMin)
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
        CalculateYearsOffered();
    }

    void CalculateYearsOffered()
    {
        yearsOffered = (SaveSession.CurrentGameData.profile.lastResults - 1) / 25 + 1;
        years = yearsOffered;
        yearsOfferedText.text = "Years offered: " + years;
    }

    void PopulateTeamDropdown()
    {
        string path = Path.Combine(Application.persistentDataPath, "saves", "Cicero_g15866", "teamsList.json"); // Fix id load

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

    void RegisterProposal(int teamId, float requestedSalary, int requestedYears, int requestedStatus, bool closed = false)
    {
        if (!negotiationLimits.ContainsKey(teamId))
            negotiationLimits[teamId] = new NegotiationLimit();

        var limits = negotiationLimits[teamId];

        if (closed == true)
        {
            limits.maxSalary = requestedSalary;
            limits.maxYears = requestedYears;
            limits.maxStatus = requestedStatus;
            return;
        }

        if (requestedSalary < limits.maxSalary && requestedSalary > salaryOffered)
            limits.maxSalary = requestedSalary;
        if (requestedYears < limits.maxYears && requestedYears > yearsOffered)
            limits.maxYears = requestedYears;
        if (requestedStatus > limits.maxStatus && requestedStatus < statusOffered) // 0 é melhor
            limits.maxStatus = requestedStatus;
    }

    void ChangeBtns(bool newStatus)
    {
        minusSalaryBtn.interactable = newStatus;
        plusSalaryBtn.interactable = newStatus;
        minusYearsBtn.interactable = newStatus;
        plusYearsBtn.interactable = newStatus;
        statusOfferedDropdown.interactable = newStatus;
        negotiateBtn.interactable = newStatus;
        signBtn.interactable = newStatus;
    }
}
