using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PathCarving : MonoBehaviour
{
    public Vector2Int HashDimensions = new Vector2Int(10,10);
    private Vector3[] _pathPointsLUT;
    private List<int>[,] _hashTable;
    private Vector2 _cellSize;

   [Tooltip("Marca lo que influye el carving en el cambio de altura en función de la distancia al punto. Sus valores deben oscilar entre 0 y 1")]
    public AnimationCurve CarvingInfluence;
    public float MaxCarvingDistance = 1;

    //Spatial hash constants
    private const float PI2 = 2 * Mathf.PI;
    private const int RadialCellCheckSteps = 8;
    private const float RadialStepIncrement = PI2 / RadialCellCheckSteps;
    private const float RadialCheckRadius = 1f;


    private void Start() { }

    /// <summary>
    /// Aplicar el carving en funcion del trazado de las curvas de paths y la curva de influencia del carving
    /// </summary>
    /// <param name="terrainMesh">Malla de puntos que forma el terreno</param>
    /// <param name="terrainSize">Dimensiones del terreno en vertices de largo y ancho</param>
    /// <param name="paths">Lista completa de caminos</param>
    public void CarvePaths(ref MeshData terrainMesh, Vector2Int terrainSize, List<NodePath> paths)
    {
        Vector3 terrainOffset = terrainSize.ToVector3_XZ() / 2f;

        GeneratePathsLookUpTable(paths);
        GenerateHashTable(terrainSize, terrainOffset);

        for (int y = 0; y < terrainSize.y; y++)
        {
            for (int x = 0; x < terrainSize.x; x++)
            {
                int currentIndex = x + y * terrainSize.x;
                Vector3 currentPosition = terrainMesh.Vertices[currentIndex];

                float closestDistance;
                //Vector3 closest = GetClosestPoint(currentPosition, out closestDistance);
                Vector3 closest = GetClosestPointInHashTable(currentPosition, terrainOffset, out closestDistance);
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


    #region SPATIAL HASH

    /// <summary>
    /// Genera un array con todos los puntos discretos que definenen las curvas y que ya se han generado previamente.
    /// De esta manera se evita tener que calcular el punto mas cercano a la curva continuamente y simplemente se tiene que buscar cual de estos puntos es el mas cercano.
    /// Se excluirá el ultimo punto de segmento de la curva para evitar duplicados en la tabla y aumentar la eficiencia
    /// </summary>
    /// <param name="paths">Lista de todos los paths</param>
    /// <param name="pathSegments">Número de segmentos en en los que se subdivide cada curva de path (los usados en el line renderer)</param>
    /// <returns></returns>
    private void GeneratePathsLookUpTable(List<NodePath> paths)
    {
        int subdivisions = paths[0].Line.positionCount-1;
        _pathPointsLUT = new Vector3[paths.Count*subdivisions];

        for (int i = 0; i < paths.Count; i++)
        {
            NodePath path = paths[i];
            Vector3[] linePoints = new Vector3[path.Line.positionCount];
            path.Line.GetPositions(linePoints);

            for(int s = 0; s < subdivisions; s++)
            {
                Vector3 point = linePoints[s];
                _pathPointsLUT[i * subdivisions + s] = point;
            }
        }
    }

    /// <summary>
    /// Genera una tabla hash con una lista de puntos que pertenecen a cada celda de la tabla según su posición y el radio de detección
    /// </summary>
    /// <param name="terrainDimensions"></param>
    /// <param name="terrainOffset"></param>
    private void GenerateHashTable(Vector2 terrainDimensions, Vector3 terrainOffset)
    {
        _hashTable = new List<int>[HashDimensions.x,HashDimensions.y];
        _cellSize = terrainDimensions/HashDimensions;

        //Inicializar tabla de hash
        for (int y = 0; y < HashDimensions.y; y++)
        {
            for (int x = 0; x < HashDimensions.x; x++)
            {
                _hashTable[x,y] = new List<int>();
            }
        }

        //Rellenar cada celda con los vértices que están dentro (o cerca)
        for(int i=0; i<_pathPointsLUT.Length; i++)
        {
            AddToHashTable(i, _pathPointsLUT[i]+terrainOffset, RadialCheckRadius);
        }
    }

    /// <summary>
    /// Añade un punto a la celda hash correspondiente segun sus coordenadas XZ. Si se indica un radio mayor que 0 se comprobará si pertenece también a celdas circundantes.
    /// </summary>
    /// <param name="lutIndex">Índice del punto en la LookUpTable</param>
    /// <param name="position">Posicion del punto a incluir en la tabla con el offset del terreno ya aplicado/param>
    /// <param name="cellSize">Dimensiones de cada celda de la tabla</param>
    /// <param name="checkRadius">Radio de comprobación de otras celdas cercanas a las que pertenece el punto</param>
    private void AddToHashTable(int lutIndex, Vector3 position, float checkRadius = 0)
    {
        Vector2Int cell = GetHashCell(position);
        _hashTable[cell.x, cell.y].Add(lutIndex);

        //Si no hay radio, no comprobamos celdas de alrededores (evitamos así bucles infinitos)
        if (checkRadius > 0)
        {
            //Comprobamos si estamos a menos de cierta distancia de otra celda para añadirlo a su lista de celdas
            for (float angle = 0f; angle < PI2; angle += RadialStepIncrement)
            {
                AddToHashTable(lutIndex, position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * checkRadius);
            }
        }
    }

    /// <summary>
    /// Devuelve la celda de la tabla hash a la que corresponde una posicion
    /// </summary>
    /// <param name="position"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    private Vector2Int GetHashCell(Vector3 position)
    {
        float xProp = position.x / _cellSize.x;
        float yProp = position.z / _cellSize.y;

        Vector2Int cell = new Vector2Int((int)xProp, (int)yProp);
        cell.x = Mathf.Clamp(cell.x, 0, HashDimensions.x-1);
        cell.y = Mathf.Clamp(cell.y, 0, HashDimensions.y-1);

        return cell;
    }

    #endregion


    #region UTILITIES

    /// <summary>
    /// Devuelve el punto más cercano de la lista a la posicion indicada, así como la distancia a ese punto
    /// </summary>
    /// <param name="points">Lista de posibles candidatos a ser el puinto mas cercano al origen</param>
    /// <param name="originPosition">Punto de referencia sobre el que se calculara la menor distancia</param>
    /// <param name="closestDistance">Distancia al punto mas cercano</param>
    /// <returns></returns>
    public Vector3 GetClosestPoint(Vector3 originPosition, out float closestDistance)
    {
        closestDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.negativeInfinity;
        Vector2 currentPosition2D = originPosition.GetXZ();

        foreach (var point in _pathPointsLUT)
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

    /// <summary>
    /// Devuelve el punto mas cercano a la posición pasada mediante la busqueda en la tabla hash por motivos de optimización
    /// </summary>
    /// <param name="realPosition">Posicion real en el mundo</param>
    /// <param name="positionOffset">Offset de posicion para pasar al espacio en el que trabaja la tabla hash</param>
    /// <param name="closestDistance">Distancia al punto mas cercano</param>
    /// <returns>Punto mas cercano en cordenadas del mundo (no de tabla hash)</returns>
    public Vector3 GetClosestPointInHashTable(Vector3 realPosition, Vector3 positionOffset, out float closestDistance)
    {
        Vector3 hashPosition = realPosition + positionOffset;
        closestDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.negativeInfinity;
        Vector2 currentPosition2D = hashPosition.GetXZ();

        Vector2Int cell = GetHashCell(hashPosition);

        foreach (int lutIndex in _hashTable[cell.x,cell.y])
        {
            Vector3 lutPoint = _pathPointsLUT[lutIndex] + positionOffset;
            float currentDistance = Vector2.Distance(currentPosition2D, lutPoint.GetXZ());
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestPoint = lutPoint-positionOffset;
            }
        }

        return closestPoint;
    }

    #endregion
}