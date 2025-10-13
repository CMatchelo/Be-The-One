using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public GameObject step1Panel;
    public GameObject step2Panel;
    public GameObject initialScreen;
    public Button newGameBtn;
    public Button loadGameBtn;
    [SerializeField] private Transform savesListContainer;
    [SerializeField] private GameObject SaveFileBtn;


    void Start()
    {
        initialScreen.SetActive(true);
        step1Panel.SetActive(false);
        step2Panel.SetActive(false);
    }

    public void OnNewGameClick()
    {
        initialScreen.SetActive(false);
        step1Panel.SetActive(true);
    }

    public void PopulateSavesList()
    {
        foreach (Transform child in savesListContainer)
        {
            Destroy(child.gameObject);
        }

        string savesPath = Path.Combine(Application.persistentDataPath, "saves");
        
        if (!Directory.Exists(savesPath))
        {
            Debug.LogWarning("No saves directory found");
            return;
        }

        string[] saveDirectories = Directory.GetDirectories(savesPath);
        
        if (saveDirectories.Length == 0)
        {
            Debug.Log("No save games found");
            return;
        }

        foreach (string savePath in saveDirectories)
        {
            string saveId = Path.GetFileName(savePath);
            
            GameObject buttonObj = Instantiate(SaveFileBtn, savesListContainer, false);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                Debug.LogError("Componente TextMeshProUGUI não encontrado no botão");
                continue;
            }
            buttonText.text = saveId;
            button.onClick.AddListener(() => LoadSaveGame(saveId));
        }
    }

    public void LoadSaveGame(string saveId)
    {
        LoadUtility.LoadGame(saveId);
        SceneManager.LoadScene("MenuScene");
    }

}