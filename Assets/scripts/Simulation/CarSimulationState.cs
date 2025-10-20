using UnityEngine;

public class CarSimulationState
{
    public string currentTire;
    public string driver;
    public string team;
    public float carFactor;
    public float totalTime;
    public float previousTotalTime = 0f;
    public float totalWear;
    public int totalLapTyre;
    public int lapsCompleted;
    public int bestLap;

    public CarSimulationState(string startingTire, float carFactor, string driver, string team)
    {
        this.carFactor = carFactor;
        currentTire = startingTire;
        totalTime = 0f;
        totalWear = 0f;
        totalLapTyre = 0;
        lapsCompleted = 0;
        bestLap = 1000;
        this.driver = driver;
        this.team = team;
    }
}

