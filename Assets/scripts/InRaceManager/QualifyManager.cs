using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Linq;

public class QualifyManager : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject QualifyPanel;
    public GameObject RaceMenuPanel;
    public MenuRaceManager menuRaceManager;

    [Header("UI Btns and Dropdowns")]
    public Button startSessionBtn;
    public Button endSessionBtn;
    public Button NextSessionBtn;
    public Button decision1Btn;
    public Button decision2Btn;
    public Button rollDiceBtn;
    public Button nextEventBtn;
    public TMP_Dropdown tireDropdown;

    [Header("UI Texts")]
    public TMP_Text eventDescriptionText;
    public TMP_Text rollResultText;
    public TMP_Text difficultyValueText;
    public TMP_Text skillTestText;
    public TMP_Text abilityTestText;
    public TMP_Text qualifyOrder;

    private List<int> eventsDone = new List<int>();
    private int difficultyBase = 12;
    private int currentDifficulty = 12;
    private int tireSelected;
    private string decisionReferenceSkill = null;
    private int abilityId = -1;
    private int selectedEventIndex = -1;
    private float diffTime = 0f;
    private int qualifyStep = 0;

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
        NextSessionBtn.onClick.RemoveAllListeners();

        startSessionBtn.onClick.AddListener(StartSession);
        rollDiceBtn.onClick.AddListener(RollDice);
        endSessionBtn.onClick.AddListener(EndSession);
        nextEventBtn.onClick.AddListener(PickNextEvent);
        NextSessionBtn.onClick.AddListener(InitializePractice);

        decision1Btn.interactable = false;
        decision2Btn.interactable = false;
        nextEventBtn.interactable = false;
        endSessionBtn.interactable = false;
        rollDiceBtn.interactable = false;
        startSessionBtn.interactable = true;
        tireDropdown.interactable = true;

        rollResultText.text = "";
        skillTestText.text = "";
        abilityTestText.text = "";
        eventDescriptionText.text = "";

        difficultyBase = 12;
        currentDifficulty = difficultyBase;
        diffTime = 0;

        eventsDone.Clear();
    }

    void StartSession()
    {
        /* SimulateQualifying(); */
        startSessionBtn.interactable = false;
        tireDropdown.interactable = false;
        tireSelected = tireDropdown.value;
        PickNextEvent();
    }

    void PickNextEvent()
    {
        rollResultText.text = "";
        skillTestText.text = "";
        abilityTestText.text = "";
        // Select event

        MenuRaceManager manager = FindFirstObjectByType<MenuRaceManager>();
        List<FreePracticeEvent> eventList = manager.GetQualifyEvents();

        do
        {
            selectedEventIndex = RandomNumberGenerator.GetRandomBetween(0, eventList.Count() - 1);
        } while (eventsDone.Contains(selectedEventIndex));


        eventsDone.Add(selectedEventIndex);
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
        MenuRaceManager manager = FindFirstObjectByType<MenuRaceManager>();
        List<FreePracticeEvent> eventList = manager.GetQualifyEvents();
        // Get dice result
        FreePracticeEvent selectedEvent = eventList[selectedEventIndex];
        int resultEvent = RandomNumberGenerator.GetRandomBetween(0, selectedEvent.descriptions.Length - 1);
        int rollResult;
        string result = CalculateThrow.CalculateD20(currentDifficulty, decisionReferenceSkill, out rollResult, abilityId);

        // Update UI
        if (result == "suc" | result == "critSuc")
        {
            eventDescriptionText.text = selectedEvent.successText[resultEvent];
            diffTime += UnityEngine.Random.Range(-0.2f, -0.5f);
        }
        else
        {
            eventDescriptionText.text = selectedEvent.failureText[resultEvent];
            diffTime += UnityEngine.Random.Range(0.2f, 0.5f);
        }

        Debug.Log("Tempo de dirença parcial: " + diffTime);

        rollResultText.text = "d20: " + rollResult;
        rollDiceBtn.interactable = false;
        decision1Btn.interactable = false;
        decision2Btn.interactable = false;

        // If maximum events, end sessions
        if (eventsDone.Count >= 3)
        {
            Debug.Log("Tempo de dirença: " + diffTime);
            SimulateQualifying();
            endSessionBtn.interactable = true;
            return;
        }
        nextEventBtn.interactable = true;
    }

    void EndSession()
    {
        SaveUtility.UpdateProfile();
        QualifyPanel.SetActive(false);
        RaceMenuPanel.SetActive(true);
    }

    void SimulateQualifying()
    {
        MenuRaceManager manager = FindFirstObjectByType<MenuRaceManager>();

        // Apply practice bonus
        Driver playerDriver = SaveSession.CurrentGameData.profile.driver;
        WeekendBonus bonus = SaveSession.CurrentGameData.profile.weekendBonus;

        playerDriver.highSpeedCorners += bonus.highSpeedCorners;
        playerDriver.lowSpeedCorners += bonus.lowSpeedCorners;
        playerDriver.acceleration += bonus.acceleration;
        playerDriver.topSpeed += bonus.topSpeed;

        Debug.Log("Top Speed: " + playerDriver.topSpeed);

        // Calculate factors to simulate lap
        Team playerTeam = manager.teamsList.teams.Find(t => t.id == playerDriver.teamId);
        Track selectedTrack = manager.selectedTrack;
        string tireSelected = tireDropdown.options[tireDropdown.value].text;
        float trackFactor = RaceSimulatorUtility.CalculateTrackFactor(selectedTrack.circuitLength);

        List<Driver> loadedDrivers = manager.GetLoadedDrivers();
        loadedDrivers.RemoveAll(d => d.id == playerDriver.id);

        // Simulate player laptime
        float carFactor = PerformanceCalculator.CalculateCarFactor(playerDriver, playerTeam, selectedTrack);
        CarSimulationState playerCar = new CarSimulationState(tireSelected, carFactor, playerDriver.firstName, playerTeam.teamName);
        float playerLapTime = RaceSimulatorUtility.CalculateLapTime(playerCar, trackFactor, selectedTrack.circuitLength);
        manager.qualifyingTimes[playerDriver.id] = playerLapTime;
        
        List<(Driver driver, float lapTime)> results = new List<(Driver, float)>
        {
            (playerDriver, playerLapTime)
        };
        Debug.Log($"Jogador: {playerDriver.firstName} - Tempo: {playerLapTime:F3}s");
        // Simulate opponents laptime
        foreach (Driver opponent in loadedDrivers)
        {
            Team team = manager.teamsList.teams.Find(t => t.id == opponent.teamId);
            string tire = tireDropdown.options[UnityEngine.Random.Range(0, tireDropdown.options.Count)].text;

            float opponentCarFactor = PerformanceCalculator.CalculateCarFactor(opponent, team, selectedTrack);
            CarSimulationState opponentCar = new CarSimulationState(tire, opponentCarFactor, opponent.firstName, team.teamName);
            float opponentLapTime = RaceSimulatorUtility.CalculateLapTime(opponentCar, trackFactor, selectedTrack.circuitLength);
            manager.qualifyingTimes[opponent.id] = opponentLapTime;
            Debug.Log($"Oponente: {opponent.firstName} - Tempo: {opponentLapTime:F3}s");
            results.Add((opponent, opponentLapTime));
        }

        // Order from slowest to fastest
        results = results.OrderByDescending(r => r.lapTime).ToList();

        // Prepare text to display
        string phaseLabel = manager.qualifyingPhase == 0 ? "Q1 - Eliminados:\n"
                           : manager.qualifyingPhase == 1 ? "Q2 - Eliminados:\n"
                           : "Q3 - Resultados Finais:\n";

        string eliminatedText = "";

        // Add slowests to new list
        int countToTake = (manager.qualifyingPhase < 2) ? 5 : results.Count;
        bool playerEliminated = false;

        for (int i = 0; i < countToTake; i++)
        {
            var eliminatedDriver = results[i].driver;
            float eliminatedTime = results[i].lapTime;

            manager.qualifyingGrid.Add(eliminatedDriver);
            loadedDrivers.RemoveAll(d => d.id == eliminatedDriver.id);

            eliminatedText += $"{i + 1}º - {eliminatedDriver.firstName} - {eliminatedTime:F3}s\n";

            if (eliminatedDriver.id == SaveSession.CurrentGameData.profile.driver.id)
            {
                playerEliminated = true;
            }
        }

        manager.qualifyingPhase++;
        qualifyOrder.text = phaseLabel + eliminatedText;

        if (playerEliminated)
        {
            // PLayer eliminated
            SimulateRemainingQualifying(manager, loadedDrivers, trackFactor, qualifyOrder);
        }
        else if (manager.qualifyingPhase == 3)
        {
            // End Quali
            manager.qualifyingGrid.Reverse();

            string finalGridText = "GRID FINAL:\n";
            for (int i = 0; i < manager.qualifyingGrid.Count; i++)
            {
                Driver driver = manager.qualifyingGrid[i];
                float time = manager.qualifyingTimes.ContainsKey(driver.id) ? manager.qualifyingTimes[driver.id] : -1f;
                finalGridText += $"{i + 1}º - {manager.qualifyingGrid[i].firstName} - {time:F3}s \n";
            }

            qualifyOrder.text = finalGridText;

            endSessionBtn.interactable = true;
            NextSessionBtn.interactable = false;
        }
        else
        {
            // Player in next session
            NextSessionBtn.interactable = true;
            endSessionBtn.interactable = false;
        }

    }

    void SimulateRemainingQualifying(MenuRaceManager manager, List<Driver> loadedDrivers, float trackFactor, TMP_Text qualifyOrder)
    {
        List<(Driver driver, float lapTime)> results = new List<(Driver, float)>();

        while (manager.qualifyingPhase < 3)
        {
            results.Clear();

            foreach (Driver driver in loadedDrivers)
            {
                Team team = manager.teamsList.teams.Find(t => t.id == driver.teamId);
                string tire = tireDropdown.options[UnityEngine.Random.Range(0, tireDropdown.options.Count)].text;

                float opponentCarFactor = PerformanceCalculator.CalculateCarFactor(driver, team, manager.selectedTrack);
                CarSimulationState car = new CarSimulationState(tire, opponentCarFactor, driver.firstName, team.teamName);
                float lapTime = RaceSimulatorUtility.CalculateLapTime(car, trackFactor, manager.selectedTrack.circuitLength);
                manager.qualifyingTimes[driver.id] = lapTime;
                results.Add((driver, lapTime));
            }

            results = results.OrderByDescending(r => r.lapTime).ToList();
            int eliminations = (manager.qualifyingPhase < 2) ? 5 : results.Count;

            for (int i = 0; i < eliminations; i++)
            {
                Driver eliminatedDriver = results[i].driver;
                manager.qualifyingGrid.Add(eliminatedDriver);
                loadedDrivers.RemoveAll(d => d.id == eliminatedDriver.id);
            }

            manager.qualifyingPhase++;
        }

        // Inverter e exibir grid final
        manager.qualifyingGrid.Reverse();

        string finalGridText = "GRID FINAL:\n";
        for (int i = 0; i < manager.qualifyingGrid.Count; i++)
        {
            Driver driver = manager.qualifyingGrid[i];
            float time = manager.qualifyingTimes.ContainsKey(driver.id) ? manager.qualifyingTimes[driver.id] : -1f;
            finalGridText += $"{i + 1}º - {manager.qualifyingGrid[i].firstName} - {time:F3}s \n";
        }

        qualifyOrder.text = finalGridText;

        endSessionBtn.interactable = true;
        NextSessionBtn.interactable = false;
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

    void PopulateDropdowns()
    {
        // Tires
        tireDropdown.ClearOptions();
        tireDropdown.AddOptions(new List<string>(TireDatabase.TireParameters.Keys));
    }
}
