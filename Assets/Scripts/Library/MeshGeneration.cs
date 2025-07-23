using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGeneration
{
    public static List<TerrainChunk> GenerateTerrainChunks(in float[,] heightMap, Vector2Int chunkMaxQuadsSize, float heightMultiplier, AnimationCurve heightCurve, Transform chunksParent)
    {
        int width = heightMap.GetLength(0)-1;
        int height = heightMap.GetLength(1)-1;
        float midWidth = (heightMap.GetLength(0)) / 2f;
        float midHeight = (heightMap.GetLength(1)) / 2f;

        List<TerrainChunk> chunks = new List<TerrainChunk>();
        Vector2Int mapQuadsSize = new Vector2Int(width, height);
        Vector2Int chunkCellsSize = new Vector2Int(Mathf.CeilToInt((float)mapQuadsSize.x / chunkMaxQuadsSize.x), 
            Mathf.CeilToInt((float)mapQuadsSize.y / chunkMaxQuadsSize.y));

        for (int j = 0; j<chunkCellsSize.y; j++)
        {
            for(int i = 0; i<chunkCellsSize.x; i++)
            {
                Vector2Int chunkCell = new Vector2Int(i, j);
                Vector2Int chunkMapQuad = chunkMaxQuadsSize * chunkCell;
                Vector2Int chunkQuadsSize = (mapQuadsSize - chunkMapQuad);
                chunkQuadsSize.Clamp(Vector2Int.zero, chunkMaxQuadsSize);
                Vector3 chunkPosition = new Vector3(chunkMapQuad.x - midWidth, 0, chunkMapQuad.y - midHeight);

                chunks.Add(GenerateTerrainChunk(in heightMap, chunkPosition, chunkCell, chunkMapQuad, chunkQuadsSize, chunkMaxQuadsSize, heightMultiplier, heightCurve, chunksParent));
            }
        }

        return chunks;
    }

    private static TerrainChunk GenerateTerrainChunk(in float[,] fullHeightMap, Vector3 chunkPosition, Vector2Int chunkCell, Vector2Int chunkMapQuad, Vector2Int chunkQuadsSize, Vector2Int chunkMaxQuadsMaxSize, float heightMultiplier, AnimationCurve heightCurve, Transform parent)
    {
        int width = chunkQuadsSize.x;
        int height = chunkQuadsSize.y;

        //Generar malla con textura de cada chunk
        GameObject meshGO = new GameObject("Chunk " + chunkCell);
        meshGO.transform.parent = parent;
        TerrainChunk chunk = meshGO.AddComponent<TerrainChunk>();
        chunk.Initialize(chunkQuadsSize, chunkCell, chunkMapQuad);

        //Asignar los vértices de la malla y los id de vertices que forman cada triangulo
        for (int y = 0, vertexId = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++, vertexId++)
            {
                //Asignamos la posicion de los vertices segun las coordenadas X Y y el mapa de altura (lo ponemos en horizontal, siendo Y la altura ahora)
                chunk.Mesh.Vertices[vertexId] = new Vector3(x, heightCurve.Evaluate(fullHeightMap[chunkMapQuad.x + x, chunkMapQuad.y + y]) * heightMultiplier, y) + chunkPosition;
                //Asignamos las UVs de cada vector
                chunk.Mesh.UVs[vertexId] = new Vector2(x / (float)(width), y / (float)(height));

                //Para cada celda creamos 2 triangulos con los indices de vertices que forman el cuadrado
                if (y < height && x < width)
                {
                    int idDown = vertexId + width+1;
                    int idRight = vertexId + 1;
                    int idDownRight = idDown + 1;
                    chunk.Mesh.AddTriangle(vertexId, idDown, idDownRight);
                    chunk.Mesh.AddTriangle(vertexId, idDownRight, idRight);
                }
            }
        }

        chunk.UpdateMesh();

        return chunk;
    }
}