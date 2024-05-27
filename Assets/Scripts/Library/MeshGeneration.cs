using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGeneration
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        return GenerateTerrainMesh(heightMap, heightMultiplier, heightCurve);
    }

    public static MeshData GenerateTerrainChunk(ref float[,] fullHeightMap, Vector2Int chunkOrigin, Vector2Int chunkSize, float heightMultiplier, AnimationCurve heightCurve)
    {
        int width = fullHeightMap.GetLength(0);
        int height = fullHeightMap.GetLength(1);
        float midWidth = (width - 1) / 2f;
        float midHeight = (height - 1) / 2f;

        MeshData meshData = new MeshData(width, height);

        //Asignar los vértices de la malla y los id de vertices que forman cada triangulo
        for (int y = 0, vertexId = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++, vertexId++)
            {
                //Asignamos la posicion de los vertices segun las coordenadas X Y y el mapa de altura (lo ponemos en horizontal, siendo Y la altura ahora)
                meshData.Vertices[vertexId] = new Vector3(x - midWidth, heightCurve.Evaluate(fullHeightMap[x, y]) * heightMultiplier, y - midHeight);
                //Asignamos las UVs de cada vector
                meshData.UVs[vertexId] = new Vector2(x / (float)(width - 1), y / (float)(height - 1));

                //Para cada celda creamos 2 triangulos con los indices de vertices que forman el cuadrado
                if (y < height - 1 && x < width - 1)
                {
                    int idDown = vertexId + width;
                    int idRight = vertexId + 1;
                    int idDownRight = idDown + 1;
                    meshData.AddTriangle(vertexId, idDown, idDownRight);
                    meshData.AddTriangle(vertexId, idDownRight, idRight);
                }
            }
        }

        return meshData;
    }


    public class TerrainChunk
    {
        public Vector2Int Size;
        public Vector2Int Origin;
        public MeshData Mesh;


        public TerrainChunk(Vector2Int size, Vector2Int origin, MeshData mesh)
        {
            Size = size;
            Origin = origin;
            Mesh = mesh;
        }
    }
}