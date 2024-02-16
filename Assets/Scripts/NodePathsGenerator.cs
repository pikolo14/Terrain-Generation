using Habrador_Computational_Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controla la generacion de caminos a lo largo del terreno. Ofrece varios métodos de generación, todos basados en:
///     1º Colocación de puntos sobre el terreno (node points)
///     2º Trazado de caminos entre los puntos (node paths)
/// </summary>
public class NodePathsGenerator : MonoBehaviour
{
    public enum PathDrawStyle
    {
        StraightLine,
        RandomCurve,
        TangentContinuousCurveSimple,
        TangentContinuousCurveSectioned
    }

    public PathDrawStyle PathStyle = PathDrawStyle.StraightLine;
    public List<NodePoint> NodePoints;
    public List<NodePath> NodePaths;

    public GameObject NodePointPrefab;
    public GameObject NodePathPrefab;

    [Header("Generation params")]
    [Tooltip("Distancia mínima que debe de haber entre los puntos de nodo")]
    public float DistanceRadius = 1;
    [Tooltip("Rango de altura (entre 0 y 1) en los que se puede colocar un punto de nodo")]
    public Vector2 PointHeightRange = new Vector2(0.05f, 0.8f);
    [Tooltip("Cuantas veces más larga que la media puede ser una arista válida para formar un camino")]
    public float MaxEdgeProp = 0.6f;
    [Tooltip("Lejanía máxima de los puntos modificadores de la curva respecto a los puntos de extremo")]
    public float MaxRandomCurveRadius = 1;
    [Tooltip("Cercanía mínima de los puntos modificadores de la curva respecto a los puntos de extremo")]
    public float MinRandomCurveRadius = 0;
    [Tooltip("Número de segmentos en los que se va a dividir cada curva contenida de los paths")]
    public int PathCurveSubdivisions = 10;

    [Header("Sections drawing")]
    [Tooltip("Número de segmentos en los que se va a dividir cada subsección de nivel más bajo de recursividad de los paths (solo para dibujado de curvas con secciones)")]
    public int RecursiveSubsectionSubdivisions = 5;
    [Tooltip("Número de segmentos en los que se subdividira cada segmento")]
    public int SectionsPerLevel = 3;
    [Tooltip("Número de veces que se subdividira cada seccion dentro de una seccion. Si la recursividad es 2 por ejemplo se tendran secciones de secciones de la seccion principal")]
    public int RecursivityLevels = 1;
    public float MaxAngleSubsectionVariation = 120f;
    public float CustomSubsectionsTangentMultiplier = 2f;

    [Header("Continuous generation")]
    [Tooltip("Direccion preferida as la que se dirigirán los caminos")]
    public Vector2 PreferredPathDirection;
    [Tooltip("Variación máxima de ángulo respecto a la direccion preferida")]
    public float RandomTangentVariation = 15f;

    [Tooltip("Regenerar semilla al generar SOLO paths")]
    public bool AutoResetSeed = true;
    private int _currentSeed;


    public void GenerateNodePointsAndPaths(Vector2Int mapSize, Vector3 mapOrigin, float heightMultiplier, int seed = int.MinValue)
    {
        //Preparar random seed en funcion de atributos
        if(seed != int.MinValue)
            _currentSeed = seed;
        else if (AutoResetSeed)
            _currentSeed = DateTime.Now.Millisecond;
        UnityEngine.Random.InitState(_currentSeed);

        GenerateNodePoints(mapSize, mapOrigin, heightMultiplier);
        GenerateNodePathsConnections();

        if(PathStyle == PathDrawStyle.TangentContinuousCurveSimple || PathStyle == PathDrawStyle.TangentContinuousCurveSectioned)
        {
            PrepareTangentContinuousCurvePaths();

            if(PathStyle == PathDrawStyle.TangentContinuousCurveSectioned)
                GenerateNodePathsSections(SectionsPerLevel, RecursivityLevels);
        }

        DrawPathsCurves();
    }


    #region NODE POINTS GENERATION

