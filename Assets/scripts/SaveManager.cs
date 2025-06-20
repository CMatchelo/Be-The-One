using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject step1Panel;
    public GameObject step2Panel;
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField ageInput;
    public TMP_Dropdown nationalityDropdown;
    public TMP_Dropdown teamDropdown;
    public TMP_Dropdown companionDropdown;
    public TMP_Dropdown categoryDropdown;
    public TMP_InputField[] skillInputs;
    public Button highSpeedCornerPlusBtn;
    public Button highSpeedCornerMinusBtn;
    public Button lowSpeedCornersPlusBtn;
    public Button lowSpeedCornersMinusBtn;
    public Button topSpeedPlusBtn;
    public Button topSpeedMinusBtn;
    public Button accelerationsPlusBtn;
    public Button accelerationsMinusBtn;
    public TMP_Text remainingPointsText;
    public Toggle[] toggleGroup;
    public Button confirmButton;
    public Button nextButton;

    [Header("Category Settings")]
    [SerializeField] private readonly int[] basePoints = { 60, 70, 80, 90 };

    private int remainingPoints = 10;
    private int[] currentValues = new int[4];
    private List<Team> teams = new List<Team>();
    private DriversList activeDriversList;
    private DriversList inactiveDriversList;
    private TeamsList teamsList;
    private NationalitiesList nationalitiesList;
    private int limitPoints = 0;
    private List<Toggle> selectedToggles = new List<Toggle>();

    private const int MAX_SELECTION = 3;
    private int selectedTeamId = -1;
    private int selectedTeammateId = -1;
    private Nationality playerNationality;
    private string playerCategory;

    void Start()
    {
        LoadDatabases();
        PopulateTeamDropdown();
        PopulateNationalityDropdown();

        teamDropdown.onValueChanged.AddListener(OnTeamSelected);
        companionDropdown.onValueChanged.AddListener(OnCompanionSelected);
        categoryDropdown.onValueChanged.AddListener(OnCategorySelected);
        nationalityDropdown.onValueChanged.AddListener(OnNationalitySelected);
        lowSpeedCornersPlusBtn.onClick.AddListener(() => OnSkillValueChanged(1, 1));
        lowSpeedCornersMinusBtn.onClick.AddListener(() => OnSkillValueChanged(1, -1));
        highSpeedCornerPlusBtn.onClick.AddListener(() => OnSkillValueChanged(0, 1));
        highSpeedCornerMinusBtn.onClick.AddListener(() => OnSkillValueChanged(0, -1));
        topSpeedPlusBtn.onClick.AddListener(() => OnSkillValueChanged(2, 1));
        topSpeedMinusBtn.onClick.AddListener(() => OnSkillValueChanged(2, -1));
        accelerationsPlusBtn.onClick.AddListener(() => OnSkillValueChanged(3, 1));
        accelerationsMinusBtn.onClick.AddListener(() => OnSkillValueChanged(3, -1));

        foreach (Toggle toggle in toggleGroup)
        {
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        }

        confirmButton.interactable = false;
        OnCategorySelected(0);

        teamDropdown.value = 0;
        OnTeamSelected(0);

        companionDropdown.value = 0;
        OnCompanionSelected(0);

        nationalityDropdown.value = 0;
        OnCompanionSelected(0);

        step1Panel.SetActive(true);
        step2Panel.SetActive(false);
    }

    void OnCategorySelected(int categoryIndex)
    {
        // Define valores base
        for (int i = 0; i < skillInputs.Length; i++)
        {
            skillInputs[i].text = basePoints[categoryIndex].ToString();
            currentValues[i] = basePoints[categoryIndex];
        }
        limitPoints = basePoints[categoryIndex];
        remainingPoints = 10;
        playerCategory = categoryDropdown.options[categoryDropdown.value].text;
        UpdateUI();
    }

    private void OnToggleValueChanged(Toggle changedToggle, bool isOn)
    {
        if (isOn)
        {
            // Se já atingiu o máximo e está tentando marcar mais um
            if (selectedToggles.Count >= MAX_SELECTION)
            {
                changedToggle.isOn = false; // Desmarca
                return;
            }
            selectedToggles.Add(changedToggle);
        }
        else
        {
            selectedToggles.Remove(changedToggle);
        }
    }

    void OnSkillValueChanged(int index, int delta)
    {
        if (index < 0 || index >= skillInputs.Length) return;

        int currentValue;
        bool success = int.TryParse(skillInputs[index].text, out currentValue);
        if (!success) currentValue = 0;

        currentValue += delta;
        if (currentValue < limitPoints | (delta > 0 && remainingPoints <= 0)) return;
        currentValues[index] = currentValue;
        skillInputs[index].text = currentValue.ToString();
        remainingPoints -= delta;
        UpdateUI();
    }

    void OnNationalitySelected(int index)
    {
        if (index < 0 || index >= teamsList.teams.Count) return;
        playerNationality = nationalitiesList.nationalities[index];
    }

    void OnTeamSelected(int index)
    {
        if (index < 0 || index >= teamsList.teams.Count) return;
        selectedTeamId = teamsList.teams[index].id;
        PopulateCompanionDropdown(selectedTeamId);
    }

    void OnCompanionSelected(int index)
    {
        selectedTeammateId = teamsList.teams[index].id;
    }

    public void NextPage()
    {
        step1Panel.SetActive(false);
        step2Panel.SetActive(true);
    }

    void UpdateUI()
    {
        remainingPointsText.text = $"Pontos Restantes: {remainingPoints}";
        confirmButton.interactable = (remainingPoints == 0);
    }

    void LoadDatabases()
    {
        TextAsset teamsJson = Resources.Load<TextAsset>("TeamsDatabase");
        teamsList = JsonUtility.FromJson<TeamsList>(teamsJson.text);

        TextAsset NationsJson = Resources.Load<TextAsset>("Nationalities");
        nationalitiesList = JsonUtility.FromJson<NationalitiesList>(NationsJson.text);

        TextAsset activeDriversJson = Resources.Load<TextAsset>("DriversDatabase");
        activeDriversList = JsonUtility.FromJson<DriversList>(activeDriversJson.text);

        TextAsset inactiveDriversJson = Resources.Load<TextAsset>("InactiveDriversDatabase");
        inactiveDriversList = JsonUtility.FromJson<DriversList>(inactiveDriversJson.text);
    }

    void PopulateNationalityDropdown()
    {
        List<string> countryNames = new List<string>();
        foreach (Nationality n in nationalitiesList.nationalities)
        {
            countryNames.Add(n.country);
        }

        nationalityDropdown.ClearOptions();
        nationalityDropdown.AddOptions(countryNames);
    }
    void PopulateTeamDropdown()
    {
        teamDropdown.ClearOptions();

        List<string> teamNames = new List<string>();
        foreach (Team team in teamsList.teams)
        {
            teamNames.Add(team.teamName);
        }
        teamDropdown.AddOptions(teamNames);
        selectedTeamId = teamsList.teams[0].id;
    }

    void PopulateCompanionDropdown(int teamId)
    {
        companionDropdown.ClearOptions();

        List<Driver> teamDrivers = activeDriversList.drivers.FindAll(d => d.teamId == teamId);
        List<string> options = new List<string>();

        foreach (Driver driver in teamDrivers)
        {
            options.Add(new string(driver.firstName + " " + driver.lastName));
        }

        companionDropdown.AddOptions(options);
    }

    public void CreateNewSave()
    {
        if (string.IsNullOrEmpty(firstNameInput.text) || string.IsNullOrEmpty(lastNameInput.text) || selectedTeamId == -1 || companionDropdown.value == -1)
        {
            Debug.LogError("Preencha todos os campos!");
            return;
        }

        Driver player = new Driver
        {
            firstName = firstNameInput.text,
            lastName = lastNameInput.text,
            realFirstName = firstNameInput.text,
            realLastName = lastNameInput.text,
            nationality = playerNationality,
            birthDate = "1992-12-10",
            highSpeedCorners = currentValues[0],
            lowSpeedCorners = currentValues[1],
            topSpeed = currentValues[2],
            acceleration = currentValues[3],
            id = 123321,
            teamId = selectedTeamId,
            active = true
        };

        PlayerProfile profile = new PlayerProfile
        {
            playerFirstName = firstNameInput.text,
            playerLastName = lastNameInput.text,
            age = int.Parse(ageInput.text),
            teamId = selectedTeamId,
            id = 123321,
            nationality = playerNationality,
            past = playerCategory,
            highSpeedCorners = currentValues[0],
            lowSpeedCorners = currentValues[1],
            topSpeed = currentValues[2],
            acceleration = currentValues[3],
        };


        SaveUtility.CreateNewSave(
            player,
            selectedTeamId,
            companionDropdown,
            activeDriversList,
            inactiveDriversList
        );


    }
}

/* public string playerFirstName;
    public string playerLastName;
    public int age;
    public Nationality nationality;
    public int teamId;
    public int companionId;
    public string past;
    public int lowSpeedCorners;
    public int highSpeedCorner;
    public int topSpeed;
    public int acceleration;
    public int technique;
    public int bravery;
    public int potential;
    public int charisma;
    public int focus;
    public int awareness;
    public List<Ability> abilities; */