using System;
using System.Collections.Generic;

[Serializable]
public class Ability
{
    public string name;
    public string referenceSkill;

}
[Serializable]
public class PlayerProfile
{
    public string playerFirstName;
    public string playerLastName;
    public int age;
    public Nationality nationality;
    public int teamId;
    public int companionId;
    public int id;
    public string past;
    public int lowSpeedCorners;
    public int highSpeedCorners;
    public int topSpeed;
    public int acceleration;
    public int technique;
    public int bravery;
    public int potential;
    public int charisma;
    public int focus;
    public int awareness;
    public List<Ability> abilities;
}

