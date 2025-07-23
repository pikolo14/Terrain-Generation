using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct MeshData
{
    public Vector3[] Vertices;
    public int[] Triangles;
    public Vector2[] UVs;

    private int _currentTriangle;

    public MeshData(int quadsWidth, int quadsHeight)
    {
        Vertices = new Vector3[(quadsWidth+1) * (quadsHeight+1)];
        Triangles = new int[quadsWidth * quadsHeight * 6];
        UVs = new Vector2[(quadsWidth+1) * (quadsHeight+1)];
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
