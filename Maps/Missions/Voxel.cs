using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum Opacity { Opaque, Translucent, Transparent }
public enum Direction { Up, Down, North, South, East, West };

[Serializable]
public class Voxel
{
    public bool IsUnobstructedAbove { get; private set; }
    public void SetUnobstructedAbove() { IsUnobstructedAbove = true; }
    public Slice Slice { get; private set; }
    public int X { get; private set; } // Relative coordinates.
    public int Y { get; private set; }
    public int Z { get; private set; }

    public const byte MaxAmount = 30; // Bitshifting, 1 == 0 units, MaxAmount == 30 units.
    public byte SumOfConstituents { get; private set; }
    public void SumConstituents()
    {
        SumOfConstituents = 0;

        foreach (var constituent in Constituents)
        {
            SumOfConstituents += constituent.Amount;
        }
    }

    // Constructors
    public Voxel(Slice slice, int x, int z) // Relative coordinates.
    {
        Slice = slice;
        X = x;
        Z = z;
        Constituents = new List<Constituent>();
        IsUnobstructedAbove = false;
    }

    // ----- Contents
    public List<Constituent> Constituents { get; private set; }
    public void AddConstituent(ConstituentState constituentState, byte amount = 1)
    {
        Constituents.Insert(0, new Constituent(constituentState, amount));
        //return Constituents[0].AddAmount(amount);
    }
    public Constituent GetPrimaryConstituent()
    {
        Constituent primaryConstituent = null;
        foreach (var constituent in Constituents)
        {
            if (primaryConstituent == null || constituent.Amount > primaryConstituent.Amount)
                primaryConstituent = constituent;
        }
        return primaryConstituent;
    }

    public bool IsTransparent()
    {
        SumConstituents();

        // No contents.  For now, this means air.
        if (SumOfConstituents == 0) return true;

        byte transparent = 0;
        byte translucent = 0;
        foreach (var constituent in Constituents)
        {
            if (constituent.Opacity == Opacity.Transparent)
                transparent += constituent.Amount;
            if (constituent.Opacity == Opacity.Translucent)
                translucent += constituent.Amount;
        }

        // All contents are transparent.
        if (transparent == SumOfConstituents) return true;

        // All contents are transparent or translucent, most are transparent.
        if (transparent + translucent == SumOfConstituents && transparent > translucent) return true;

        // Something is opaque.
        return false;
    }

    public Voxel GetNeighbor(Direction direction) // Returns immediate neighbor in any cardinal direction.
    {
        if (direction == Direction.Up)
        {
            Voxel v = Slice.GetVoxel(Slice.X + X, Slice.Y + 1, Slice.Z + Z);
            //Debug.Log("Voxel above XYZ: " + (v.Slice.X + v.X) + ", " + v.Slice.Y + ", " + (v.Slice.Z + v.Z));
            return Slice.GetVoxel(Slice.X + X, Slice.Y + 1, Slice.Z + Z);
        }
        if (direction == Direction.Down)
            return Slice.GetVoxel(Slice.X + X, Slice.Y - 1, Slice.Z + Z);
        if (direction == Direction.North)
            return Slice.GetVoxel(Slice.X + X, Slice.Y, Slice.Z + Z + 1);
        if (direction == Direction.South)
            return Slice.GetVoxel(Slice.X + X, Slice.Y, Slice.Z + Z - 1);
        if (direction == Direction.East)
            return Slice.GetVoxel(Slice.X + X + 1, Slice.Y, Slice.Z + Z);
        if (direction == Direction.West)
            return Slice.GetVoxel(Slice.X + X - 1, Slice.Y, Slice.Z + Z);

        return null;
    }
    public SliceMeshData AddToSliceMeshData(Slice slice, int x, int z, SliceMeshData sliceMeshData, SliceMeshData.Type meshType, bool colliderToo = true)
    {
        if (IsTransparent()) return sliceMeshData; // Transparent, don't draw faces.  Later modify for gas/liquid.

        sliceMeshData.useRenderDataForCollider = colliderToo;
        float y = slice.Y * Map.VoxelFloorHeight;
        Voxel neighborVoxel = null;

        if (meshType == SliceMeshData.Type.Sides)
        {
            neighborVoxel = slice.GetVoxel(slice.X + x, slice.Y, slice.Z + z + 1);
            if (neighborVoxel == null || neighborVoxel.IsTransparent())
                sliceMeshData = FaceDataNorth(slice, x, y, z, sliceMeshData);

            neighborVoxel = slice.GetVoxel(slice.X + x, slice.Y, slice.Z + z - 1);
            if (neighborVoxel == null || neighborVoxel.IsTransparent())                     
                sliceMeshData = FaceDataSouth(slice, x, y, z, sliceMeshData);

            neighborVoxel = slice.GetVoxel(slice.X + x + 1, slice.Y, slice.Z + z);
            if (neighborVoxel == null || neighborVoxel.IsTransparent())                     
                sliceMeshData = FaceDataEast(slice, x, y, z, sliceMeshData);

            neighborVoxel = slice.GetVoxel(slice.X + x - 1, slice.Y, slice.Z + z);
            if (neighborVoxel == null || neighborVoxel.IsTransparent())                     
                sliceMeshData = FaceDataWest(slice, x, y, z, sliceMeshData);

            if (Map.DrawSliceBottoms) // Default: false - never seen by player.
            {
                neighborVoxel = slice.GetVoxel(slice.X + x, slice.Y - 1, slice.Z + z);
                if (neighborVoxel == null || neighborVoxel.IsTransparent())                 
                    sliceMeshData = FaceDataDown(slice, x, y, z, sliceMeshData);
            }
        }
        if (meshType == SliceMeshData.Type.Top)
        {
            neighborVoxel = slice.GetVoxel(slice.X + x, slice.Y + 1, slice.Z + z);
            if (neighborVoxel == null || neighborVoxel.IsTransparent() || Map.InDebugMode)
            {
                sliceMeshData = FaceDataUp(slice, x, y, z, sliceMeshData);
            }
            else if (neighborVoxel != null && !neighborVoxel.IsTransparent())
            {
                sliceMeshData = FaceDataUp(slice, x, y, z, sliceMeshData, true);
            }
        }
        if (meshType == SliceMeshData.Type.Liquids)
        {
            // Not much to do here yet...
        }

        return sliceMeshData;
    }




