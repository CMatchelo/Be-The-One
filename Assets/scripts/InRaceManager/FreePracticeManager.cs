using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Linq;

public class FreePracticeManager : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject FPPanel;
    public GameObject RaceMenuPanel;

    [Header("UI Btns and Dropdowns")]
    public Button startSessionBtn;
    public Button endSessionBtn;
    public Button decision1Btn;
    public Button decision2Btn;
    public Button rollDiceBtn;
    public Button nextEventBtn;
    public TMP_Dropdown tireDropdown;
    public TMP_Dropdown inRaceSkillsDropdown;

    [Header("UI Texts")]
    public TMP_Text eventDescriptionText;
    public TMP_Text rollResultText;
    public TMP_Text skillTestText;
    public TMP_Text abilityTestText;
    public List<LocalizedString> localizedInRaceSkills;

    private FreePracticeEventList eventList;
    private AbilityList abilityList;
    private int totalSkillBonus = 0;
    private List<int> eventsDone = new List<int>();
    private int difficulty = 12;
    private int tireSelected;
    private int skillSelected;
    private string decisionReferenceSkill = null;
    private int abilityId = -1;
    private int selectedEventIndex = -1;


    void Awake()
    {
        FPPanel.SetActive(true);
        RaceMenuPanel.SetActive(false);
    }

    void Start()
    {
        LoadDatabases();
        PopulateDropdowns();
        startSessionBtn.onClick.AddListener(StartFreePractice);
        rollDiceBtn.onClick.AddListener(RollDice);
        endSessionBtn.onClick.AddListener(EndSession);
        decision1Btn.interactable = false;
        decision2Btn.interactable = false;
        nextEventBtn.interactable = false;
        endSessionBtn.interactable = false;
        rollDiceBtn.interactable = false;
        nextEventBtn.onClick.AddListener(PickNextEvent);
    }

    void StartFreePractice()
    {
        startSessionBtn.interactable = false;
        tireDropdown.interactable = false;
        inRaceSkillsDropdown.interactable = false;
        if (tireDropdown.value == 1) difficulty = 14;
        if (tireDropdown.value == 2) difficulty = 16;
        tireSelected = tireDropdown.value;
        skillSelected = inRaceSkillsDropdown.value;
        PickNextEvent();
    }

    void PickNextEvent()
    {
        skillTestText.text = "";
        abilityTestText.text = "";

        // Select event
        do
        {
            selectedEventIndex = RandomNumberGenerator.GetRandomBetween(0, 15);
        } while (eventsDone.Contains(selectedEventIndex));

        eventsDone.Add(selectedEventIndex);
        FreePracticeEvent selectedEvent = eventList.events[selectedEventIndex];

        // Select event description
        int selectedEventDescription = RandomNumberGenerator.GetRandomBetween(0, selectedEvent.descriptions.Length - 1);
        string selectedDescription = selectedEvent.descriptions[selectedEventDescription];

        // Update UI Buttons

        eventDescriptionText.text = selectedDescription;
        TMP_Text decision1Text = decision1Btn.GetComponentInChildren<TMP_Text>();
        TMP_Text decision2Text = decision2Btn.GetComponentInChildren<TMP_Text>();
        decision1Text.text = selectedEvent.decisions[0].decision;
        decision2Text.text = selectedEvent.decisions[1].decision;
        decision1Btn.onClick.AddListener(() => SelectDecision(selectedEvent.decisions[0]));
        decision2Btn.onClick.AddListener(() => SelectDecision(selectedEvent.decisions[1]));
        decision1Btn.interactable = true;
        decision2Btn.interactable = true;
        nextEventBtn.interactable = false;
    }

    async void SelectDecision(Decision decision)
    {
        // Get ability if exists
        Ability decisionAbility = null;
        abilityId = -1;
        string abilityToTest = "";
        if (decision.hasAbility)
        {
            decisionAbility = abilityList.abilities.FirstOrDefault(a => a.id == decision.ability);
            if (decisionAbility != null)
            {
                abilityId = decisionAbility.id;
                abilityToTest = await SearchTextLocation.GetLocalizedStringAsync("AbilityNames", decisionAbility.name);
            }
        }

        // Get texts locations

        decisionReferenceSkill = decision.referenceSkill;
        string skillToTest = await SearchTextLocation.GetLocalizedStringAsync("Skills", decisionReferenceSkill);
        string skillText = await SearchTextLocation.GetLocalizedStringAsync("Skills", "skillText");
        string abilityText = await SearchTextLocation.GetLocalizedStringAsync("Skills", "abilityText");

        if (!string.IsNullOrEmpty(abilityToTest))
        {
            abilityTestText.text = $"{abilityText}: {abilityToTest}";
        }
        else
        {
            abilityTestText.text = $"";
        }

        // Update UI
        rollDiceBtn.interactable = true;
        int skillValue = GetSkillValue(SaveSession.CurrentGameData.profile, decisionReferenceSkill);
        skillTestText.text = $"{skillText}: {skillToTest} (+{skillValue})";

    }

    void RollDice()
    {
        // Get dice result
        FreePracticeEvent selectedEvent = eventList.events[selectedEventIndex];
        int resultEvent = RandomNumberGenerator.GetRandomBetween(0, selectedEvent.descriptions.Length - 1);
        string result = CalculateThrow.CalculateD20(difficulty, decisionReferenceSkill, abilityId);

        // Update UI
        if (result == "suc" | result == "critSuc")
        {
            totalSkillBonus += 1;
            eventDescriptionText.text = selectedEvent.successText[resultEvent];
        }
        else
        {
            eventDescriptionText.text = selectedEvent.failureText[resultEvent];
        }

        // Update UI

        rollDiceBtn.interactable = false;
        decision1Btn.interactable = false;
        decision2Btn.interactable = false;

        // If maximum events, end sessions
        if (eventsDone.Count >= 3)
        {
            endSessionBtn.interactable = true;
            return;
        }
        nextEventBtn.interactable = true;
    }

    void EndSession()
    {
        int selectedIndex = inRaceSkillsDropdown.value;
        if (SaveSession.CurrentGameData.profile.weekendBonus == null)
        {
            SaveSession.CurrentGameData.profile.weekendBonus = new WeekendBonus();
        }
        switch (selectedIndex)
        {
            case 0:
                SaveSession.CurrentGameData.profile.weekendBonus.highSpeedCorners = totalSkillBonus;
                break;
            case 1:
                SaveSession.CurrentGameData.profile.weekendBonus.lowSpeedCorners = totalSkillBonus;
                break;
            case 2:
                SaveSession.CurrentGameData.profile.weekendBonus.acceleration = totalSkillBonus;
                break;
            case 3:
                SaveSession.CurrentGameData.profile.weekendBonus.topSpeed = totalSkillBonus;
                break;
            default:
                Debug.LogWarning("Índice inválido selecionado no dropdown.");
                break;
        }
        Debug.Log("Nova habilidade: " + SaveSession.CurrentGameData.profile.weekendBonus.topSpeed);
        SaveUtility.UpdateProfile();
        /* FPPanel.SetActive(true);
        RaceMenuPanel.SetActive(false); */
    }
    private int GetSkillValue(PlayerProfile profile, string skillKey)
    {
        switch (skillKey)
        {
            case "technique": return profile.technique;
            case "bravery": return profile.bravery;
            case "potential": return profile.potential;
            case "charisma": return profile.charisma;
            case "focus": return profile.focus;
            case "awareness": return profile.awareness;
            default:
                Debug.LogError($"Skill inválida: {skillKey}");
                return 0;
        }
    }

    void LoadDatabases()
    {
        TextAsset eventsJson = Resources.Load<TextAsset>("FreePracticeEvents");
        eventList = JsonUtility.FromJson<FreePracticeEventList>(eventsJson.text);

        TextAsset abilityJson = Resources.Load<TextAsset>("abilities");
        abilityList = JsonUtility.FromJson<AbilityList>(abilityJson.text);
    }

    void PopulateDropdowns()
    {
        // Tires
        tireDropdown.ClearOptions();
        tireDropdown.AddOptions(new List<string>(TireDatabase.TireParameters.Keys));

        // In Race Skills
        inRaceSkillsDropdown.ClearOptions();
        List<string> texts = new List<string>();
        int remaining = localizedInRaceSkills.Count;

        foreach (var locStr in localizedInRaceSkills)
        {
            var handle = locStr.GetLocalizedStringAsync();
            handle.Completed += op =>
            {
                texts.Add(op.Result);
                remaining--;

                if (remaining == 0)
                {
                    inRaceSkillsDropdown.AddOptions(texts);
                }
            };
        }
    }
}
