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
    public TMP_Text outputText;
    public TMP_Text eventDescriptionText;
    public TMP_Text rollResultText;
    public TMP_Text difficultyValueText;
    public TMP_Text skillTestText;
    public TMP_Text abilityTestText;

    private List<(Driver driver, float totalTime, float lastLap)> raceState = new();
    private List<string> logMessages = new();
    private List<int> eventsDone = new List<int>();
    private bool waitingForPlayer = false;
    private int selectedEventIndex = -1;
    private int currentDifficulty = 10;
    private int difficultyBase = 10;
    private string decisionReferenceSkill = null;
    private int abilityId = -1;
    private int rollResult = 0;

    private void Start()
    {

    }

    public void InitializePractice()
    {
        menuRaceManager = FindFirstObjectByType<MenuRaceManager>();
        startRaceButton.onClick.AddListener(StartRace);
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
            float carFactor = PerformanceCalculator.CalculateCarFactor(driver,
                menuRaceManager.teamsList.teams.Find(t => t.id == driver.teamId),
                track);

            var car = new CarSimulationState("Soft", carFactor, driver.firstName, driver.teamId.ToString());
            raceState.Add((driver, 0f, 0f)); // tempo total, última volta
        }

        // Loop das voltas
        for (int lap = 1; lap <= track.totalLaps; lap++)
        {
            logMessages.Clear();
            outputText.text = "";
            List<(Driver driver, float totalTime, float lastLap)> updatedState = new();

            for (int i = 0; i < raceState.Count; i++)
            {
                var entry = raceState[i];
                var driver = entry.driver;

                float carFactor = PerformanceCalculator.CalculateCarFactor(driver,
                    menuRaceManager.teamsList.teams.Find(t => t.id == driver.teamId),
                    track);

                var car = new CarSimulationState("Soft", carFactor, driver.firstName, driver.teamId.ToString());

                float lapTime = RaceSimulatorUtility.CalculateLapTime(car, trackFactor, track.circuitLength);

                // Verificar ultrapassagem (exceto para o líder)
                if (i > 0)
                {
                    float frontTotal = updatedState[i - 1].totalTime;
                    float behindTotal = entry.totalTime + lapTime;

                    // Se o piloto de trás ficaria na frente do da frente
                    if (behindTotal < frontTotal)
                    {
                        Driver overtaker = driver;
                        Driver overtaken = updatedState[i - 1].driver;
                        int playerId = SaveSession.CurrentGameData.profile.driver.id;
                        if (overtaker.id == playerId || overtaken.id == playerId)
                        {
                            AddLogMessage($"Jogador envolvido em ultrapassagem: {overtaker.firstName} → {overtaken.firstName}");
                            waitingForPlayer = true;
                            RaceMenuPanel.SetActive(false);
                            RaceDecisionPanel.SetActive(true);
                            PickNextEvent();
                            yield return new WaitUntil(() => waitingForPlayer == false);

                        }
                        else
                        {
                            rollResult = RandomNumberGenerator.GetRandomBetween(1, 20);
                        }
                        // Aplicar erro grave se roll == 1
                        if (rollResult == 1)
                        {
                            float penalty = UnityEngine.Random.Range(9f, 11f);
                            lapTime += penalty;
                            AddLogMessage($"{driver.firstName} cometeu um erro grave! Perdeu {penalty:F1}s.");
                        }
                        if (rollResult < track.difficulty && rollResult != 1)
                        {
                            // Não ultrapassa: tempo ajustado para ficar atrás do piloto da frente
                            float penalty = UnityEngine.Random.Range(0.1f, 0.5f);
                            behindTotal = frontTotal - penalty;
                            lapTime = behindTotal - entry.totalTime;

                            AddLogMessage($"{driver.firstName} ficou preso atrás de {updatedState[i - 1].driver.firstName} (Roll: {rollResult} ≤ {track.difficulty})");
                        }
                        else
                        {
                            AddLogMessage($"{driver.firstName} ultrapassou {updatedState[i - 1].driver.firstName} (Roll: {rollResult} > {track.difficulty})");
                        }
                    }

                    updatedState.Add((driver, behindTotal, lapTime));
                }

                else
                {
                    // Líder, simula normal
                    updatedState.Add((driver, entry.totalTime + lapTime, lapTime));
                }
            }

            // Atualizar a raceState e ordenar
            raceState = updatedState.OrderBy(t => t.totalTime).ToList();

            // Construir log
            AddLogMessage($"--- Volta {lap}/{track.totalLaps} ---");
            for (int i = 0; i < raceState.Count; i++)
            {
                var entry = raceState[i];
                AddLogMessage($"{i + 1}º - {entry.driver.firstName} - {entry.lastLap:F3}s - Total: {entry.totalTime:F3}s");
            }
            yield return new WaitForSeconds(0.5f);
        }

        AddLogMessage("--- Corrida Finalizada! ---");
        AddLogMessage("Classificação Final:");
        for (int i = 0; i < raceState.Count; i++)
        {
            AddLogMessage($"{i + 1}º - {raceState[i].driver.firstName} - Total: {raceState[i].totalTime:F3}s");
        }
    }

    void PickNextEvent()
    {
        rollResultText.text = "";
        skillTestText.text = "";
        abilityTestText.text = "";
        // Select event

        List<FreePracticeEvent> eventList = menuRaceManager.GetOvertakeEvents();

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
        RaceMenuPanel.SetActive(true);
        RaceDecisionPanel.SetActive(false);
        waitingForPlayer = false;
    }

    private void AddLogMessage(string message)
    {
        logMessages.Add(message);
        outputText.text = string.Join("\n", logMessages);
    }
}
