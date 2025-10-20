using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PersonalItems
{
    public string name;
    public int value;

}

[Serializable]
public class PersonalItemsList
{
    public List<PersonalItems> personalItemsList;
}