    // ----- Rendering
    private SliceMeshData FaceDataUp(Slice slice, int x, float y, int z, SliceMeshData meshData, bool IsUndiscovered = false)
    {
        int sliceXOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        int sliceYOffset = -Map.YOffset; 
        int sliceZOffset = -(Map.XZOffset * Map.VoxelsInXZ);

        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddQuadTris();
        meshData.uvs.AddRange(FaceUVs(IsUndiscovered, true)); // true == IsTop
        return meshData;
    }
    private SliceMeshData FaceDataDown(Slice slice, int x, float y, int z, SliceMeshData meshData)
    {
        int sliceXOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        int sliceYOffset = -Map.YOffset;
        int sliceZOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddQuadTris();
        meshData.uvs.AddRange(FaceUVs());
        return meshData;

    }
    private SliceMeshData FaceDataNorth(Slice slice, int x, float y, int z, SliceMeshData meshData)
    {
        int sliceXOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        int sliceYOffset = -Map.YOffset;
        int sliceZOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddQuadTris();
        meshData.uvs.AddRange(FaceUVs());
        return meshData;
    }
    private SliceMeshData FaceDataSouth(Slice slice, int x, float y, int z, SliceMeshData meshData)
    {
        int sliceXOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        int sliceYOffset = -Map.YOffset;
        int sliceZOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddQuadTris();
        meshData.uvs.AddRange(FaceUVs());
        return meshData;
    }
    private SliceMeshData FaceDataEast(Slice slice, int x, float y, int z, SliceMeshData meshData)
    {
        int sliceXOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        int sliceYOffset = -Map.YOffset;
        int sliceZOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x + 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddQuadTris();
        meshData.uvs.AddRange(FaceUVs());
        return meshData;
    }
    private SliceMeshData FaceDataWest(Slice slice, int x, float y, int z, SliceMeshData meshData)
    {
        int sliceXOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        int sliceYOffset = -Map.YOffset;
        int sliceZOffset = -(Map.XZOffset * Map.VoxelsInXZ);
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z + 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y + 0.5f + Map.VoxelFloorHeight + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddVertex(new Vector3(x - 0.5f + sliceXOffset, y - 0.5f + sliceYOffset, z - 0.5f + sliceZOffset));
        meshData.AddQuadTris();
        meshData.uvs.AddRange(FaceUVs());
        return meshData;
    }

    private Vector2[] FaceUVs(bool IsUndiscovered = false, bool IsTop = false)
    {
        Vector2[] uvs = new Vector2[4];
        Tile tilePos = BaseTexturePosition(IsUndiscovered, IsTop);
        float tileSize = 0.25f;

        uvs[0] = new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y);
        uvs[1] = new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y + tileSize);
        uvs[2] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y + tileSize);
        uvs[3] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y);
        return uvs;
    }

    private Tile BaseTexturePosition(bool IsUndiscovered = false, bool IsTop = false)
    {
        if (!Map.InDebugMode && IsUndiscovered)
            return TextureData.tiles["undiscovered"];

        Constituent primaryConstituent = GetPrimaryConstituent();
        if (primaryConstituent == null)
        {
            Debug.Log("Empty voxel attempted to draw textures.");
            return TextureData.tiles["undiscovered"]; // Create a special texture for this - maybe magenta?  Call it error.
        }
        
        // TEMPORARY CHECK TO SEE IF THIS IS THE HIGHEST TILE AND IS DIRT, IF SO, MAKE GRASS.
        if (IsUnobstructedAbove && primaryConstituent.ConstituentState.Name == "dirt")
        {
            if (IsTop)
            {
                Tile tile = null;
                TextureData.tiles.TryGetValue("grass-top", out tile);
                if (tile != null)
                    return tile;
            }

            return TextureData.tiles["grass"];
        }

        if (IsTop) // Return the top tile for this constituent.
        {
            Tile tile = null;
            TextureData.tiles.TryGetValue(primaryConstituent.ConstituentState.Name + "-top", out tile);
            if (tile != null)
                return tile;
        }

        // Discovered, has constituent(s), is not the top.
        return TextureData.tiles[primaryConstituent.ConstituentState.Name];
    }
    // -----
}
