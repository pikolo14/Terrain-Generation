using Habrador_Computational_Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodePointsGenerator : MonoBehaviour
{
    [Serializable]
    public struct NodePoint
    {
        public Vector2 Position2D;
        public GameObject GO;

        public NodePoint(Vector2 position2D, GameObject gO)
        {
            Position2D = position2D;
            GO = gO;
        }
    }

    [Serializable]
    public struct NodePath
    {
        public NodePoint P1, P2;
        public LineRenderer Line;
    }

    [Serialize]
    private List<NodePoint> _nodePoints;
    [Serialize]
    private List<NodePath> _nodePaths;

    public float DistanceRadius = 1;
    private MapGenerator _mapGenerator;
    private MapView _mapView;
    public GameObject NodePointPrefab;
    public GameObject NodePathPrefab;
    public float HeightOffset = 1;

    public bool AutoResetSeed = true;
    private int _currentSeed;

    public MeshFilter DebugTrianglesMesh;
    public float MaxEdgeProp = 0.6f;


    [ExecuteAlways]
    public void GenerateNodePointsAndPaths()
    {
        if (AutoResetSeed)
            _currentSeed = System.DateTime.Now.Millisecond;

        Vector2 zoneSize = new Vector2(_mapGenerator.MapWidth, _mapGenerator.MapHeight);
        GenerateNodePoints(zoneSize);
        GenerateNodePaths();
    }

    public void GenerateNodePoints(Vector2 zoneSize)
    {
        EraseAllNodePoints();

        _nodePoints = new List<NodePoint>();
        List<Vector2> points = PointsGeneration.GeneratePoissonDiscPoints(DistanceRadius, _mapView.transform.position, zoneSize, _currentSeed);

        foreach(var point in points)
        {
            //Hacemos un Raycast sobre el terreno para obtener la ubicacion de los nodos pegados a la tierra
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(point.x, 100, point.y), Vector3.down, out hit))
            {
                GameObject nodeGO = Instantiate(NodePointPrefab, transform);
                nodeGO.transform.position = hit.point;
                _nodePoints.Add(new NodePoint(point, nodeGO));
            }
        }
    }

    public void GenerateNodePaths()
    {
        RemoveAllNodePaths();

        List<Vector2> NodePositions2D = new List<Vector2>();

        foreach(var node in _nodePoints)
        {
            NodePositions2D.Add(node.Position2D);
        }

        HashSet<HalfEdge2> edges = DelaunayTriangulation.GetDelaunayMeshShorterEdges(NodePositions2D, MaxEdgeProp);

        foreach (var edge in edges)
        {
            NodePath path = new NodePath();
            path.P1 = GetPointInPosition(edge.v.position.ToVector2());
            path.P2 = GetPointInPosition(edge.prevEdge.v.position.ToVector2());

            Vector3 p1 = path.P1.GO.transform.position;
            Vector3 p2 = path.P2.GO.transform.position;
            var lineGO = Instantiate(NodePathPrefab, transform);
            path.Line = lineGO.GetComponent<LineRenderer>();
            path.Line.SetPositions(new Vector3[] { p1, p2 });
            //TODO: Evitar paths duplicados
            _nodePaths.Add(path);
        }
    }

    private NodePoint GetPointInPosition(Vector2 position)
    {
        foreach(var point in _nodePoints)
        {
            if (point.Position2D == position)
                return point;
        }

        return default(NodePoint);
    }

    private void EraseAllNodePoints()
    {
        if(_nodePoints != null)
        {
            foreach (var node in _nodePoints)
            {
                if(node.GO)
                {
                    if (Application.isPlaying)
                        Destroy(node.GO);
                    else
                        DestroyImmediate(node.GO);
                }
            }

            _nodePoints.Clear();
        }
    }

    private void RemoveAllNodePaths()
    {
        if (_nodePaths != null)
        {
            foreach (var path in _nodePaths)
            {
                if (path.Line)
                {
                    if (Application.isPlaying)
                        Destroy(path.Line.gameObject);
                    else
                        DestroyImmediate(path.Line.gameObject);
                }
            }
            _nodePaths.Clear();
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
