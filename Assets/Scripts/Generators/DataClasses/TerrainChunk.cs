using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public Vector2Int Size;
    public Vector2Int Coordinates;
    public Vector2Int MapOrigin;
    public MeshData Mesh;
    public Texture Texture;

    public TerrainChunk(Vector2Int size, Vector2Int coordinates, Vector2Int mapOrigin)
    {
        Size = size;
        Coordinates = coordinates;
        MapOrigin = mapOrigin;
        Mesh = new MeshData(size.x, size.y);
    }
}