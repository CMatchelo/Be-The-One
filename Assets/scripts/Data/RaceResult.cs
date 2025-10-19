using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
[System.Serializable]
public class RaceResult
{
    public string trackName;
    public List<DriverResult> results = new List<DriverResult>();
    public RaceResult(string trackName, List<DriverResult> results)
    {
        this.trackName = trackName;
        this.results = results;
    }
}
[System.Serializable]
public class DriverResult {
    public Driver driver;
    public int position;
    public float totalTime;
    public float lastLap;
    public float bestLap;
    public DriverResult(Driver driver, int position, float totalTime, float lastLap, float bestLap)
    {
        this.driver = driver;
        this.position = position;
        this.totalTime = totalTime;
        this.lastLap = lastLap;
        this.bestLap = bestLap;
    }
}