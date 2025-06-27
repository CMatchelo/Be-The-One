using System;
using System.Collections.Generic;

[Serializable]
public class PressOutlet
{
    public string outlet;
    public List<string> reporters;
}

[Serializable]
public class PressList
{
    public List<PressOutlet> pressOutlet;
}
