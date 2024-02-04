using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCarving : MonoBehaviour
{
    [Tooltip("Marca lo que influye el carving en el cambio de altura en función de la distancia al punto. Sus valores deben oscilar entre 0 y 1")]
    public AnimationCurve CarvingInfluence;
    public float MaxCarvingDistance = 1;

    private void Start()
    {

    }

    /// <summary>
    /// Aplicar el carving en funcion del trazado de las curvas de paths y la curva de influencia del carving
    /// </summary>
    /// <param name="terrainMesh">Malla de puntos que forma el terreno</param>
    /// <param name="terrainSize">Dimensiones del terreno en vertices de largo y ancho</param>
    /// <param name="paths">Lista completa de caminos</param>
    public void CarvePaths(ref MeshData terrainMesh, Vector2Int terrainSize, List<NodePath> paths)
    {
        Vector3[] pathPointsLUT = GeneratePathsLookUpTable(paths);

        for (int y = 0; y < terrainSize.y; y++)
        {
            for (int x = 0; x < terrainSize.x; x++)
            {
                int currentIndex = x + y * terrainSize.x;
                Vector3 currentPosition = terrainMesh.Vertices[currentIndex];

                float closestDistance;
                Vector3 closest = GetClosestPoint(pathPointsLUT, currentPosition, out closestDistance);
                float distProp = closestDistance/MaxCarvingDistance;
                if(distProp<=1f && distProp>0)
                {
                    float influence = CarvingInfluence.Evaluate(1-distProp);
                    float newHeight = Mathf.Lerp(currentPosition.y, closest.y, influence);
                    terrainMesh.Vertices[currentIndex].y = newHeight;
                }
            }
        }
    }

    /// <summary>
    /// Genera un array con todos los puntos discretos que definenen las curvas y que ya se han generado previamente.
    /// De esta manera se evita tener que calcular el punto mas cercano a la curva continuamente y simplemente se tiene que buscar cual de estos puntos es el mas cercano.
    /// Se excluirá el ultimo punto de segmento de la curva para evitar duplicados en la tabla y aumentar la eficiencia
    /// </summary>
    /// <param name="paths">Lista de todos los paths</param>
    /// <param name="pathSegments">Número de segmentos en en los que se subdivide cada curva de path (los usados en el line renderer)</param>
    /// <returns></returns>
    public Vector3[] GeneratePathsLookUpTable(List<NodePath> paths)
    {
        int subdivisions = paths[0].Line.positionCount-1;
        Vector3[] pathPoints = new Vector3[paths.Count*subdivisions];

        for (int i = 0; i < paths.Count; i++)
        {
            NodePath path = paths[i];
            Vector3[] linePoints = new Vector3[path.Line.positionCount];
            path.Line.GetPositions(linePoints);

            for(int s = 0; s < subdivisions; s++)
            {
                Vector3 point = linePoints[s];
                pathPoints[i * subdivisions + s] = point;
            }
        }

        return pathPoints;
    }

    /// <summary>
    /// Devuelve el punto más cercano de la lista a la posicion indicada, así como la distancia a ese punto
    /// </summary>
    /// <param name="points">Lista de posibles candidatos a ser el puinto mas cercano al origen</param>
    /// <param name="originPosition">Punto de referencia sobre el que se calculara la menor distancia</param>
    /// <param name="closestDistance">Distancia al punto mas cercano</param>
    /// <returns></returns>
    public Vector3 GetClosestPoint(Vector3[] points, Vector3 originPosition, out float closestDistance)
    {
        closestDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.negativeInfinity;
        Vector2 currentPosition2D = originPosition.GetXZ();

        foreach (var point in points)
        {
            float currentDistance = Vector2.Distance(currentPosition2D, point.GetXZ());
            if(currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }
}