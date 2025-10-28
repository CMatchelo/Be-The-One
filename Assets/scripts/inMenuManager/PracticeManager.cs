using UnityEngine;
using TMPro;

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
            var trainButton = row.GetComponentInChildren<Button>();
            string localizedString = await SearchTextLocation.GetLocalizedStringAsync("AbilityNames", ability.name);
            abilityName.text = localizedString;
            trainButton.onClick.AddListener(() => PracticeItem(ability.name, ability.referenceSkill));
        }
    }

    async private void PopulateInRaceSkillsList()
    {
        foreach (Transform child in inRaceSkillsView) Destroy(child.gameObject);
        foreach (var field in typeof(WeekendBonus).GetFields())
        {
            GameObject row = Instantiate(abilityBox, inRaceSkillsView, false);
            string fieldName = field.Name;
            var value = field.GetValue(SaveSession.CurrentGameData.profile.weekendBonus);

            Debug.Log($"{fieldName}: {value}");
        }
    }

    private void PracticeItem(string name, string referenceSkill = null)
    {
        // Aumenta 1 ponto de treino para essa habilidade
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
        else if (SaveSession.CurrentGameData.profile.weekendBonus != null)
        {
            var bonusField = typeof(WeekendBonus).GetField(skillName);
            if (bonusField != null)
            {
                int currentValue = (int)bonusField.GetValue(SaveSession.CurrentGameData.profile.weekendBonus);
                bonusField.SetValue(SaveSession.CurrentGameData.profile.weekendBonus, currentValue + 1);
                Debug.Log($"Weekend bonus '{skillName}' increased to {currentValue + 1}!");
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
}