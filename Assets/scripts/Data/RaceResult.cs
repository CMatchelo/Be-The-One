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
    public Driver driver;
    public int position;
    public float totalTime;
    public float lastLap;
    public DriverResult(Driver driver, int position, float totalTime, float lastLap)
    {
        this.driver = driver;
        this.position = position;
        this.totalTime = totalTime;
        this.lastLap = lastLap;
    }
}