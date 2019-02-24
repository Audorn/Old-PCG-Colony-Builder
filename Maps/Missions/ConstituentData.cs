using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The base data for a Constituent.  It will contain a list of possible States.
public class ConstituentData 
{
    public string Name { get; private set; }

    public List<ConstituentState> States { get; private set; }
    public bool AddState(ConstituentState state)
    {
        ConstituentState existingState = States.Find(s => s == state);
        if (existingState != null) return false;                        // Already exists.

        States.Insert(0, state);
        return true;                                                    // Successful.
    }
    public ConstituentState GetState(string name)
    {
        ConstituentState existingState = States.Find(s => s.Name == name);
        if (existingState == null) return null;                         // No match found.

        return existingState;                                           // Successful.

    }

    // Constructors
    public ConstituentData(string name)
    {
        Name = name;
        States = new List<ConstituentState>();
    }

}
