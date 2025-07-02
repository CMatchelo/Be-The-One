using System;
using System.Collections.Generic;

[Serializable]
public class Sponsor
{
    public string realName;
    public string name;
    public string presentation;
    public int valuePerRace;
    public int valuePerGoal;
    public string preferredProfile;
    public int minimumDriverAverage;
    public List<string> goals;
}

[Serializable]
public class SponsorsList
{
    public List<Sponsor> sponsors;
}
