using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
[System.Serializable]
public class DriversChampionshipStatus
{
    public List<DriverStanding> driverStandings = new List<DriverStanding>();

    // Método para atualizar pontos
    /* public void AddPoints(int driverId, int teamId, int points)
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
    } */
}

[System.Serializable]
public class TeamsChampionshipStatus
{
    public List<TeamStanding> teamStandings = new List<TeamStanding>();
}

[System.Serializable]
public class DriverStanding
{
    public int driverId;
    public int teamId;
    public int points;

    public DriverStanding(int driverId, int teamId, int points)
    {
        this.driverId = driverId;
        this.teamId = teamId;
        this.points = points;
    }
}

[System.Serializable]
public class TeamStanding
{
    public int teamId;
    public int points;

    public TeamStanding(int teamId, int points)
    {
        this.teamId = teamId;
        this.points = points;
    }
}