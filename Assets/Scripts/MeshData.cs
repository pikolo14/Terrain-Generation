using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshData
{
    public Vector3[] Vertices;
    public int[] Triangles;
    public Vector2[] UVs;

    private int _currentTriangle;

    public MeshData(int width, int height)
    {
        Vertices = new Vector3[width * height];
        Triangles = new int[(width - 1) * (height - 1) * 6];
        UVs = new Vector2[width * height];
        _currentTriangle = 0;
    }

    public void AddTriangle(int a, int b, int c)
    {
        Triangles[_currentTriangle] = a;
        Triangles[_currentTriangle + 1] = b;
        Triangles[_currentTriangle + 2] = c;
        _currentTriangle += 3;
    }

    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
