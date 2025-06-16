using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Team
{
    public string teamName;
    public int highSpeedCorners;
    public int lowSpeedCorners;
    public int acceleration;
    public int topSpeed;
}

[Serializable]
public class TeamsList
{
    public List<Team> teams;
}