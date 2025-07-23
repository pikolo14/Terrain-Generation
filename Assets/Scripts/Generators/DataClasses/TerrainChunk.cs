using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public Vector2Int Size;
    public Vector2Int Coordinates;
    public Vector2Int MapOrigin;
    public MeshData Mesh;
    public Texture Texture;
    private MeshRenderer meshRenderer;
    private MeshFilter filter;
    private new MeshCollider collider;


    public void Initialize(Vector2Int size, Vector2Int coordinates, Vector2Int mapOrigin)
    {
        Size = size;
        Coordinates = coordinates;
        MapOrigin = mapOrigin;
        Mesh = new MeshData(size.x, size.y);
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        filter = gameObject.AddComponent<MeshFilter>();
        collider = gameObject.AddComponent<MeshCollider>();
    }

    public void UpdateMesh()
    {
        filter.sharedMesh = Mesh.GetMesh();
        collider.sharedMesh = filter.sharedMesh;
    }

    public void UpdateMaterial()
    {
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
        meshRenderer.sharedMaterial.mainTexture = Texture;
    }
}