using UnityEngine;
using System.Collections.Generic;

public class SliceMeshData
{
    public enum Type { Sides, Top, Liquids }

    public List<Vector3> verts = new List<Vector3>();
    public List<int> tris = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();

    public List<Vector3> cVerts = new List<Vector3>();
    public List<int> cTris = new List<int>();

    public bool useRenderDataForCollider;

    public void AddQuadTris()
    {
        tris.Add(verts.Count - 4);
        tris.Add(verts.Count - 3);
        tris.Add(verts.Count - 2);

        tris.Add(verts.Count - 4);
        tris.Add(verts.Count - 2);
        tris.Add(verts.Count - 1);

        if (useRenderDataForCollider)
        {
            cTris.Add(cVerts.Count - 4);
            cTris.Add(cVerts.Count - 3);
            cTris.Add(cVerts.Count - 2);

            cTris.Add(cVerts.Count - 4);
            cTris.Add(cVerts.Count - 2);
            cTris.Add(cVerts.Count - 1);
        }
    }
    public void AddTriangle(int tri)
    {
        tris.Add(tri);
        if (useRenderDataForCollider)
            cTris.Add(tri - (verts.Count - cVerts.Count));
    }
    public void AddVertex(Vector3 vertex)
    {
        verts.Add(vertex);
        if (useRenderDataForCollider)
            cVerts.Add(vertex);
    }

}
