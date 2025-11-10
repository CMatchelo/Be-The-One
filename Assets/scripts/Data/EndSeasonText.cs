using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
[System.Serializable]
public class EndSeasonTextList
{
    public List<EndSeasonText> endSeasonTextList = new List<EndSeasonText>();
}
[System.Serializable]
public class EndSeasonText
{
    public int id;
    public LocalizedText driverNoContract1;
    public LocalizedText driverNoContract2;
    public LocalizedText driverNoContract3;
}

[System.Serializable]
public class LocalizedText
{
    public string en;
    public string pt;
}