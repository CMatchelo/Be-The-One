using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PersonalItem
{
    public int id;
    public string propertyId;
    public LocalizedName name;
    public int value;
    public PersonalItem(int id, LocalizedName name, int value)
    {
        this.id = id;
        this.name = name;
        this.value = value;
    }
}

[Serializable]
public class LocalizedName
{
    public string en;
    public string pt;
}

[Serializable]
public class PersonalItemsList
{
    public List<PersonalItem> personalItems;
    public PersonalItemsList(List<PersonalItem> personalItems)
    {
        this.personalItems = personalItems;
    }
    public PersonalItemsList()
    {
        personalItems = new List<PersonalItem>();
    }
}