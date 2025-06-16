using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Season
{
    public int year;
    public List<Driver> drivers;
    public List<RaceResult> raceResults = new();
}
