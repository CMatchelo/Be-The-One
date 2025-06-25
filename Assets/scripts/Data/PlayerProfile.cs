using System;
using System.Collections.Generic;

[Serializable]
public class AbilityList
{
    public Ability[] abilities;
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
    public int id;
    public string past;
    public int lastResults;
    public string status;
    public int technique;
    public int bravery;
    public int potential;
    public int charisma;
    public int focus;
    public int awareness;
    public List<Ability> abilities = new List<Ability>();
}

