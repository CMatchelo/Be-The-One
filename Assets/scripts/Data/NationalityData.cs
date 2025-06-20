using System.Collections.Generic;

[System.Serializable]
public class Nationality
{
    public string country;
    public string nationality;
    public string pluralNationality;
}

[System.Serializable]
public class NationalitiesList
{
    public List<Nationality> nationalities;
}