using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using System.Collections;
using UnityEngine.Localization.Settings;


public class SponsorshipManager : MonoBehaviour
{
    [Header("Canvas Elements")]
    public GameObject SponsorNegotiationPanel;
    public GameObject MenuPanel;
    public MenuManager menuManager;

    [Header("Dropdown/BTN Elements")]
    public TMP_Dropdown sponsorsDropdown;
    public Button signBtn;

    [Header("Text Elements")]
    public TMP_Text sponsorNameText;
    public TMP_Text sponsorPresentationText;
    public TMP_Text objective1Text;
    public TMP_Text objective2Text;
    public TMP_Text objective1ValueText;
    public TMP_Text objective2ValueText;

    [Header("Settings")]
    private SponsorsList sponsorsList;
    private List<Sponsor> availableSponsors = new List<Sponsor>();
    private Sponsor selectedSponsor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void LoadDatabase()
    {
        string languageCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        TextAsset sponsorListJson = null;
        if (languageCode == "en-US") sponsorListJson = Resources.Load<TextAsset>("Sponsors_en");
        if (languageCode == "pt-BR") sponsorListJson = Resources.Load<TextAsset>("Sponsors_pt");
        sponsorsList = JsonUtility.FromJson<SponsorsList>(sponsorListJson.text);
        sponsorsDropdown.onValueChanged.AddListener((int index) => OnSelectSponsor(index));
        PopulateSponsorsDropdown();
        signBtn.onClick.AddListener(SignContract);
    }

    async void OnSelectSponsor(int index)
    {
        Debug.Log("Selected spontos: " + index);
        string localizedString = await SearchTextLocation.GetLocalizedStringAsync("SponsorPanel", "eachRaceText");
        selectedSponsor = availableSponsors[index];
        sponsorNameText.text = selectedSponsor.name;
        sponsorPresentationText.text = selectedSponsor.presentation;
        objective1Text.text = selectedSponsor.goals[0];
        objective2Text.text = selectedSponsor.goals[1];
        objective1ValueText.text = localizedString + ": " + selectedSponsor.valuePerGoal;
        objective2ValueText.text = localizedString + ": " + selectedSponsor.valuePerGoal;
    }

    void SignContract()
    {
        SaveSession.CurrentGameData.profile.sponsorMaster = selectedSponsor;
        SaveUtility.UpdateProfile();
        SponsorNegotiationPanel.SetActive(false);
        MenuPanel.SetActive(true);
        menuManager.CheckNegotiations();
        /* StartCoroutine(DisableSponsorManagerNextFrame()); */
    }

    void PopulateSponsorsDropdown()
    {
        if (sponsorsList == null)
        {
            Debug.LogError("Falha ao carregar ou lista vazia");
            return;
        }

        sponsorsDropdown.ClearOptions();
        List<string> sponsorsNames = new List<string>();

        foreach (Sponsor sponsor in sponsorsList.sponsors)
        {
            Debug.Log(sponsor.name + " " + sponsor.minimumDriverAverage + " " + SaveSession.CurrentGameData.profile.driver.Average);
            if (sponsor.minimumDriverAverage <= SaveSession.CurrentGameData.profile.driver.Average)
            {
                availableSponsors.Add(sponsor);
                sponsorsNames.Add(sponsor.name);
            }
        }
        sponsorsDropdown.AddOptions(sponsorsNames);
        OnSelectSponsor(0);
    }

    /* private IEnumerator DisableSponsorManagerNextFrame()
    {
        yield return null; // Espera 1 frame
        gameObject.SetActive(false);
    }  */
}
