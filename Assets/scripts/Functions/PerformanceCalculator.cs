using UnityEngine;

public static class PerformanceCalculator
{
    public static float CalculateCarFactor(Driver driver, Team team, Track selectedTrack)
    {
        if (driver == null || team == null || selectedTrack == null)
        {
            Debug.LogError("Driver, Team or Track is null!");
            return 0f;
        }
        float inpiration = UnityEngine.Random.Range(-5f, 5f);
        float highSpeedFactor = ((inpiration + driver.highSpeedCorners + team.highSpeedCorners) / 2) * selectedTrack.highSpeedCorners;
        float lowSpeedFactor = ((inpiration + driver.lowSpeedCorners + team.lowSpeedCorners) / 2) * selectedTrack.lowSpeedCorners;
        float accelerationFactor = ((inpiration + driver.acceleration + team.acceleration) / 2) * selectedTrack.acceleration;
        float topSpeedFactor = ((inpiration + driver.topSpeed + team.topSpeed) / 2) * selectedTrack.topSpeed;
        float totalFactor = (highSpeedFactor + lowSpeedFactor + accelerationFactor + topSpeedFactor) / 10000;

        float timeBase = Mathf.Pow(1 - totalFactor, selectedTrack.statsFactor) * selectedTrack.lapLenghtFactor;

        /* Debug.Log($"{driver.firstName} | {inpiration} |  {highSpeedFactor} | {lowSpeedFactor} | {accelerationFactor} | {topSpeedFactor}"); */

        return timeBase; // placeholder
    }
}
