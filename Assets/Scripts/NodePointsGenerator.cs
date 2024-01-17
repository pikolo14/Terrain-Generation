using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePointsGenerator : MonoBehaviour
{
    public float DistanceRadius = 1;
    public MapGenerator Map;
    public GameObject NodePointPrefab;
    public float HeightOffset = 1;
    public GameObject[] Nodes;

    public bool AutoResetSeed = true;
    private int _currentSeed;


    [ExecuteAlways]
    public void GenerateNodePoints()
    {
        if (AutoResetSeed)
            _currentSeed = System.DateTime.Now.Millisecond;

        EraseAllNodePoints();

        Vector2 zoneSize = new Vector2(Map.MapWidth, Map.MapHeight);
        List<Vector2> nodePoints = PointsGeneration.GeneratePoissonDiscPoints(DistanceRadius, Map.transform.position, zoneSize, _currentSeed);
        Nodes = new GameObject[nodePoints.Count];
        float height = HeightOffset + Map.transform.position.y;

        for (int i = 0; i < nodePoints.Count; i++)
        {
            Vector2 point = nodePoints[i];
            GameObject nodeGO = Instantiate(NodePointPrefab, transform);
            nodeGO.transform.position = new Vector3(point.x, height, point.y);
            Nodes[i] = nodeGO;
        }
    }

    private void EraseAllNodePoints()
    {
        foreach (var node in Nodes)
        {
            if (Application.isPlaying)
                Destroy(node.gameObject);
            else
                DestroyImmediate(node.gameObject);
        }
    }
}
