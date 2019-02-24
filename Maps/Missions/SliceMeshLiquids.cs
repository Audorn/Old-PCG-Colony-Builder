using UnityEngine;
using System.Collections;

public class SliceMeshLiquids : SliceMesh
{
    public void SetSlice(Slice slice) { Slice = slice; }

    private void Start()
    {
        base.Initialize();
    }
}
