using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using UnityEngine.Localization.Settings;


public class PersonalLifeManager
 : MonoBehaviour
{
    [Header("UI Items")]
    public Transform buyView;
    public Transform sellView;
    public GameObject luxuryItemPrefab;

    [Header("Buttons")]
    public Button relationshipChief;
    public Button relationshipSponsor;
    public Button relationshipEngineer;

    public PersonalItemsList personalItemsList;


    private void Awake()
    {

    }
    private void Start()
    {
        LoadDatabases();
    }

    private void LoadDatabases()
    {
        TextAsset itemsLocal = Resources.Load<TextAsset>("PersonalItems");
        personalItemsList = JsonUtility.FromJson<PersonalItemsList>(itemsLocal.text);
        PopulateBuyList();
        PopulateSellList();
    }

    public string GetLocalizedName(PersonalItem item)
    {
        string languageCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        return languageCode switch
        {
            "pt-BR" => item.name.pt,
            "en-US" => item.name.en,
            _ => item.name.en
        };
    }

    private void PopulateBuyList()
    {
        if (SaveSession.CurrentGameData.profile.personalItemsList == null)
        {
            SaveSession.CurrentGameData.profile.personalItemsList = new PersonalItemsList();
        }
        foreach (Transform child in buyView) Destroy(child.gameObject);

        foreach (var item in personalItemsList.personalItems)
        {
            GameObject row = Instantiate(luxuryItemPrefab, buyView, false);
            var itemName = row.GetComponentsInChildren<TextMeshProUGUI>();
            var buyButton = row.GetComponentInChildren<Button>();
            var buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = "Buy";
            buyButton.onClick.AddListener(() => BuyItem(item));
            itemName[0].text = GetLocalizedName(item);
            //itemName[0].text = item.name;
            if (SaveSession.CurrentGameData.profile.money < item.value) buyButton.interactable = false;
        }


    }

    private void PopulateSellList()
    {
        foreach (Transform child in sellView) Destroy(child.gameObject);
        foreach (var item in SaveSession.CurrentGameData.profile.personalItemsList.personalItems)
        {
            GameObject row = Instantiate(luxuryItemPrefab, sellView, false);
            var itemName = row.GetComponentsInChildren<TextMeshProUGUI>();
            var sellButton = row.GetComponentInChildren<Button>();
            var buttonText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = "Sell";
            sellButton.onClick.AddListener(() => SellItem(item));
            itemName[0].text = GetLocalizedName(item);
        }
    }

    private void BuyItem(PersonalItem newItem)
    {
        var itemOwned = newItem.Clone();
        SaveSession.CurrentGameData.profile.money -= itemOwned.value;
        itemOwned.value = itemOwned.value - ((itemOwned.value * 15) / 100);
        itemOwned.propertyId = RandomStringGenerator.GenerateRandomString();
        SaveSession.CurrentGameData.profile.personalItemsList.personalItems.Add(itemOwned);
        SaveUtility.UpdateProfile();
        PopulateBuyList();
        PopulateSellList();
    }

    private void SellItem(PersonalItem soldItem)
    {
        SaveSession.CurrentGameData.profile.money += soldItem.value;
        SaveSession.CurrentGameData.profile.personalItemsList.personalItems.RemoveAll(item => item.propertyId == soldItem.propertyId);
        SaveUtility.UpdateProfile();
        PopulateBuyList();
        PopulateSellList();
    }

    public void ProfessionalRelationship(int relType)
    {
        if (relType == 0) SaveSession.CurrentGameData.profile.chiefRelationship += 2;
        if (relType == 1) SaveSession.CurrentGameData.profile.sponsorRelationship += 2;
        if (relType == 2) SaveSession.CurrentGameData.profile.engineerRelationship += 2;
        if (SaveSession.CurrentGameData.profile.chiefRelationship > 10) SaveSession.CurrentGameData.profile.chiefRelationship = 10;
        if (SaveSession.CurrentGameData.profile.sponsorRelationship > 10) SaveSession.CurrentGameData.profile.sponsorRelationship = 10;
        if (SaveSession.CurrentGameData.profile.engineerRelationship > 10) SaveSession.CurrentGameData.profile.engineerRelationship = 10;
        relationshipChief.interactable = false;
        relationshipSponsor.interactable = false;
        relationshipEngineer.interactable = false;
        SaveUtility.UpdateProfile();
    }
}