using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SliceRenderMode { Slice, Adaptive, Full }
public class Map : MonoBehaviour
{
    // Singleton
    private static Map instance;
    public static Map Instance { get { return instance; } }
    private void Awake() { if (instance == null) instance = this; }

    public static bool InDebugMode { get; private set; }
    public static int VoxelsInXZ { get; private set; }
    public static int SlicesInXZ { get; private set; }
    public static int SlicesInY { get; private set; }
    public static bool DrawSliceBottoms { get; private set; }
    public static byte Throttle_Generation { get; private set; }
    public static byte Throttle_Rendering { get; private set; }
    public static float VoxelFloorHeight { get; private set; }
    public static int XZOffset { get; private set; }
    public static int YOffset { get; private set; }
    public static byte VisibleLayer { get; private set; }
    public static byte GhostedLayer { get; private set; }
    public static byte HiddenLayer { get; private set; }
    public static List<Slice> SlicesWaitingForReRender { get; private set; }
    public static List<Slice> VisibleSlices { get; private set; }
    public static List<Slice> GhostedSlices { get; private set; }
    public static SliceRenderMode SliceRenderMode { get; private set; }

    public GameObject sliceMeshPrefab;
    private string missionName = "debug";
    public Slice[,,] Slices { get; private set; }
    // Use this for initialization
    void Start()
    {
        InDebugMode = true;
        VoxelsInXZ = 32;
        SlicesInXZ = 6;
        SlicesInY = 100; // Keep a multiple of 2.
        XZOffset = SlicesInXZ / 2;
        YOffset = SlicesInY / 2;
        DrawSliceBottoms = false;
        Throttle_Generation = 25;
        Throttle_Rendering = 8;
        VoxelFloorHeight = 0.3f;
        VisibleLayer = 8;
        GhostedLayer = 9;
        HiddenLayer = 10;
        Slices = new Slice[SlicesInXZ, SlicesInY, SlicesInXZ];
        SlicesWaitingForReRender = new List<Slice>();
        VisibleSlices = new List<Slice>();
        GhostedSlices = new List<Slice>();
        SliceRenderMode = SliceRenderMode.Full;
    }

