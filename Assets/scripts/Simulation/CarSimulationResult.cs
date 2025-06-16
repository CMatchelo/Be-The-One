using System.Collections.Generic;
using UnityEngine;

public class CarSimulationResult
{
    public string trackName;
    public string date;
    public List<DriverResult> results = new();
}

public class DriverResult2 {
    public string driverName;
    public string teamName;
    public float totalTime;
}
