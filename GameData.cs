using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameData : MonoBehaviour
{
    public static List<ConstituentData> Constituents { get; private set; }
    public static bool AddConstituent(ConstituentData constituent, ConstituentState state)
    {
        ConstituentData constituentData = Constituents.Find(c => c == constituent);
        if (constituentData != null) return false; // Duplicate ConstituentData found.

        ConstituentState constituentState = constituentData.States.Find(s => s == state);
        if (constituentState != null) return false; // Duplicate ConstituentState found.

        return true;
    }
    public static ConstituentState GetConstituent(string constituentName, string stateName)
    {
        if (Constituents.Count == 0) return null; // No ConstituentData stored yet.

        ConstituentData constituentData = Constituents.Find(c => c.Name == constituentName);
        if (constituentData == null) return null; // No matching ConstituentData.

        ConstituentState constituentState = constituentData.States.Find(s => s.Name == stateName);
        if (constituentState == null) return null; // No matching ConstituentState.

        return constituentState;
    }

    // Load info from all files.
    private void Start()
    {
        Constituents = new List<ConstituentData>();
        ConstituentData constituent = new ConstituentData("dirt");
        constituent.AddState(new ConstituentState("dirt"));
        Constituents.Add(constituent);

        constituent = new ConstituentData("stone");
        constituent.AddState(new ConstituentState("stone"));
        Constituents.Add(constituent);
    }
}
