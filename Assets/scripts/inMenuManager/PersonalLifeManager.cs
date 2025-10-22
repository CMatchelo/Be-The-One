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
        string languageCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        TextAsset itemsLocal = null;
        if (languageCode == "en-US") itemsLocal = Resources.Load<TextAsset>("PersonalItems_en");
        if (languageCode == "pt-BR") itemsLocal = Resources.Load<TextAsset>("PersonalItems_pt");
        personalItemsList = JsonUtility.FromJson<PersonalItemsList>(itemsLocal.text);
        PopulateBuyList();
        PopulateSellList();
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
            itemName[0].text = item.name;
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
            itemName[0].text = item.name;
        }
    }

    private void BuyItem(PersonalItem newItem)
    {
        SaveSession.CurrentGameData.profile.money -= newItem.value;
        newItem.value = newItem.value - ((newItem.value * 15) / 100);
        SaveSession.CurrentGameData.profile.personalItemsList.personalItems.Add(newItem);
        SaveUtility.UpdateProfile();
        PopulateBuyList(); 
        PopulateSellList();
    }

    private void SellItem(PersonalItem soldItem)
    {
        Debug.Log("Vendendo " + soldItem.name);
    }

}