using Habrador_Computational_Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public class NodePointsAndPathsGenerator : MonoBehaviour
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

        public static bool operator ==(NodePoint obj1, NodePoint obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;
            if (ReferenceEquals(obj1, null))
                return false;
            if (ReferenceEquals(obj2, null))
                return false;
            return obj1.Equals(obj2);
        }
        public static bool operator !=(NodePoint obj1, NodePoint obj2) => !(obj1 == obj2);
        public bool Equals(NodePoint other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return this.Position2D == other.Position2D;
        }
    }

    [Serializable]
    public struct NodePath
    {
        public NodePoint P1, P2;
        public LineRenderer Line;
    }

    public List<NodePoint> _nodePoints;
    public List<NodePath> _nodePaths;

    public GameObject NodePointPrefab;
    public GameObject NodePathPrefab;
    [Serialize]
    private MapGenerator _mapGenerator;
    [Serialize]
    private MapView _mapView;

    [Header("Generation params")]
    [Tooltip("Distancia mínima que debe de haber entre los puntos de nodo")]
    public float DistanceRadius = 1;
    [Tooltip("Rango de altura (entre 0 y 1) en los que se puede colocar un punto de nodo")]
    public Vector2 PointHeightRange = new Vector2(0.05f, 0.8f);
    [Tooltip("Cuantas veces más larga que la media puede ser una arista válida para formar un camino")]
    public float MaxEdgeProp = 0.6f;
    public bool AutoResetSeed = true;
    private int _currentSeed;


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
        RemoveAllNodePoints();

        _nodePoints = new List<NodePoint>();
        List<Vector2> points = PointsGeneration.GeneratePoissonDiscPoints(DistanceRadius, _mapView.transform.position, zoneSize, _currentSeed);

        foreach(var point in points)
        {
            //Hacemos un Raycast sobre el terreno para obtener la ubicacion de los nodos pegados a la tierra
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(point.x, 100, point.y), Vector3.down, out hit))
            {
                //Comprobamos que el punto generado esté en el rango deseado de altura
                if(IsCorrectHeight(hit.point.y))
                {
                    GameObject nodeGO = Instantiate(NodePointPrefab, transform);
                    nodeGO.transform.position = hit.point;
                    _nodePoints.Add(new NodePoint(point, nodeGO));
                }
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
            path.P1 = GetNodePointInPosition(edge.v.position.ToVector2());
            path.P2 = GetNodePointInPosition(edge.prevEdge.v.position.ToVector2());

            //Evitar paths duplicados
            if(!IsPathCreated(path.P1, path.P2))
            {
                Vector3 p1 = path.P1.GO.transform.position;
                Vector3 p2 = path.P2.GO.transform.position;
                var lineGO = Instantiate(NodePathPrefab, transform);
                path.Line = lineGO.GetComponent<LineRenderer>();
                path.Line.SetPositions(new Vector3[] { p1, p2 });
                _nodePaths.Add(path);
            }
        }
    }

    private bool IsCorrectHeight(float height)
    {
        return _mapGenerator.IsHeightInRange(height, PointHeightRange);
    }

    private bool IsPathCreated(NodePoint p1, NodePoint p2)
    {
        foreach(var path in _nodePaths)
        {
            if ((p1 == path.P1 || p1 == path.P2) && (p2 == path.P1 || p2 == path.P2))
                return true;
        }
        return false;
    }

    private NodePoint GetNodePointInPosition(Vector2 position)
    {
        foreach(var point in _nodePoints)
        {
            if (point.Position2D == position)
                return point;
        }

        return default(NodePoint);
    }

    private void RemoveAllNodePoints()
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