    private void GenerateNodePoints(Vector2 zoneSize, Vector3 mapOrigin, float heightMultiplier)
    {
        RemoveAllNodePoints();

        NodePoints = new List<NodePoint>();
        List<Vector2> points = PointsGeneration.GeneratePoissonDiscPoints(DistanceRadius,mapOrigin, zoneSize, _currentSeed);

        foreach(var point in points)
        {
            //Hacemos un Raycast sobre el terreno para obtener la ubicacion de los nodos pegados a la tierra
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(point.x, 100, point.y), Vector3.down, out hit))
            {
                //Comprobamos que el punto generado esté en el rango deseado de altura
                if(IsHeightInRange(hit.point.y, PointHeightRange, mapOrigin, heightMultiplier))
                {
                    GameObject nodeGO = Instantiate(NodePointPrefab, transform);
                    nodeGO.transform.position = hit.point;
                    NodePoints.Add(new NodePoint(point, nodeGO));
                }
            }
        }
    }

    public static bool IsHeightInRange(float realHeight, Vector2 heightRangeProp, Vector2 mapOrigin, float heightMultiplier)
    {
        Vector2 realHeightRange = heightRangeProp * heightMultiplier + new Vector2(mapOrigin.y, mapOrigin.y);
        return (realHeight > realHeightRange.x && realHeight < realHeightRange.y);
    }

    #endregion


    #region PATHS GENERATION

    /// <summary>
    /// Genera la conexión de los puntos previamente creados mediante la triangulación de Delaunay. Sólo crea los caminos, no traza ni dibuja las curvas del camino.
    /// </summary>
    private void GenerateNodePathsConnections()
    {
        RemoveAllNodePaths();

        //Cancelamos generacion de caminos si no hay puntos de nodo suficientes
        if(NodePoints == null || NodePoints.Count < 2)
        {
            int count = 0;
            if(NodePoints!=null)
                count = NodePoints.Count;

            Debug.LogError("No hay suficientes puntos para hacer caminos ("+count+")");
        }

        //Obtener una malla con la triangulacion de Delaunay con los puntos generados previamente para los nodos
        List<Vector2> NodePositions2D = new List<Vector2>();
        NodePaths = new List<NodePath>();

        foreach(var node in NodePoints)
        {
            NodePositions2D.Add(node.Position2D);
        }

        HashSet<HalfEdge2> edges = DelaunayTriangulation.GetDelaunayMeshShorterEdges(NodePositions2D, MaxEdgeProp);

        //Obtener paths a raiz de las aristas de la malla de Delaunay
        int pathCount = 0;
        foreach (var edge in edges)
        {
            var lineGO = Instantiate(NodePathPrefab, transform);
            NodePath path = lineGO.AddComponent<NodePath>();
            path.P1 = GetNodePointInPosition(edge.v.position.ToVector2());
            path.P2 = GetNodePointInPosition(edge.prevEdge.v.position.ToVector2());

            //Evitar paths duplicados
            if(path.P1 != null && path.P2 != null && !IsPathCreated(path.P1, path.P2))
            {
                //Almacenar en cada punto los paths a los que pertenece
                path.P1.Paths.Add(path);
                path.P2.Paths.Add(path);

                //Preparar line renderer
                path.Line = lineGO.GetComponent<LineRenderer>();
                NodePaths.Add(path);

                //Numerar paths
                lineGO.name = "Path " + pathCount;
                pathCount++;
            }
            else
            {
                if (Application.isPlaying)
                    Destroy(lineGO);
                else
                    DestroyImmediate(lineGO);
            }
        }        
    }

    /// <summary>
    /// Subdivide la curva principal de cada path en secciones mas pequeñas
    /// </summary>
    private void GenerateNodePathsSections(int sectionsPerLevel, int recursivityLevels)
    {
        foreach(var path in NodePaths)
        {
            path.DoRecursiveSubsection(sectionsPerLevel, recursivityLevels, MaxAngleSubsectionVariation, CustomSubsectionsTangentMultiplier);
        }
    }

    /// <summary>
    /// Dibuja el recorrido de cada path (previamente generados durante la conexion de nodos) siguiendo el estilo de dibujo especificado
    /// </summary>
    private void DrawPathsCurves()
    {
        switch(PathStyle)
        {
            case PathDrawStyle.StraightLine:
                foreach (var path in NodePaths)
                {
                    path.DrawStaightLine();
                }
                break;

            case PathDrawStyle.RandomCurve:
                foreach (var path in NodePaths)
                {
                    path.DrawRandomCurve(MaxRandomCurveRadius, PathCurveSubdivisions);
                }
                break;

            case PathDrawStyle.TangentContinuousCurveSimple:
                DrawTangentContinuousCurvePathsSimply(PathCurveSubdivisions);
                break;

            case PathDrawStyle.TangentContinuousCurveSectioned:
                DrawTangentContinuousCurvePathsWithSections(RecursiveSubsectionSubdivisions);
                break;
        }
    }


    private void PrepareTangentContinuousCurvePaths()
    {
        //Colocamos inicialmente los modificadores de curva en el mismo punto que el extremo (inicialmente los paths son líneas rectas)
        for (int i = 0; i < NodePaths.Count; i++)
        {
            NodePath path = NodePaths[i];
            path.M1 = path.P1.Position2D;
            path.M2 = path.P2.Position2D;
        }

        //Preparamos los puntos modificadores
        Vector2 randomCurveRange = new Vector2(MinRandomCurveRadius, MaxRandomCurveRadius);
        for (int i = 0; i < NodePaths.Count; i++)
        {
            NodePath path = NodePaths[i];
            path.PrepareDrawingOppositeContinuousCurve(PreferredPathDirection, RandomTangentVariation, randomCurveRange);
        }
    }

    /// <summary>
    /// Dibuja los paths como curvas intentando continuar la tangente de los paths colocados justo en frente en el mismo nodo.
    /// </summary>
    private void DrawTangentContinuousCurvePathsSimply(int subdivisions)
    {
        //Dibujamos los caminos con bezier siguiendo los puntos preparados
        foreach (var path in NodePaths)
        {
            path.DrawPreparedCurveSimply(subdivisions);
        }
    }

    /// <summary>
    /// Dibuja los paths como curvas intentando continuar la tangente de los paths colocados justo en frente en el mismo nodo.
    /// </summary>
    private void DrawTangentContinuousCurvePathsWithSections(int subdivisions)
    {
        //Dibujamos los caminos con bezier siguiendo los puntos preparados
        foreach (var path in NodePaths)
        {
            path.DrawPreparedCurveWithSections(subdivisions);
        }
    }

    #endregion


    #region GENERATION UTILS

    private bool IsPathCreated(NodePoint p1, NodePoint p2)
    {
        foreach(var path in NodePaths)
        {
            if ((p1 == path.P1 || p1 == path.P2) && (p2 == path.P1 || p2 == path.P2))
                return true;
        }
        return false;
    }

    private NodePoint GetNodePointInPosition(Vector2 position)
    {
        foreach(var point in NodePoints)
        {
            if (point.Position2D == position)
                return point;
        }

        return null;
    }

    #endregion


    #region REMOVING NODES AND PATHS

    public void RemoveAll()
    {
        RemoveAllNodePaths();
        RemoveAllNodePoints();
    }

    private void RemoveAllNodePoints()
    {
        if(NodePoints != null)
        {
            foreach (var node in NodePoints)
            {
                if(node.GO)
                {
                    if (Application.isPlaying)
                        Destroy(node.GO);
                    else
                        DestroyImmediate(node.GO);
                }
            }

            NodePoints.Clear();
        }
    }

    private void RemoveAllNodePaths()
    {
        if (NodePaths != null)
        {
            foreach (var path in NodePaths)
            {
                if (path.Line)
                {
                    if (Application.isPlaying)
                        Destroy(path.Line.gameObject);
                    else
                        DestroyImmediate(path.Line.gameObject);
                }
            }
            NodePaths.Clear();
        }
    }

    #endregion
}
