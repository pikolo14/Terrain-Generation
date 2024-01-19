using Habrador_Computational_Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodePointsGenerator : MonoBehaviour
{
    public float DistanceRadius = 1;
    private MapGenerator _mapGenerator;
    private MapView _mapView;
    public GameObject NodePointPrefab;
    public float HeightOffset = 1;
    public List<GameObject> Nodes;

    public bool AutoResetSeed = true;
    private int _currentSeed;

    public MeshFilter DebugTrianglesMesh;


    [ExecuteAlways]
    public void GenerateNodePoints()
    {
        if (AutoResetSeed)
            _currentSeed = System.DateTime.Now.Millisecond;

        EraseAllNodePoints();

        Vector2 zoneSize = new Vector2(_mapGenerator.MapWidth, _mapGenerator.MapHeight);
        List<Vector2> nodePoints = PointsGeneration.GeneratePoissonDiscPoints(DistanceRadius, _mapView.transform.position, zoneSize, _currentSeed);
        Nodes = new List<GameObject>();

        float height = HeightOffset + _mapGenerator.transform.position.y;

        ////Colocamos los puntos a una altura fija formando un plano sobre el terreno
        //for (int i = 0; i < nodePoints.Count; i++)
        //{
        //    Vector2 point = nodePoints[i];
        //    GameObject nodeGO = Instantiate(NodePointPrefab, transform);
        //    nodeGO.transform.position = new Vector3(point.x, height, point.y);
        //    Nodes.Add(nodeGO);
        //}

        //Hacemos un Raycast sobre el terreno para obtener la ubicacion de los nodos pegados a la tierra
        //TODO: Mejorar método para que los puntos en los límites colisionen con en el terreno
        for (int i = 0; i < nodePoints.Count; i++)
        {
            var point = nodePoints[i];
            RaycastHit hit;

            if (Physics.Raycast(new Vector3(point.x, 100, point.y), Vector3.down, out hit))
            {
                GameObject nodeGO = Instantiate(NodePointPrefab, transform);
                nodeGO.name = "NodePoint " + i;
                nodeGO.transform.position = hit.point;
                Nodes.Add(nodeGO);
            }
        }

        //Obtenemos una malla con la triangulacion de Delaunay de Habrador
        GenerateDelaunayTriangulation(nodePoints);

        //TODO: Recorrer toda la malla, identificar todos los vertices con nodepoints y obtener las relaciones entre puntos con las aristas
        //TODO: Eliminar edges demasiado largos
    }

    public void GenerateDelaunayTriangulation(List<Vector2> points)
    {
        DebugTrianglesMesh.sharedMesh = DelaunayTriangulation.GetDelaunayTriangleMesh(points);
    }

    private void EraseAllNodePoints()
    {
        foreach (var node in Nodes)
        {
            if(node)
            {
                if (Application.isPlaying)
                    Destroy(node.gameObject);
                else
                    DestroyImmediate(node.gameObject);
            }
        }
    }

    private void OnValidate()
    {
        if (!_mapView)
            _mapView = FindObjectOfType<MapView>();
        if (!_mapGenerator)
            _mapGenerator = FindObjectOfType<MapGenerator>();
    }
}
