using UnityEngine;
using System.Collections;

public class SliceMeshTop : SliceMesh
{
    public void SetSlice(Slice slice) { Slice = slice; }

    private void Start()
    {
        base.Initialize();
    }

}
