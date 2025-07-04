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
    private List<int> eventsDone = new List<int>();
    private int difficulty = 12;
    private int tireSelected;
    private int skillSelected;
    private string eventReferenceSkill = null;
    private int abilityId = -1;
    private int selectedEventIndex = -1;


    void Start()
    {
        LoadDatabases();
        PopulateDropdowns();
        startSessionBtn.onClick.AddListener(StartFreePractice);
        rollDiceBtn.onClick.AddListener(RollDice);
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

    async void PickNextEvent()
    {
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

        // Get texts locations
        Ability eventAbility = null;
        eventReferenceSkill = selectedEvent.referenceSkill;
        string skillToTest = await SearchTextLocation.GetLocalizedStringAsync("Skills", eventReferenceSkill);
        string skillText = await SearchTextLocation.GetLocalizedStringAsync("Skills", "skillText");
        string abilityText = await SearchTextLocation.GetLocalizedStringAsync("Skills", "abilityText");

        // Get ability to test
        abilityId = -1;
        string abilityToTest = "";

        if (selectedEvent.hasAbility)
        {
            eventAbility = abilityList.abilities.FirstOrDefault(a => a.id == selectedEvent.ability);
            if (eventAbility != null)
            {
                abilityId = eventAbility.id;
                abilityToTest = await SearchTextLocation.GetLocalizedStringAsync("AbilityNames", eventAbility.name);
            }
        }

        // Atualiza UI

        if (!string.IsNullOrEmpty(abilityToTest))
        {
            Debug.Log($"{abilityText}: {abilityToTest} // ID: {abilityId}");
            abilityTestText.text = $"{abilityText}: {abilityToTest}";
        }

        nextEventBtn.interactable = false;
        rollDiceBtn.interactable = true;
        int skillValue = GetSkillValue(SaveSession.CurrentGameData.profile, eventReferenceSkill);
        skillTestText.text = $"{skillText}: {skillToTest} (+{skillValue})";
        eventDescriptionText.text = selectedDescription;
    }

    void RollDice()
    {
        // Get dice result
        FreePracticeEvent selectedEvent = eventList.events[selectedEventIndex];
        int resultEvent = RandomNumberGenerator.GetRandomBetween(0, selectedEvent.descriptions.Length - 1);
        string result = CalculateThrow.CalculateD20(difficulty, eventReferenceSkill, abilityId);

        // Update UI
        if (result == "suc" | result == "critSuc")
        {
            eventDescriptionText.text = selectedEvent.successText[resultEvent];
        }
        else
        {
            eventDescriptionText.text = selectedEvent.failureText[resultEvent];
        }

        rollDiceBtn.interactable = false;

        // If maximum events, end sessions
        if (eventsDone.Count >= 3)
        {
            endSessionBtn.interactable = true;
            return;
        }
        nextEventBtn.interactable = true;
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
