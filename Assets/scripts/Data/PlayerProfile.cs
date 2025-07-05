using System;
using System.Collections.Generic;

[Serializable]
public class AbilityList
{
    public Ability[] abilities;
}
[Serializable]
public class WeekendBonus
{
    public int highSpeedCorners;
    public int lowSpeedCorners;
    public int acceleration;
    public int topSpeed;
}
[Serializable]
public class Ability
{
    public int id;
    public string name;
    public string referenceSkill;
}
[Serializable]
public class PlayerProfile
{
    public Driver driver;
    public WeekendBonus weekendBonus = new WeekendBonus();
    public int id;
    public string past;
    public int ambitious = 5;
    public int loyal = 5;
    public int charismatic = 5;
    public int neutral = 0;
    public int lastResults = 49;
    public Sponsor sponsorMaster;
    public Sponsor sponsorSecondary;
    public string status;
    public int technique;
    public int bravery;
    public int potential;
    public int charisma;
    public int focus;
    public int awareness;
    public List<Ability> abilities = new List<Ability>();
}

