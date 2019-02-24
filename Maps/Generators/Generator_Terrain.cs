using UnityEngine;
using System.Collections;
using SimplexNoise;

public class Generator_Terrain : MonoBehaviour
{
    // Singleton
    private static Generator_Terrain instance;
    public static Generator_Terrain Instance { get { return instance; } }
    private void Awake() { if (instance == null) instance = this; }

    float stoneBaseHeight = -24;            // Starting point for stone.
    float stoneBaseNoise = 0.05f;           // Peaks are around 25 blocks apart.
    float stoneBaseNoiseHeight = 4;         // Max difference between peak and valley is 4 blocks.
    float stoneMountainHeight = 48;         // Sharper than stone.
    float stoneMountainFrequency = 0.008f;  // Sharper than stone.
    float stoneMinHeight = -12;             // The lowest that stone is allowed to go.
    float dirtBaseHeight = 1;               // A layer of dirt on the top.
    float dirtNoise = 0.04f;                // A little more messy than stone.
    float dirtNoiseHeight = 3;              // Max difference between peak and valley is 3 blocks.
    float caveFrequency = 0.025f;           // 
    int caveSize = 7;                       // 

    public static Slice GenerateSlice(int x, int y, int z)
    {
        Slice slice = new Slice(x * Map.VoxelsInXZ, y, z * Map.VoxelsInXZ);
        Voxel[,] voxels = new Voxel[Map.VoxelsInXZ, Map.VoxelsInXZ];
        for (int vX = 0; vX < Map.VoxelsInXZ; vX++)
            for (int vZ = 0; vZ < Map.VoxelsInXZ; vZ++)
            {
                voxels[vX, vZ] = GenerateVoxel(slice, slice.X + vX, slice.Z + vZ);
            }

        slice.SetVoxels(voxels);
        return slice;
    }

    private static Voxel GenerateVoxel(Slice slice, int x, int z)
    {
        byte constituentAmount = 10;
        // Stone
        int stoneHeight = Mathf.FloorToInt(Instance.stoneBaseHeight);
        stoneHeight += GetNoise(x, 0, z, Instance.stoneMountainFrequency, Mathf.FloorToInt(Instance.stoneMountainHeight));
        if (stoneHeight < Instance.stoneMinHeight)
            stoneHeight = Mathf.FloorToInt(Instance.stoneMinHeight);
        stoneHeight += GetNoise(x, 0, z, Instance.stoneBaseNoise, Mathf.FloorToInt(Instance.stoneBaseNoiseHeight));

        // Dirt
        int dirtHeight = stoneHeight + Mathf.FloorToInt(Instance.dirtBaseHeight);
        dirtHeight += GetNoise(x, 100, z, Instance.dirtNoise, Mathf.FloorToInt(instance.dirtNoiseHeight));

        int y = slice.Y - Map.YOffset;

        Voxel voxel = new Voxel(slice, x - slice.X, z - slice.Z);

        int caveChance = GetNoise(x, y, z, Instance.caveFrequency, 100);
        if (y <= stoneHeight && Instance.caveSize < caveChance)
            voxel.AddConstituent(GameData.GetConstituent("stone", "stone"), constituentAmount);
        else if (y <= dirtHeight && Instance.caveSize < caveChance)
            voxel.AddConstituent(GameData.GetConstituent("dirt", "dirt"), constituentAmount);

        return voxel;
    }

    private static int GetNoise(int x, int y, int z, float scale, int max)
    {
        return Mathf.FloorToInt((Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f));
    }

}
