using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Linq;
using System.IO;

public class MenuRaceManager : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject FPPanel;
    public GameObject QualifyPanel;
    public GameObject RacePanel;
    public GameObject RaceMenuPanel;
    public FreePracticeManager freePracticeManager;
    public QualifyManager qualifyManager;
    public RaceManager raceManager;

    [Header("UI Btns and Dropdowns")]
    public List<Button> GoToEventBtn;
    public Button endWeekend;


    [Header("UI Texts")]
    public TMP_Text titleNews;
    public List<TMP_Text> fpTiresText = new List<TMP_Text>();
    public List<TMP_Text> fpTimesText = new List<TMP_Text>();

    public TeamsList teamsList;
    public Track selectedTrack;

    private List<string> fpTiresList = new List<string>();
    private List<float> fpTimesList = new List<float>();
    public List<Driver> qualifyingGrid = new List<Driver>();
    public Dictionary<int, float> qualifyingTimes = new Dictionary<int, float>();
    public int qualifyingPhase = 0;

    private FreePracticeEventList FPEventList;
    public List<FreePracticeEvent> GetFPEvents()
    {
        return FPEventList?.events?.ToList();
    }

    private FreePracticeEventList QualifyEventList;
    public List<FreePracticeEvent> GetQualifyEvents()
    {
        return QualifyEventList?.events?.ToList();
    }

    private FreePracticeEventList OvertakingEvents;
    public List<FreePracticeEvent> GetOvertakingEvents()
    {
        return OvertakingEvents?.events?.ToList();
    }

    private FreePracticeEventList OvertakenEvents;
    public List<FreePracticeEvent> GetOvertakenEvents()
    {
        return OvertakenEvents?.events?.ToList();
    }

    private AbilityList abilityList;
    public List<Ability> GetAbilities()
    {
        return abilityList?.abilities?.ToList();
    }

    private DriversList driversList;
    public List<Driver> GetLoadedDrivers()
    {
        return driversList?.drivers;
    }

    void Awake()
    {
        //LoadUtility.LoadGame(SaveSession.CurrentGameData.saveId); // Fix id load
        LoadDatabases();
        SaveSession.CurrentGameData.profile.weekendBonus.highSpeedCorners = 0;
        SaveSession.CurrentGameData.profile.weekendBonus.lowSpeedCorners = 0;
        SaveSession.CurrentGameData.profile.weekendBonus.acceleration = 0;
        SaveSession.CurrentGameData.profile.weekendBonus.topSpeed = 0;
        
    }

    void Start()
    {
        ChampionshipManager.Initialize();
        selectedTrack = ChampionshipManager.GetNextRace();
        for (int i = 0; i < GoToEventBtn.Count; i++)
        {
            int capturedIndex = i + 1;
            GoToEventBtn[i].onClick.AddListener(() => GoToEvent(capturedIndex));
        }
        for (int i = 0; i < 3; i++)
        {
            fpTiresText[i].text = "FP não feito";

            fpTimesText[i].text = "FP não feito";
        }
        endWeekend.interactable = false;
        endWeekend.onClick.AddListener(GoToMenu);
    }

    public void UpdateFPTexts(string tire, float time)
    {
        fpTiresList.Add(tire);
        fpTimesList.Add(time);
        for (int i = 0; i < 3; i++)
        {
            fpTiresText[i].text = (fpTiresList != null && i < fpTiresList.Count)
                ? fpTiresList[i]
                : "FP não feito";

            fpTimesText[i].text = (fpTimesList != null && i < fpTimesList.Count)
                ? fpTimesList[i].ToString("F2")
                : "FP não feito";
        }
    }

    void GoToEvent(int eventNumber)
    {
        if (eventNumber == 5)
        {
            /* GenerateTestQualifyingGrid(); */
            RaceMenuPanel.SetActive(false);
            RacePanel.SetActive(true);
            raceManager.InitializeRace();
        }
        else if (eventNumber == 4)
        {
            RaceMenuPanel.SetActive(false);
            QualifyPanel.SetActive(true);
            qualifyManager.InitializeQuali();
        }
        else
        {
            RaceMenuPanel.SetActive(false);
            FPPanel.SetActive(true);
            freePracticeManager.InitializePractice();
        }

        for (int i = 0; i < eventNumber; i++)
        {
            GoToEventBtn[i].interactable = false;
        }
    }

    void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    void LoadDatabases()
    {
        TextAsset teamsLocal = Resources.Load<TextAsset>("TeamsDatabase");
        teamsList = JsonUtility.FromJson<TeamsList>(teamsLocal.text);

        TextAsset FPEventsJson = Resources.Load<TextAsset>("FreePracticeEvents");
        FPEventList = JsonUtility.FromJson<FreePracticeEventList>(FPEventsJson.text);

        TextAsset QualifyEventsJson = Resources.Load<TextAsset>("QualifyEvents");
        QualifyEventList = JsonUtility.FromJson<FreePracticeEventList>(QualifyEventsJson.text);

        TextAsset OvertakingEventsJson = Resources.Load<TextAsset>("OvertakingEvents");
        OvertakingEvents = JsonUtility.FromJson<FreePracticeEventList>(OvertakingEventsJson.text);

        TextAsset OvertakenEventsJson = Resources.Load<TextAsset>("OvertakenEvents");
        OvertakenEvents = JsonUtility.FromJson<FreePracticeEventList>(OvertakenEventsJson.text);

        TextAsset abilityJson = Resources.Load<TextAsset>("abilities");
        abilityList = JsonUtility.FromJson<AbilityList>(abilityJson.text);

        LoadDrivers();
    }

    void LoadDrivers()
    {
        string path = Path.Combine(Application.persistentDataPath, "saves", "Cicero_g15866", "activeDriversList.json"); // fix status
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            driversList = JsonUtility.FromJson<DriversList>(json);
            driversList.drivers = driversList.drivers
                .Where(driver => driver.role == 0 || driver.role == 1)
                .ToList();
        }
        else
        {
            Debug.LogWarning($"Arquivo não encontrado em: {path}");
        }
    }

    //////////

    public void GenerateTestQualifyingGrid()
    {
        // Garante que a lista está limpa
        qualifyingGrid.Clear();

        // Ordena os pilotos pela soma das habilidades principais (como um "valor geral")
        List<Driver> orderedDrivers = driversList.drivers
            .Where(d => d.active && (d.role == 0 || d.role == 1)) // Só pilotos ativos titulares e reservas
            .OrderByDescending(d => d.highSpeedCorners + d.lowSpeedCorners + d.acceleration + d.topSpeed)
            .ToList();

        // Adiciona todos à lista de grid (do mais rápido pro mais lento)
        qualifyingGrid.AddRange(orderedDrivers);

        Debug.Log("Grid de largada gerado automaticamente para testes.");
        for (int i = 0; i < qualifyingGrid.Count; i++)
        {
            Debug.Log($"{i + 1}º - {qualifyingGrid[i].firstName} {qualifyingGrid[i].lastName}");
        }
    }

}
