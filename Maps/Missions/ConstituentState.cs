using UnityEngine;
using System.Collections;

// A single state that a Constituent may be in, and all its settings.
public class ConstituentState
{
    public string Name { get; private set; }
    public Opacity Opacity { get; private set; }


    // Constructors
    public ConstituentState(string name, Opacity opacity = Opacity.Opaque)
    {
        Name = name;
        Opacity = opacity;
    }
}
