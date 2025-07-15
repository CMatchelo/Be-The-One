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
	public MenuRaceManager menuRaceManager;

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
	public TMP_Text difficultyValueText;
	public TMP_Text skillTestText;
	public TMP_Text abilityTestText;
	public List<LocalizedString> localizedInRaceSkills;

	/* private FreePracticeEventList eventList; */
	
	private int totalSkillBonus = 0;
	private List<int> eventsDone = new List<int>();
	private int difficultyBase = 15;
	private int currentDifficulty = 15;
	private string decisionReferenceSkill = null;
	private int abilityId = -1;
	private int selectedEventIndex = -1;
	private float bestLapTime = 0f;
	private List<string> skillIDs = new List<string>();

	void Awake()
	{

	}

	void Start()
	{

	}

	public void InitializePractice()
	{
		PopulateDropdowns();
		
		startSessionBtn.onClick.RemoveAllListeners();
		rollDiceBtn.onClick.RemoveAllListeners();
		endSessionBtn.onClick.RemoveAllListeners();
		nextEventBtn.onClick.RemoveAllListeners();

		startSessionBtn.onClick.AddListener(StartSession);
		rollDiceBtn.onClick.AddListener(RollDice);
		endSessionBtn.onClick.AddListener(EndSession);
		nextEventBtn.onClick.AddListener(PickNextEvent);

		decision1Btn.interactable = false;
		decision2Btn.interactable = false;
		nextEventBtn.interactable = false;
		endSessionBtn.interactable = false;
		rollDiceBtn.interactable = false;
		startSessionBtn.interactable = true;
		tireDropdown.interactable = true;
		inRaceSkillsDropdown.interactable = true;

		rollResultText.text = "";
		skillTestText.text = "";
		abilityTestText.text = "";
		eventDescriptionText.text = "";

		difficultyBase = 15;
		currentDifficulty = difficultyBase;
		totalSkillBonus = 0;

		eventsDone.Clear();
	}

	void StartSession()
	{
		startSessionBtn.interactable = false;
		tireDropdown.interactable = false;
		inRaceSkillsDropdown.interactable = false;
		difficultyBase -= tireDropdown.value * 2;
		PickNextEvent();
	}

	void PickNextEvent()
	{
		rollResultText.text = "";
		skillTestText.text = "";
		abilityTestText.text = "";

		// Select event
		do
		{
			selectedEventIndex = RandomNumberGenerator.GetRandomBetween(0, 15);
		} while (eventsDone.Contains(selectedEventIndex));

		eventsDone.Add(selectedEventIndex);
		MenuRaceManager manager = FindFirstObjectByType<MenuRaceManager>();
		List<FreePracticeEvent> eventList = manager.GetFPEvents();
		FreePracticeEvent selectedEvent = eventList[selectedEventIndex];

		// Select event description
		int selectedEventDescription = RandomNumberGenerator.GetRandomBetween(0, selectedEvent.descriptions.Length - 1);
		string selectedDescription = selectedEvent.descriptions[selectedEventDescription];

		// Update UI Buttons

		int numbVar = RandomNumberGenerator.GetRandomBetween(-1, 1);
		currentDifficulty = difficultyBase + numbVar;
		difficultyValueText.text = "" + currentDifficulty;
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
		MenuRaceManager manager = FindFirstObjectByType<MenuRaceManager>();
		List<Ability> abilityList = manager.GetAbilities();
		Ability decisionAbility = null;
		abilityId = -1;
		string abilityToTest = "";
		if (decision.hasAbility)
		{
			decisionAbility = abilityList.FirstOrDefault(a => a.id == decision.ability);
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
		MenuRaceManager manager = FindFirstObjectByType<MenuRaceManager>();
		List<FreePracticeEvent> eventList = manager.GetFPEvents();
		FreePracticeEvent selectedEvent = eventList[selectedEventIndex];
		int resultEvent = RandomNumberGenerator.GetRandomBetween(0, selectedEvent.descriptions.Length - 1);
		int rollResult;
		string result = CalculateThrow.CalculateD20(currentDifficulty, decisionReferenceSkill, out rollResult, abilityId);

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

		rollResultText.text = "d20: " + rollResult;
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

		if (selectedIndex < 0 || selectedIndex >= skillIDs.Count)
		{
			Debug.LogWarning("Índice selecionado inválido.");
			return;
		}

		string selectedSkillID = skillIDs[selectedIndex]; // Ex: "acceleration", etc.

		if (SaveSession.CurrentGameData.profile.weekendBonus == null)
		{
			SaveSession.CurrentGameData.profile.weekendBonus = new WeekendBonus();
		}

		totalSkillBonus *= 3 - tireDropdown.value;

		switch (selectedSkillID)
		{
			case "highSpeedCorners":
				SaveSession.CurrentGameData.profile.weekendBonus.highSpeedCorners = totalSkillBonus;
				break;
			case "lowSpeedCorners":
				SaveSession.CurrentGameData.profile.weekendBonus.lowSpeedCorners = totalSkillBonus;
				break;
			case "acceleration":
				SaveSession.CurrentGameData.profile.weekendBonus.acceleration = totalSkillBonus;
				break;
			case "topSpeed":
				SaveSession.CurrentGameData.profile.weekendBonus.topSpeed = totalSkillBonus;
				break;
			default:
				Debug.LogWarning("Skill não reconhecida: " + selectedSkillID);
				break;
		}

		UpdateMenuTexts();
		SaveUtility.UpdateProfile();
		FPPanel.SetActive(false);
		RaceMenuPanel.SetActive(true);
	}

	void UpdateMenuTexts()
	{
		Driver playerDriver = SaveSession.CurrentGameData.profile.driver;
		MenuRaceManager manager = FindFirstObjectByType<MenuRaceManager>();
		Team playerTeam = manager.teamsList.teams.Find(t => t.id == playerDriver.teamId);
		Track selectedTrack = manager.selectedTrack;
		string tireSelected = tireDropdown.options[tireDropdown.value].text;

		float trackFactor = RaceSimulatorUtility.CalculateTrackFactor(selectedTrack.circuitLength);

		float carFactor = PerformanceCalculator.CalculateCarFactor(playerDriver, playerTeam, selectedTrack);
		CarSimulationState playerCar = new CarSimulationState(tireSelected, carFactor, playerDriver.firstName, playerTeam.teamName);

		bestLapTime = RaceSimulatorUtility.CalculateLapTime(playerCar, trackFactor, selectedTrack.circuitLength);
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

	async void PopulateDropdowns()
	{
		// Tires
		tireDropdown.ClearOptions();
		tireDropdown.AddOptions(new List<string>(TireDatabase.TireParameters.Keys));

		// In Race Skills
		inRaceSkillsDropdown.ClearOptions();
		skillIDs.Clear(); // Limpa a lista antes de repopular

		List<string> texts = new List<string>();

		foreach (var locStr in localizedInRaceSkills)
		{
			var tableEntry = LocalizationSettings.StringDatabase.GetTableEntry(locStr.TableReference, locStr.TableEntryReference);
			var key = tableEntry.Entry.SharedEntry.Key; // Ex: "highSpeedCorners"

			var field = SaveSession.CurrentGameData.profile.weekendBonus.GetType().GetField(key);
			if (field == null)
			{
				Debug.LogWarning($"Campo {key} não encontrado.");
				continue;
			}

			int value = (int)field.GetValue(SaveSession.CurrentGameData.profile.weekendBonus);
			if (value > 0)
			{
				continue;
			}
			string localizedString = await locStr.GetLocalizedStringAsync().Task;
			skillIDs.Add(key);
			texts.Add(localizedString);
		}

		inRaceSkillsDropdown.AddOptions(texts);
	}
}
