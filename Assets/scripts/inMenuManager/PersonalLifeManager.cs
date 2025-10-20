using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;


public class PersonalLifeManager
 : MonoBehaviour
{
    [Header("UI Canvas")]
    public ScrollRect calendarTable;
    public ScrollRect raceTable;


    [Header("UI Btns and Dropdowns")]

    [Header("UI Texts")]
    public Transform calendarView;
    public Transform raceResultView;
    public GameObject raceRowPrefab;
    public GameObject standingLinePrefab;



    private void Awake()
    {

    }
    private void Start()
    {
        LoadDatabases();
    }

    private void LoadDatabases()
    {
    }
}