using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Track
{
    public string circuitName;
    public string country;
    public float highSpeedCorners;
    public float lowSpeedCorners;
    public float acceleration;
    public float topSpeed;
    public float statsFactor;
    public float lapLenghtFactor;
    public int circuitLength;
    public int totalLaps;
    public int difficulty;
}

[Serializable]
public class TracksList
{
    public List<Track> tracks;
}
