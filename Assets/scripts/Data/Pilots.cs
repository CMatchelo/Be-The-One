using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Driver
{
    public string firstName;
    public string lastName;
    public string realFirstName;
    public string realLastName;
    public Nationality nationality;
    public string team;
    public int age;
    public int yearsOfContract;
    public int highSpeedCorners;
    public int lowSpeedCorners;
    public int acceleration;
    public int topSpeed;
    public int teamId;
    public int id;
    public bool active;
    public int role;
    public List<Result> results = new List<Result>();
    public float Average
    {
        get { return (highSpeedCorners + lowSpeedCorners + acceleration + topSpeed) / 4f; }
    }
}

[Serializable]
public class DriversList
{
    public List<Driver> drivers;
}

[Serializable]
public class Result
{
    public int season;
    public string track;
    public int position;
    public float bestLap;
    public float totalTime;

    public Result(int season, string track, int position, float bestLap, float totalTime)
    {
        this.season = season;
        this.track = track;
        this.position = position;
        this.bestLap = bestLap;
        this.totalTime = totalTime;
    }
}