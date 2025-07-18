using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
[System.Serializable]
public class RaceResult
{
    public string trackName;
    public string date;
    public List<DriverResult> results = new List<DriverResult>();
}
[System.Serializable]
public class DriverResult {
    public string driverName;
    public string teamName;
    public float totalTime;
}