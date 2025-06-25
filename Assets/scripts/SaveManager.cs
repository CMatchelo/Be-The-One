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
    /* public TMP_Dropdown teamDropdown; */
    /* public TMP_Dropdown companionDropdown; */
    public TMP_Dropdown categoryDropdown;
    public TMP_InputField[] inRaceskillInputs;
    public TMP_InputField[] skillsInputs;
    public Button[] inRaceSkillsPlusButtons;
    public Button[] inRaceSkillsMinusButtons;
    public Button[] skillsPlusButtons;
    public Button[] skillsMinusButtons;
    public TMP_Text remainingPointsText;
    public TMP_Text remainingSkillsPointsText;
    public Toggle[] toggleGroup;
    public Button confirmButton;
    public Button nextButton;

    [Header("Category Settings")]
    [SerializeField] private readonly int[] basePoints = { 60, 70, 80, 90 };
    private int remainingPoints = 10;
    private int limitPoints = 0;
    private int remainingSkillsPoints = 10;
    private int[] currentInRaceValues = new int[4];
    private int[] currentOffRaceValues = new int[6];
    /* private List<Team> teams = new List<Team>(); */
    private DriversList activeDriversList;
    private DriversList inactiveDriversList;
    private AbilityList abilityList;
    private TeamsList teamsList;
    private NationalitiesList nationalitiesList;
    private List<Toggle> selectedToggles = new List<Toggle>();
    private const int MAX_SELECTION = 4;
    /* private int selectedTeamId = -1;
    private int selectedTeammateId = -1; */
    private Nationality playerNationality;
    private string playerCategory;

    void Start()
    {
        LoadDatabases();
        /* PopulateTeamDropdown(); */
        PopulateNationalityDropdown();
        PopulateAbilitiesToggle();

        /* teamDropdown.onValueChanged.AddListener(OnTeamSelected); */
        /* companionDropdown.onValueChanged.AddListener(OnCompanionSelected); */
        categoryDropdown.onValueChanged.AddListener(OnCategorySelected);
        nationalityDropdown.onValueChanged.AddListener(OnNationalitySelected);
        for (int i = 0; i < inRaceSkillsPlusButtons.Length; i++)
        {
            int index = i;
            inRaceSkillsPlusButtons[i].onClick.AddListener(() => OnInRaceSkillValueChanged(index, 1));
        }
        for (int i = 0; i < inRaceSkillsMinusButtons.Length; i++)
        {
            int index = i;
            inRaceSkillsMinusButtons[i].onClick.AddListener(() => OnInRaceSkillValueChanged(index, -1));
        }
        for (int i = 0; i < skillsPlusButtons.Length; i++)
        {
            int index = i;
            skillsPlusButtons[i].onClick.AddListener(() => OnSkillValueChanged(index, 1));
        }
        for (int i = 0; i < skillsMinusButtons.Length; i++)
        {
            int index = i;
            skillsMinusButtons[i].onClick.AddListener(() => OnSkillValueChanged(index, -1));
        }

        foreach (Toggle toggle in toggleGroup)
        {
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        }

        confirmButton.interactable = false;
        OnCategorySelected(0);

        /* teamDropdown.value = 0;
        OnTeamSelected(0); */

        /* companionDropdown.value = 0;
        OnCompanionSelected(0); */

        nationalityDropdown.value = 0;
        OnNationalitySelected(0);
    }

    void OnCategorySelected(int categoryIndex)
    {
        // Define valores base
        for (int i = 0; i < inRaceskillInputs.Length; i++)
        {
            inRaceskillInputs[i].text = basePoints[categoryIndex].ToString();
            currentInRaceValues[i] = basePoints[categoryIndex];
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
        Debug.Log(selectedToggles);
    }

    void OnInRaceSkillValueChanged(int index, int delta)
    {
        if (index < 0 || index >= inRaceskillInputs.Length) return;

        int currentValue;
        bool success = int.TryParse(inRaceskillInputs[index].text, out currentValue);
        if (!success) currentValue = 0;

        currentValue += delta;
        if (currentValue < limitPoints | (delta > 0 && remainingPoints <= 0)) return;
        currentInRaceValues[index] = currentValue;
        inRaceskillInputs[index].text = currentValue.ToString();
        remainingPoints -= delta;
        UpdateUI();
    }

    void OnSkillValueChanged(int index, int delta)
    {
        if (index < 0 || index >= skillsInputs.Length) return;

        int currentValue;
        bool success = int.TryParse(skillsInputs[index].text, out currentValue);
        if (!success) currentValue = 0;

        currentValue += delta;
        if (currentValue > 6 | currentValue < 0 | (delta > 0 && remainingSkillsPoints <= 0)) return;
        currentOffRaceValues[index] = currentValue;
        skillsInputs[index].text = currentValue.ToString();
        remainingSkillsPoints -= delta;
        UpdateUI();
    }

    void OnNationalitySelected(int index)
    {
        if (index < 0 || index >= nationalitiesList.nationalities.Count) return;
        playerNationality = nationalitiesList.nationalities[index];
    }

    /* void OnTeamSelected(int index)
    {
        if (index < 0 || index >= teamsList.teams.Count) return;
        selectedTeamId = teamsList.teams[index].id;
        PopulateCompanionDropdown(selectedTeamId);
    } */

    /* void OnCompanionSelected(int index)
    {
        selectedTeammateId = teamsList.teams[index].id;
    } */

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

        TextAsset abilityJson = Resources.Load<TextAsset>("abilities");
        abilityList = JsonUtility.FromJson<AbilityList>(abilityJson.text);
    }

    void PopulateAbilitiesToggle()
    {
        for (int i = 0; i < toggleGroup.Length && i < abilityList.abilities.Length; i++)
        {
            Toggle toggle = toggleGroup[i];
            string abilityName = abilityList.abilities[i].name;

            // Atribui ao texto filho do Toggle
            Text label = toggle.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = abilityName;
            }
        }
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
    /* void PopulateTeamDropdown()
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
    } */

    public void CreateNewSave()
    {
        if (string.IsNullOrEmpty(firstNameInput.text) || string.IsNullOrEmpty(lastNameInput.text)/*  || selectedTeamId == -1 || companionDropdown.value == -1 */)
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
            age = int.Parse(ageInput.text),
            highSpeedCorners = currentInRaceValues[0],
            lowSpeedCorners = currentInRaceValues[1],
            topSpeed = currentInRaceValues[2],
            acceleration = currentInRaceValues[3],
            id = 123321,
            /* teamId = selectedTeamId, */
            active = false
        };

        PlayerProfile profile = new PlayerProfile
        {
            driver = player,
            id = 123321,
            past = playerCategory,
            technique = int.Parse(skillsInputs[0].text),
            bravery = int.Parse(skillsInputs[1].text),
            potential = int.Parse(skillsInputs[2].text),
            charisma = int.Parse(skillsInputs[3].text),
            focus = int.Parse(skillsInputs[4].text),
            awareness = int.Parse(skillsInputs[5].text)
        };

        profile.abilities = new List<Ability>();
        for (int i = 0; i < toggleGroup.Length; i++)
        {
            Toggle toggle = toggleGroup[i];
            if (toggle.isOn)
            {
                profile.abilities.Add(abilityList.abilities[i]);
            }
        }
        SaveUtility.CreateNewSave(
            profile,
            player,
            /* selectedTeamId,
            companionDropdown, */
            activeDriversList,
            inactiveDriversList,
            teamsList
        );
    }
}

/* public string playerFirstName;
    public int technique;
    public int bravery;
    public int potential;
    public int charisma;
    public int focus;
    public int awareness;
    public List<Ability> abilities; */