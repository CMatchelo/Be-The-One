using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
[System.Serializable]
public class ChampionshipStatus
{
    public List<DriverStanding> driverStandings = new List<DriverStanding>();
    
    // Método para atualizar pontos
    public void AddPoints(int driverId, int teamId, int points)
    {
        var driver = driverStandings.Find(d => d.driverId == driverId);
        if (driver == null)
        {
            driver = new DriverStanding(driverId, teamId, points);
            driverStandings.Add(driver);
        }
        else
        {
            driver.points += points;
        }
    }
}

[System.Serializable]
public class DriverStanding
{
    public int driverId;
    public int teamId;
    public int points;

    public DriverStanding(int name, int team, int initialPoints)
    {
        driverId = name;
        teamId = team;
        points = initialPoints;
    }
}