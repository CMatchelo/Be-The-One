using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using UnityEngine.Localization.Settings;


public class PracticeManager
 : MonoBehaviour
{
    [Header("UI Items")]
    public Transform inRaceSkillsView;
    public Transform skillsView;
    public Transform abilitiesView;
    public MenuManager menuManager;

    [Header("Buttons")]

    public AbilityList abilityList;
    public GameObject abilityBox;


    private void Awake()
    {

    }
    private void Start()
    {
        LoadDatabases();
        PopulateAbilitiesList();
        PopulateSkillsList();
        PopulateInRaceSkillsList();
    }

    private void LoadDatabases()
    {
        TextAsset abilitesLocal = Resources.Load<TextAsset>("abilities");
        abilityList = JsonUtility.FromJson<AbilityList>(abilitesLocal.text);
    }
    async private void PopulateAbilitiesList()
    {
        foreach (Transform child in abilitiesView) Destroy(child.gameObject);
        foreach (var ability in abilityList.abilities)
        {
            bool alreadyHasAbility = SaveSession.CurrentGameData.profile.abilities
                .Any(a => a.id == ability.id);

            if (alreadyHasAbility)
                continue;

            GameObject row = Instantiate(abilityBox, abilitiesView, false);
            var abilityName = row.GetComponentInChildren<TextMeshProUGUI>();
            var practiceBtn = row.GetComponentInChildren<Button>();
            string localizedString = await SearchTextLocation.GetLocalizedStringAsync("AbilityNames", ability.name);
            abilityName.text = localizedString;
            practiceBtn.onClick.AddListener(() => PracticeItem(ability.name, practiceBtn, ability.referenceSkill));
        }
    }

    async private void PopulateSkillsList()
    {
        foreach (Transform child in skillsView) Destroy(child.gameObject);
        List<string> skills = new List<string> { "technique", "bravery", "potential", "charisma", "focus", "awareness" };
        foreach (string skillString in skills)
        {
            GameObject row = Instantiate(abilityBox, skillsView, false);
            var skillName = row.GetComponentInChildren<TextMeshProUGUI>();
            var practiceBtn = row.GetComponentInChildren<Button>();
            string localizedString = await SearchTextLocation.GetLocalizedStringAsync("Skills", skillString);
            skillName.text = localizedString;
            practiceBtn.onClick.AddListener(() => PracticeItem(skillString, practiceBtn));
        }
    }

    async private void PopulateInRaceSkillsList()
    {
        foreach (Transform child in inRaceSkillsView) Destroy(child.gameObject);
        foreach (var field in typeof(WeekendBonus).GetFields())
        {
            GameObject row = Instantiate(abilityBox, inRaceSkillsView, false);
            var skillName = row.GetComponentInChildren<TextMeshProUGUI>();
            var buyPoint = row.GetComponentInChildren<Button>();
            var btnText = buyPoint.GetComponentInChildren<TextMeshProUGUI>();

            string localizedString = await SearchTextLocation.GetLocalizedStringAsync("Skills", field.Name);
            var driver = SaveSession.CurrentGameData.profile.driver;
            var fielddd = driver.GetType().GetField(field.Name);
            int currentValue = (int)fielddd.GetValue(driver);
            var pricePoint = CostPerPoint(currentValue);

            if (pricePoint > SaveSession.CurrentGameData.profile.money) buyPoint.interactable = false;
            skillName.text = localizedString;
            btnText.text = $"{pricePoint}";
            buyPoint.onClick.AddListener(() => BuySkillPoint(field.Name, pricePoint));

        }
    }

    private void PracticeItem(string name, Button btn, string referenceSkill = null)
    {
        // Aumenta 1 ponto de treino para essa habilidade
        btn.interactable = false;
        menuManager.WeeklyAction -= 1;
        var field = typeof(PractingSkills).GetField(name);
        if (field == null)
        {
            Debug.LogWarning($"Skill or ability '{name}' not found in PractingSkill!");
            return;
        }

        int currentValue = (int)field.GetValue(SaveSession.CurrentGameData.profile.practingSkills);
        currentValue++;
        field.SetValue(SaveSession.CurrentGameData.profile.practingSkills, currentValue);

        Debug.Log($"{name} trained! ({currentValue}/10)");

        // Quando atingir 10 treinos...
        if (currentValue >= 10)
        {
            if (referenceSkill == null)
            {
                // é uma skill base ou weekend bonus
                UpgradeProfileSkill(name);
            }
            else
            {
                // é uma ability nova
                UnlockAbility(name, referenceSkill);
            }

            // zera o progresso de treino dessa skill
            field.SetValue(SaveSession.CurrentGameData.profile.practingSkills, 0);
        }
        SaveUtility.UpdateProfile();
    }

    private void UpgradeProfileSkill(string skillName)
    {
        var profileField = SaveSession.CurrentGameData.profile.GetType().GetField(skillName);
        if (profileField != null)
        {
            int currentValue = (int)profileField.GetValue(SaveSession.CurrentGameData.profile);
            profileField.SetValue(SaveSession.CurrentGameData.profile, currentValue + 1);
            Debug.Log($"Skill '{skillName}' increased to {currentValue + 1}!");
        }
        else if (SaveSession.CurrentGameData.profile.driver != null)
        {
            var driver = SaveSession.CurrentGameData.profile.driver;
            var field = driver.GetType().GetField(skillName);
            if (field != null)
            {
                int currentValue = (int)field.GetValue(driver);
                field.SetValue(driver, currentValue + 1);
                Debug.Log($"Driver skill '{skillName}' increased to {currentValue + 1}!");
            }
            else
            {
                Debug.LogWarning($"Skill '{skillName}' not found in Driver!");
            }
        }
    }

    private void UnlockAbility(string name, string referenceSkill)
    {
        var PlayerAbilityList = SaveSession.CurrentGameData.profile.abilities;

        if (PlayerAbilityList.Any(a => a.name == name))
        {
            Debug.Log($"Ability '{name}' already unlocked!");
            return;
        }

        //int newId = abilityList.Count > 0 ? abilityList.Max(a => a.id) + 1 : 0;
        int newId = abilityList.abilities.Find(a => a.name == name)?.id ?? -1;

        var newAbility = new Ability
        {
            id = newId,
            name = name,
            referenceSkill = referenceSkill
        };

        PlayerAbilityList.Add(newAbility);
        Debug.Log($"Unlocked new ability: {name}!");
    }

    private void BuySkillPoint(string skillName, int pricePoint)
    {
        Debug.Log("Comprando ponto " + skillName);
        var driver = SaveSession.CurrentGameData.profile.driver;
        var field = typeof(Driver).GetField(skillName);

        int currentValue = (int)field.GetValue(driver);
        field.SetValue(driver, currentValue + 1);

        SaveSession.CurrentGameData.profile.money -= pricePoint;

        SaveUtility.UpdateProfile();
        PopulateInRaceSkillsList();
    }

    private static int CostPerPoint(int level)
    {
        const int BASE_LEVEL = 70;
        const int BASE_COST = 5000;
        const double R = 1.35; // fator de crescimento exponencial

        Debug.Log(level);
        int exponent = level - BASE_LEVEL;
        if (exponent < 0) exponent = 0;

        double rawCost = BASE_COST * Math.Pow(R, exponent);

        // arredonda para o próximo múltiplo de 1000
        int rounded = (int)(Math.Ceiling(rawCost / 1000.0) * 1000);

        return rounded;
    }

    public void DisableBtns()
    {
        foreach (Transform child in abilitiesView)
        {
            var btn = child.GetComponentInChildren<Button>();
            btn.interactable = false;
        }
        foreach (Transform child in skillsView)
        {
            var btn = child.GetComponentInChildren<Button>();
            btn.interactable = false;
        }
    }
}