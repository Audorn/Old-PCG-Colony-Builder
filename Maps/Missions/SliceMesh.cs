using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public abstract class SliceMesh : MonoBehaviour
{
    // Reference to the slice itself.
    public Slice Slice { get; protected set; }

    // Visualization
    public MeshFilter MeshFilter { get; private set; }
    public MeshCollider MeshCollider { get; private set; }
    public enum Visibility { Visible, Ghosted, Hidden }
    public Visibility CurrentVisibility
    {
        get
        {
            switch (gameObject.layer)
            {
                case 8:
                    return Visibility.Visible;
                case 9:
                    return Visibility.Ghosted;
                case 10:
                    return Visibility.Hidden;
            }
            Debug.Log("SliceMeshSides.gameObject is not on a valid layer regarding visibility!");
            return Visibility.Visible;
        }
    }
    public void SetVisible()
    {
        gameObject.layer = Map.VisibleLayer;
        MeshCollider.enabled = true;
    }
    public void SetGhosted()
    {
        gameObject.layer = Map.GhostedLayer;
        MeshCollider.enabled = false;
    }
    public void SetHidden()
    {
        gameObject.layer = Map.HiddenLayer;
        MeshCollider.enabled = false;
    }

    // Connect to Visualization
    protected void Initialize()
    {
        MeshFilter = GetComponent<MeshFilter>();
        MeshCollider = GetComponent<MeshCollider>();
    }

    // Visualize the mesh handed to it by Map
    public void RenderMesh(SliceMeshData meshData)
    {
        MeshFilter.mesh.Clear();
        MeshFilter.mesh.vertices = meshData.verts.ToArray();
        MeshFilter.mesh.triangles = meshData.tris.ToArray();
        MeshFilter.mesh.uv = meshData.uvs.ToArray();
        MeshFilter.mesh.RecalculateNormals();

        MeshCollider.sharedMesh = null;
        Mesh mesh = new Mesh();
        mesh.vertices = meshData.cVerts.ToArray();
        mesh.triangles = meshData.cTris.ToArray();
        mesh.RecalculateNormals();
        MeshCollider.sharedMesh = mesh;
    }

}
