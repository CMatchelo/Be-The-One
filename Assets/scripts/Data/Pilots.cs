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
    public string birthDate;
    public int highSpeedCorners;
    public int lowSpeedCorners;
    public int acceleration;
    public int topSpeed;
    public int teamId;
    public int id;
    public bool active;
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
