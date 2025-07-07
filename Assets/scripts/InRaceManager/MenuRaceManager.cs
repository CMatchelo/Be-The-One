using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Linq;

public class MenuRaceManager : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject FPPanel;
    public GameObject RaceMenuPanel;
    public FreePracticeManager freePracticeManager;

    [Header("UI Btns and Dropdowns")]
    public List<Button> GoToEventBtn;


    [Header("UI Texts")]
    public TMP_Text titleNews;

    public static MenuRaceManager Instance;
    public int currentWeekendStep = 1;
    public List<string> practicedSkill = new List<string>();

    private GameObject currentFreePractice;


    void Awake()
    {
        LoadUtility.LoadGame("Cicero_g15866"); // Fix id load
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < GoToEventBtn.Count; i++)
        {
            int capturedIndex = i + 1;
            GoToEventBtn[i].onClick.AddListener(() => GoToEvent(capturedIndex));
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
            Debug.Log("Go To Quali");
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
}