    public static void SetSliceRenderMode(SliceRenderMode sliceRenderMode) { SliceRenderMode = sliceRenderMode; }
    public static void QueueSliceForReRender(Slice slice)
    {
        if (SlicesWaitingForReRender.Contains(slice))
            return;

        SlicesWaitingForReRender.Add(slice);
    }
    public static void RemoveSliceFromReRenderQueue(Slice slice)
    {
        if (SlicesWaitingForReRender.Contains(slice))
            SlicesWaitingForReRender.Remove(slice);
    }
    public static void RebuildQueuedSlices()
    {
        // may need render throttle in here for performance.
        List<Slice> slicesRebuilt = new List<Slice>();
        foreach (var slice in SlicesWaitingForReRender)
        {
            if (slice.updateMeshLiquids)
                slice.BuildMesh(SliceMeshData.Type.Liquids);
            if (slice.updateMeshTop)
                slice.BuildMesh(SliceMeshData.Type.Top);
            if (slice.updateMeshSides)
                slice.BuildMesh(SliceMeshData.Type.Sides);

            slicesRebuilt.Add(slice);
        }

        // Remove the rebuilt slices from the rebuild queue.
        foreach (var slice in slicesRebuilt)
            SlicesWaitingForReRender.Remove(slice);
    }
    public static void RebuildQueuedSlicesOnY(int y)
    {
        // may need render throttle in here for performance.
        List<Slice> slicesRebuilt = new List<Slice>();
        foreach (var slice in SlicesWaitingForReRender)
        {
            if (slice.Y == y)
            {
                if (slice.updateMeshLiquids)
                    slice.BuildMesh(SliceMeshData.Type.Liquids);
                if (slice.updateMeshTop)
                    slice.BuildMesh(SliceMeshData.Type.Top);
                if (slice.updateMeshSides)
                    slice.BuildMesh(SliceMeshData.Type.Sides);

                slicesRebuilt.Add(slice);
            }
        }

        // Remove the rebuilt slices from the rebuild queue.
        foreach (var slice in slicesRebuilt)
            SlicesWaitingForReRender.Remove(slice);
    }
    public static void SetSliceVisible(Slice slice)
    {
        if (VisibleSlices.Contains(slice))
            return;

        VisibleSlices.Add(slice);
    }
    public static void SetSlicesOnYVisible(int y)
    {
        foreach (var slice in Instance.Slices)
        {
            if (slice.Y == y)
            {
                if (IsSliceVisible(slice))
                    continue;
                slice.SetVisible();
            }
        }
    }
    public static void SetSlicesOnAndBelowYVisible(int y)
    {
        foreach (var slice in Instance.Slices)
        {
            if (slice.Y <= y)
            {
                if (IsSliceVisible(slice))
                    continue;
                slice.SetVisible();
            }
        }
    }
    public static bool IsSliceVisible(Slice slice)
    {
        if (VisibleSlices.Contains(slice))
            return true;
        return false;
    }
    public static void SetSliceGhosted(Slice slice)
    {
        if (GhostedSlices.Contains(slice))
            return;

        GhostedSlices.Add(slice);
    }
    public static void SetSlicesOnYGhosted(int y)
    {
        foreach (var slice in Instance.Slices)
        {
            if (slice.Y == y)
            {
                if (IsSliceGhosted(slice))
                    continue;
                slice.SetGhosted();
            }
        }
    }
    public static bool IsSliceGhosted(Slice slice)
    {
        if (GhostedSlices.Contains(slice))
            return true;
        return false;
    }
    public static void SetSliceHidden(Slice slice)
    {
        if (VisibleSlices.Contains(slice))
            VisibleSlices.Remove(slice);
        if (GhostedSlices.Contains(slice))
            GhostedSlices.Remove(slice);
    }
    public static void SetSlicesOnYHidden(int y)
    {
        foreach (var slice in Instance.Slices)
        {
            if (slice.Y == y)
            {
                if (IsSliceHidden(slice))
                    continue;
                slice.SetHidden();
            }
        }
    }
    public static void SetSlicesAboveYHidden(int y)
    {
        foreach (var slice in Instance.Slices)
        {
            if (slice.Y > y)
            {
                if (IsSliceHidden(slice))
                    continue;
                slice.SetHidden();
            }
        }
    }
    public static bool IsSliceHidden(Slice slice)
    {
        if (!IsSliceVisible(slice) && !IsSliceGhosted(slice))
            return true;
        return false;
    }
    public static Voxel GetVoxelByWorldCoordinates(float x, float y, float z)
    {
        return null;
    }
    public static int FindDistanceToFirstNonTransparentVoxel(Direction direction, Voxel voxel)
    {
        int distance = 0;
        if (voxel == null)
        {
            Debug.Log("Voxel requested by FindDistanceToFirstNonTransparentVoxel() was null.");
            return distance;
        }

        // Chain through the voxels to find the first non-transparent one in the specified direction.
        while (voxel != null && voxel.GetNeighbor(direction) != null && voxel.GetNeighbor(direction).IsTransparent())
        {
            distance++;
            voxel = voxel.GetNeighbor(direction);
        }

        return distance;
    }
    public static Voxel GetVoxelCameraFocusCoordinates(float x, float y, float z)
    {
        return GetVoxel(Mathf.FloorToInt(x + XZOffset * VoxelsInXZ + 0.5f), (int)y + YOffset, Mathf.FloorToInt(z + XZOffset * VoxelsInXZ + 0.5f));
    }
    void Update()
    {
        // Rebuild and render a slice mesh if its visible or ghosted and needs it.
        if (mapCreated)
        {
            int throttle = 0;
            foreach (var slice in Slices)
            {
                if (throttle < Throttle_Rendering)
                {
                    if (slice.SliceMeshSides.CurrentVisibility != SliceMesh.Visibility.Hidden && slice.updateMeshSides)
                    {
                        slice.SliceMeshSides.RenderMesh(slice.BuildMesh(SliceMeshData.Type.Sides));
                        slice.updateMeshSides = false;
                        throttle++;
                    }
                    if (slice.SliceMeshSides.SliceMeshTop.CurrentVisibility != SliceMesh.Visibility.Hidden && slice.updateMeshTop)
                    {
                        slice.SliceMeshSides.SliceMeshTop.RenderMesh(slice.BuildMesh(SliceMeshData.Type.Top));
                        slice.updateMeshTop = false;
                        throttle++;
                    }
                    if (slice.SliceMeshSides.SliceMeshLiquids.CurrentVisibility != SliceMesh.Visibility.Hidden && slice.updateMeshLiquids)
                    {
                        slice.SliceMeshSides.SliceMeshLiquids.RenderMesh(slice.BuildMesh(SliceMeshData.Type.Liquids));
                        slice.updateMeshLiquids = false;
                        throttle++;
                    }
                }
                else break;
            }
        }
    }

