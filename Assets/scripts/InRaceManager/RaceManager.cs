using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject RacePanel;
    public GameObject RaceMenuPanel;
    public GameObject RaceDecisionPanel;
    public MenuRaceManager menuRaceManager;

    [Header("UI Buttons")]
    public Button startRaceButton;
    public Button continueRaceBtn;
    public Button decision1Btn;
    public Button decision2Btn;
    public Button rollDiceBtn;


    [Header("UI Texts")]
    public TMP_Text currentLapText;
    public TMP_Text previousLapText;
    public TMP_Text lapEventsText;
    public TMP_Text eventDescriptionText;
    public TMP_Text rollResultText;
    public TMP_Text difficultyValueText;
    public TMP_Text skillTestText;
    public TMP_Text abilityTestText;

    private List<DriverResult> raceState = new();
    private List<string> logMessages = new();
    private List<string> lapEventMessages = new();
    private List<int> eventsDone = new List<int>();
    private bool waitingForPlayer = false;
    private int selectedEventIndex = -1;
    private FreePracticeEvent selectedEvent;
    private int currentDifficulty = 10;
    private int difficultyBase = 10;
    private string decisionReferenceSkill = null;
    private int abilityId = -1;
    private int rollResult = 0;

    private void Start()
    {

    }

    public void InitializeRace()
    {
        menuRaceManager = FindFirstObjectByType<MenuRaceManager>();
        startRaceButton.onClick.AddListener(StartRace);
        rollDiceBtn.onClick.AddListener(RollDice);
        continueRaceBtn.onClick.AddListener(ContinueRace);
        decision1Btn.onClick.AddListener(() => SelectDecision(selectedEvent.decisions[0]));
        decision2Btn.onClick.AddListener(() => SelectDecision(selectedEvent.decisions[1]));
    }

    private void StartRace()
    {
        StartCoroutine(SimulateRace());
    }

    private IEnumerator SimulateRace()
    {
        List<Driver> grid = menuRaceManager.qualifyingGrid;
        Track track = menuRaceManager.selectedTrack;
        float trackFactor = RaceSimulatorUtility.CalculateTrackFactor(track.circuitLength);

        // Inicializar o estado da corrida
        raceState.Clear();
        foreach (var driver in grid)
        {
            int playerId = SaveSession.CurrentGameData.profile.driver.id;
            if (driver.id == playerId)
            {
                Debug.Log(driver.highSpeedCorners);
            }
            float carFactor = PerformanceCalculator.CalculateCarFactor(driver,
                menuRaceManager.teamsList.teams.Find(t => t.id == driver.teamId),
                track);

            var car = new CarSimulationState("Soft", carFactor, driver.firstName, driver.teamId.ToString());
            raceState.Add((driver, 0f, 0f)); // total time, last lap
        }

        // Loop das voltas
        for (int lap = 1; lap <= track.totalLaps; lap++)
        {
            previousLapText.text = "";
            previousLapText.text = string.Join("\n", logMessages);
            logMessages.Clear();
            lapEventMessages.Clear();
            List<(Driver driver, float totalTime, float lastLap)> updatedState = new();

            for (int idx = 0; idx < raceState.Count; idx++)
            {
                var entry = raceState[idx];
                var driver = entry.driver;

                float carFactor = PerformanceCalculator.CalculateCarFactor(driver,
                    menuRaceManager.teamsList.teams.Find(t => t.id == driver.teamId),
                    track);

                var car = new CarSimulationState("Soft", carFactor, driver.firstName, driver.teamId.ToString());
                float lapTime = RaceSimulatorUtility.CalculateLapTime(car, trackFactor, track.circuitLength);

                float baseTime = entry.totalTime;

                yield return StartCoroutine(TryOvertake(driver, baseTime, lapTime, updatedState, track, trackFactor, result =>
                {
                    updatedState.Insert(result.insertIndex, (result.driver, result.totalTime, result.lastLap));
                }));
            }

            raceState = updatedState.OrderBy(t => t.totalTime).ToList();
            currentLapText.text = "";

            AddLogMessage($"--- Volta {lap}/{track.totalLaps} ---");
            for (int i = 0; i < raceState.Count; i++)
            {
                var entry = raceState[i];
                AddLogMessage($"{i + 1}º - {entry.driver.firstName} - {entry.lastLap:F3}s - Total: {entry.totalTime:F3}s");
            }
            yield return new WaitForSeconds(0.1f);
        }

        AddLogMessage("--- Corrida Finalizada! ---");
        AddLogMessage("Classificação Final:");
        for (int i = 0; i < raceState.Count; i++)
        {
            AddLogMessage($"{i + 1}º - {raceState[i].driver.firstName} - Total: {raceState[i].totalTime:F3}s");
        }
        ChampionshipManager.CompleteCurrentRace();
        RaceSaveSystem.UpdateChampionship(raceState);
    }

    private IEnumerator TryOvertake(
    Driver driver,
    float baseTime,
    float lapTime,
    List<(Driver driver, float totalTime, float lastLap)> updatedState,
    Track track,
    float trackFactor,
    Action<(Driver driver, float totalTime, float lastLap, int insertIndex)> onFinished)
    {
        float newTotal = baseTime + lapTime;
        int insertIndex = updatedState.Count;

        for (int i = updatedState.Count - 1; i >= 0; i--)
        {
            var frontDriver = updatedState[i];
            if (frontDriver.totalTime - newTotal >= 1.5f)
            {
                AddEventMessage($"{driver.firstName} ultrapassou facilmente {frontDriver.driver.firstName}");
                insertIndex = i;
                continue;
            }
            if (newTotal < frontDriver.totalTime)
            {
                int roll = 0;
                //int playerId = SaveSession.CurrentGameData.profile.driver.id;
                int playerId = 1111;

                if (driver.id == playerId || frontDriver.driver.id == playerId)
                {
                    AddEventMessage($"Jogador envolvido em ultrapassagem: {driver.firstName} → {frontDriver.driver.firstName}");
                    waitingForPlayer = true;
                    RaceDecisionPanel.SetActive(true);
                    RacePanel.SetActive(false);

                    if (driver.id == playerId) PickNextEvent("overtaking");
                    else PickNextEvent("overtaken");

                    yield return new WaitUntil(() => waitingForPlayer == false);

                    RaceDecisionPanel.SetActive(false);
                    RacePanel.SetActive(true);

                    if (driver.id == playerId) roll = rollResult;
                    else roll = roll = 20 - rollResult;
                }
                else
                {
                    roll = RandomNumberGenerator.GetRandomBetween(1, 20);
                }

                if (roll == 1)
                {
                    float penalty = UnityEngine.Random.Range(9f, 11f);
                    newTotal = frontDriver.totalTime + penalty;
                    lapTime = newTotal - baseTime;
                    AddEventMessage($"{driver.firstName} cometeu um erro grave! Perdeu {penalty:F1}s.");
                    break;
                }

                if (roll < track.difficulty)
                {
                    float penalty = UnityEngine.Random.Range(0.1f, 0.5f);

                    float attemptedTime = frontDriver.totalTime + penalty;

                    if (i < updatedState.Count - 1)
                    {
                        var behindDriver = updatedState[i + 1];
                        attemptedTime = Mathf.Min(attemptedTime, behindDriver.totalTime - 0.001f);
                    }

                    newTotal = attemptedTime;
                    lapTime = newTotal - baseTime;

                    AddEventMessage(
                        $"{driver.firstName} ficou preso atrás de {frontDriver.driver.firstName} (Roll: {roll} ≤ {track.difficulty})"
                    );
                    break;
                }
                else
                {
                    AddEventMessage($"{driver.firstName} ultrapassou {frontDriver.driver.firstName} (Roll: {roll} > {track.difficulty})");
                    insertIndex = i;
                    continue;
                }
            }
            else
            {
                // Adicionar aqui informaçoes sobre a posiçao mantidas
                break;
            }
        }

        onFinished((driver, newTotal, lapTime, insertIndex));
    }

    void PickNextEvent(string eventType)
    {
        rollResultText.text = "";
        skillTestText.text = "";
        abilityTestText.text = "";
        List<FreePracticeEvent> eventList;
        // Select event
        if (eventType == "overtaking") eventList = menuRaceManager.GetOvertakingEvents();
        else eventList = menuRaceManager.GetOvertakenEvents();

        do
        {
            selectedEventIndex = RandomNumberGenerator.GetRandomBetween(0, eventList.Count() - 1);
        } while (eventsDone.Contains(selectedEventIndex));


        eventsDone.Add(selectedEventIndex);
        selectedEvent = eventList[selectedEventIndex];

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

        decision1Btn.interactable = true;
        decision2Btn.interactable = true;
        continueRaceBtn.interactable = false;
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
        string result = CalculateThrow.CalculateD20(currentDifficulty, decisionReferenceSkill, out rollResult, abilityId);
        rollResultText.text = "d20: " + rollResult;
        int resultEvent = RandomNumberGenerator.GetRandomBetween(0, selectedEvent.descriptions.Length - 1);
        if (result == "suc" | result == "critSuc")
        {
            eventDescriptionText.text = selectedEvent.successText[resultEvent];
        }
        else
        {
            eventDescriptionText.text = selectedEvent.failureText[resultEvent];
        }
        rollDiceBtn.interactable = false;
        decision1Btn.interactable = false;
        decision2Btn.interactable = false;
        continueRaceBtn.interactable = true;
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

    public void ContinueRace()
    {
        RacePanel.SetActive(true);
        RaceDecisionPanel.SetActive(false);
        RacePanel.SetActive(true);
        waitingForPlayer = false;
    }

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        currentLapText.text = string.Join("\n", logMessages);
    }

    private void AddEventMessage(string message)
    {
        lapEventMessages.Add(message);
        lapEventsText.text = string.Join("\n", lapEventMessages);
    }
}
