using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
[System.Serializable]
public class ChampionshipStatus
{
    public List<DriverStanding> driverStandings = new List<DriverStanding>();
    
    // Método para atualizar pontos
    public void AddPoints(string driverName, string teamName, int points)
    {
        var driver = driverStandings.Find(d => d.driverName == driverName);
        if (driver == null)
        {
            driver = new DriverStanding(driverName, teamName, points);
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
    public string driverName;
    public string teamName;
    public int points;

    public DriverStanding(string name, string team, int initialPoints)
    {
        driverName = name;
        teamName = team;
        points = initialPoints;
    }
}