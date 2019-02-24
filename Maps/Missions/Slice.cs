using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis { Y, X, Z }

public class Slice
{
    // Visualization
    public SliceMeshSides SliceMeshSides { get; private set; } // For real world position, which matches x,y,z in Slices[,,].
    public void SetSliceMeshSides(SliceMeshSides sides) { SliceMeshSides = sides; }
    public bool updateMeshSides = true;     // Will automatically render after creation.
    public bool updateMeshTop = true;       // Will automatically render after creation.
    public bool updateMeshLiquids = true;   // Will automatically render after creation.
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }

    public SliceMeshData BuildMesh(SliceMeshData.Type meshType)
    {
        SliceMeshData sliceMeshData = new SliceMeshData();
        for (int x = 0; x < Map.VoxelsInXZ; x++)
            for (int z = 0; z < Map.VoxelsInXZ; z++)
                sliceMeshData = Voxels[x, z].AddToSliceMeshData(this, x, z, sliceMeshData, meshType);

        return sliceMeshData;
    }

    // Content
    public Voxel[,] Voxels { get; private set; }
    public void SetVoxels(Voxel[,] voxels) { Voxels = voxels; }
    public Voxel GetVoxel(int x, int y, int z) // x, y, z are absolute positions.
    {
        int rX = x - X;   // Relative coordinates.
        int rY = y - Y;
        int rZ = z - Z;

        if (!InRange(this, rX, Axis.X) || !InRange(this, rY, Axis.Y) || !InRange(this, rZ, Axis.Z))
            return Map.GetVoxel(x, y, z); // Not here, Ask the Map to get it based on the absolute position.

        return Voxels[rX, rZ]; // Found it in this slice.
    }

    // Camera visibility based on gameObject.layer
    public void SetVisible()
    {
        SliceMeshSides.SetVisible();
        SliceMeshSides.SliceMeshTop.SetVisible();
        SliceMeshSides.SliceMeshLiquids.SetVisible();
        Map.SetSliceVisible(this);
    }
    public bool IsMeshVisible() { return SliceMeshSides.CurrentVisibility == SliceMesh.Visibility.Visible ? true : false; }
    public void SetGhosted()
    {
        SliceMeshSides.SetGhosted();
        SliceMeshSides.SliceMeshTop.SetGhosted();
        SliceMeshSides.SliceMeshLiquids.SetGhosted();
        Map.SetSliceGhosted(this);
    }
    public bool IsMeshGhosted() { return SliceMeshSides.CurrentVisibility == SliceMesh.Visibility.Ghosted ? true : false; }
    public void SetHidden()
    {
        SliceMeshSides.SetHidden();
        SliceMeshSides.SliceMeshTop.SetHidden();
        SliceMeshSides.SliceMeshLiquids.SetHidden();
        Map.SetSliceHidden(this);
    }
    public bool IsMeshHidden() { return SliceMeshSides.CurrentVisibility == SliceMesh.Visibility.Hidden ? true : false; }

    // Constructors
    public Slice(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    // Class Helpers
    public static bool InRange(Slice slice, int n, Axis axis) // n is absolute position
    {
        if (axis == Axis.Y)
        {
            if (n != 0)
                return false;
        }
        else if (axis == Axis.X || axis == Axis.Z)
        {
            if (n < 0 || n >= Map.VoxelsInXZ)
                return false;
        }

        return true;
    }
}
