using System;
using System.Collections.Generic;

[Serializable]
public class AbilityList
{
    public List<Ability> abilities = new List<Ability>();
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
public class PractingSkills
{
    public int corneringMastery = 0;
    public int vehicleControl = 0;
    public int showman = 0;
    public int trackAdaptation = 0;
    public int fastLapSpecialist = 0;
    public int aggressiveOvertaking = 0;
    public int positionDefense = 0;
    public int simulatorTraining = 0;
    public int continuousDevelopment = 0;
    public int telemetryReading = 0;
    public int technicalCoaching = 0;
    public int mediaHandling = 0;
    public int contractNegotiation = 0;
    public int teamPresence = 0;
    public int socialMedia = 0;
    public int negotiator = 0;
    public int pressureResistance = 0;
    public int quickDecisionMaking = 0;
    public int strategicInsight = 0;
    public int socialPerception = 0;
    public int mechanicalSensitivity = 0;
    public int highSpeedCorners = 0;
    public int lowSpeedCorners = 0;
    public int acceleration = 0;
    public int topSpeed = 0;
    public int technique = 0;
    public int bravery = 0;
    public int potential = 0;
    public int charisma = 0;
    public int focus = 0;
    public int awareness = 0;
}
[Serializable]
public class PlayerProfile
{
    public Driver driver;
    public WeekendBonus weekendBonus = new WeekendBonus();
    public PractingSkills practingSkills = new PractingSkills();
    public int id;
    public string past;
    public int ambitious = 5;
    public int loyal = 5;
    public int charismatic = 5;
    public int neutral = 0;
    public int lastResults = 49;
    public int teammateId = -1;
    public Sponsor sponsorMaster;
    public Sponsor sponsorSecondary;
    public string status;
    public int technique;
    public int bravery;
    public int potential;
    public int charisma;
    public int focus;
    public int awareness;
    public int money = 0;
    public PersonalItemsList personalItemsList = new PersonalItemsList();
    public int engineerRelationship = 0;
    public int chiefRelationship = 0;
    public int sponsorRelationship = 0;
    public List<Ability> abilities = new List<Ability>();
}

