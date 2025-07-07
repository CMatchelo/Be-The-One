using UnityEngine;
using System.Linq;

public static class CalculateThrow
{

    public static int skillValue = -1;

    public static string CalculateD20(int difficulty, string skillName, out int rollResult, int abilityId = -1)
    {
        SelectSkillValue(skillName);
        bool abilityExists = SaveSession.CurrentGameData.profile.abilities.Any(ability => ability.id == abilityId);
        int roll = RollWithAdvantage(abilityId);
        if (roll == 20)
        {
            rollResult = roll;
            return "critSuc";
        }
        if (roll == 1)
        {
            rollResult = roll;
            return "critFail";
        }
        roll += skillValue;
        rollResult = roll;
        return roll >= difficulty ? "suc" : "fail";
    }
    public static void SelectSkillValue(string skillName)
    {
        switch (skillName.ToLower())
        {
            case "technique":
                skillValue = SaveSession.CurrentGameData.profile.technique;
                break;
            case "bravery":
                skillValue = SaveSession.CurrentGameData.profile.bravery;
                break;
            case "potential":
                skillValue = SaveSession.CurrentGameData.profile.potential;
                break;
            case "charisma":
                skillValue = SaveSession.CurrentGameData.profile.charisma;
                break;
            case "focus":
                skillValue = SaveSession.CurrentGameData.profile.focus;
                break;
            case "awareness":
                skillValue = SaveSession.CurrentGameData.profile.awareness;
                break;
            default:
                Debug.LogError("Skill desconhecida: " + skillName);
                return;
        }
    }
    private static int RollWithAdvantage(int abilityId)
    {
        // Rolls once if ability does not exists
        if (abilityId == 0 || !SaveSession.CurrentGameData.profile.abilities.Any(a => a.id == abilityId))
            return Random.Range(1, 21);

        // Else, rolls twice and gets the bigger value
        int roll1 = Random.Range(1, 21);
        int roll2 = Random.Range(1, 21);
        return Mathf.Max(roll1, roll2);
    }
}