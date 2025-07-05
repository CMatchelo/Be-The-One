[System.Serializable]
public class Decision
{
    public string decision;
    public string referenceSkill;
    public int ability;
    public bool hasAbility;
}

[System.Serializable]
public class FreePracticeEvent
{
    public string id;
    public string[] descriptions;
    public Decision[] decisions;
    public string[] successText;
    public string[] failureText;
}

[System.Serializable]
public class FreePracticeEventList
{
    public FreePracticeEvent[] events;
}
