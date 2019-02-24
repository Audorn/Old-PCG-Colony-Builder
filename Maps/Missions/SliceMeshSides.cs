using UnityEngine;
using System.Collections;

public class SliceMeshSides : SliceMesh
{
    // Child Meshes
    public SliceMeshTop SliceMeshTop;
    public SliceMeshLiquids SliceMeshLiquids;
    public void SetSlice(Slice slice)
    {
        Slice = slice;
        SliceMeshTop.SetSlice(slice);
        SliceMeshLiquids.SetSlice(slice);
    }


    // Connect to Child Meshes.
    void Start()
    {
        base.Initialize();
    }
}