    public static bool mapCreated = false;
    public static IEnumerator CreateMap()
    {
        int throttle = 0;
        for (int y = 0; y < SlicesInY; y++)
            for (int x = 0; x < SlicesInXZ; x++)
                for (int z = 0; z < SlicesInXZ; z++)
                {
                    Slice slice = Generator_Terrain.GenerateSlice(x, y, z);
                    Instance.Slices[x, y, z] = slice;

                    // Create the visualization offset from the array and connect it to the slice.
                    Instance.Slices[x, y, z].SetSliceMeshSides(Instance.CreateSliceMesh(slice, x * VoxelsInXZ, y, z * VoxelsInXZ));

                    // Track which slices are visible on map creation.
                    if (SliceRenderMode == SliceRenderMode.Full)
                        SetSliceVisible(slice);
                    else if (SliceRenderMode == SliceRenderMode.Adaptive && slice.Y - YOffset <= CameraMovementManager.CurrentYPosition)
                        SetSliceVisible(slice);
                    else if (SliceRenderMode == SliceRenderMode.Slice && slice.Y - YOffset == CameraMovementManager.CurrentYPosition)
                        SetSliceVisible(slice);

                    throttle++;
                    if (throttle >= Throttle_Generation)
                    {
                        throttle = 0;
                        yield return null;
                    }
                }

        mapCreated = true;

        yield return null; // wait for instantiation of all slice meshes before accessing any.
        foreach (var slice in Instance.Slices) // Hide the slices that are not visible.
        {
            if (IsSliceHidden(slice))
                slice.SetHidden();

            for (int x = 0; x < VoxelsInXZ; x++)
            {
                for (int z = 0; z < VoxelsInXZ; z++)
                {
                    Voxel v = slice.GetVoxel(slice.X + x, slice.Y + 1, slice.Z + z);
                    if (v == null || v.IsTransparent()) // A TEMPORARY IMPLEMENTATION OF GRASS - HACK MODE
                        slice.Voxels[x, z].SetUnobstructedAbove();
                }
            }
        }
    }
    private SliceMeshSides CreateSliceMesh(Slice slice, int x, int y, int z)
    {
        GameObject sliceMeshObject = Instantiate(sliceMeshPrefab, new Vector3(x, y, z), Quaternion.Euler(Vector3.zero), transform) as GameObject;
        sliceMeshObject.name = x.ToString() + ", " + y.ToString() + ", " + z.ToString();
        SliceMeshSides sliceMesh = sliceMeshObject.GetComponentInChildren<SliceMeshSides>();
        sliceMesh.SetSlice(slice);

        return sliceMesh;
    }

    public static Slice GetSlice(int x, int y, int z) // x, y, z are absolute positions.
    {
        //if (y != 0)
        //    Debug.Log("Map.GetSlice(" + x / VoxelsInXZ + ", " + y + ", " + z / VoxelsInXZ + ")");
        return Instance.Slices[x / VoxelsInXZ, y, z / VoxelsInXZ];
    }
    public static Voxel GetVoxel(int x, int y, int z) // x, y, z are absolute positions.
    {
        if (!InRange(x, Axis.X) || !InRange(y, Axis.Y) || !InRange(z, Axis.Z))
            return null; // Not within the map.

        return GetSlice(x, y, z).GetVoxel(x, y, z);
    }
    public static bool InRange(int n, Axis axis) // n is absolute positions.
    {
        if (n < 0) return false;

        if (axis == Axis.Y)
        {
            if (n >= SlicesInY)
                return false;
        }
        else if (axis == Axis.X || axis == Axis.Z)
        {
            if (n >= SlicesInXZ * VoxelsInXZ)
                return false;
        }

        return true;
    }

}
