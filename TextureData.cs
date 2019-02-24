using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile
{
    public int x, y;
    public Tile(int x, int y) { this.x = x; this.y = y; }
}

public static class TextureData
{
    public static Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();
    public static void LoadTextures(/*filename here*/)
    {
        AddTile("stone", 0, 0);
        AddTile("grass-top", 2, 0);
        AddTile("grass", 3, 0);
        AddTile("dirt", 1, 0);
        AddTile("undiscovered", 3, 1);
    }

    private static void AddTile(string name, int x, int y)
    {
        Tile tile = new Tile(x, y);
        // Check to see if the name already exists, don't add if it does - throw exception instead.
        tiles.Add(name, tile);
    }
}
