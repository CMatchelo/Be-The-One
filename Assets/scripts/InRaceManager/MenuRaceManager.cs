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
    public GameObject RaceMenuPanel;
    public FreePracticeManager freePracticeManager;
    public QualifyManager qualifyManager;

    [Header("UI Btns and Dropdowns")]
    public List<Button> GoToEventBtn;


    [Header("UI Texts")]
    public TMP_Text titleNews;
    public List<TMP_Text> fpTiresText = new List<TMP_Text>();
    public List<TMP_Text> fpTimesText = new List<TMP_Text>();

    public TeamsList teamsList;
    public Track selectedTrack;

    private List<string> fpTiresList = new List<string>();
    private List<float> fpTimesList = new List<float>();
    public List<Driver> qualifyingGrid = new List<Driver>();
    public int qualifyingPhase = 0;
    private DriversList driversList;
    public List<Driver> GetLoadedDrivers()
    {
        return driversList?.drivers;
    }





    void Awake()
    {
        LoadUtility.LoadGame("Cicero_g15866"); // Fix id load
        LoadDatabases();
        Debug.Log("Zerou");
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
            Debug.Log("Go To Race");
        }
        else if (eventNumber == 4)
        {
            RaceMenuPanel.SetActive(false);
            QualifyPanel.SetActive(true);
            qualifyManager.InitializePractice();
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

    void LoadDatabases()
    {
        TextAsset teamsLocal = Resources.Load<TextAsset>("TeamsDatabase");
        teamsList = JsonUtility.FromJson<TeamsList>(teamsLocal.text);

        LoadDrivers();
    }

    void LoadDrivers()
    {
        string path = Path.Combine(Application.persistentDataPath, "saves", "Cicero_g15866", "activeDriversList1.json"); // fix status
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
}
