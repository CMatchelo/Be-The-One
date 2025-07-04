[System.Serializable]
public class FreePracticeEvent
{
    public string id;
    public string[] descriptions;
    public string[] successText;
    public string[] failureText;
    public string referenceSkill;
    public bool hasAbility;
    public int ability;
}

[System.Serializable]
public class FreePracticeEventList
{
    public FreePracticeEvent[] events;
}
