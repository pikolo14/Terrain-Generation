using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PointsGeneration
{
    public const string GroundTag = "Ground";


    #region POISSON DISC POINTS

    /// <summary>
    /// Generación de puntos en una zona asegurando cierta distancia entre cada punto. Crea una rejilla y solo permite que haya un punto en cada celda. 
    /// </summary>
    /// <param name="radius">Radio que debe de haber entre puntosv</param>
    /// <param name="bounds">Volumen que envuelve y define el área generación de puntos</param>
    /// <param name="triesTillRejection">Número de intentos fallidos en el que se para de generar un nuevo punto válido</param>
    /// <returns></returns>
    public static List<Vector2> GeneratePoissonDiscPointsWithBounds(float radius, Bounds bounds, int seed, int triesTillRejection = 30)
    {
        return GeneratePoissonDiscPoints(radius, bounds.center, new Vector2(bounds.size.x, bounds.size.z), seed, triesTillRejection);
    }

    /// <summary>
    /// Generación de puntos en una zona asegurando cierta distancia entre cada punto. Crea una rejilla y solo permite que haya un punto en cada celda. 
    /// </summary>
    /// <param name="radius">Radio que debe de haber entre puntos</param>
    /// <param name="origin">Centro del area rectangular sobre el que se generarán los puntos</param>
    /// <param name="zoneSize">Dimensiones del área rectgangular de generación</param>
    /// <param name="triesTillRejection">Número de intentos fallidos en el que se para de generar un nuevo punto válido</param>
    /// <returns></returns>
    public static List<Vector2> GeneratePoissonDiscPoints(float radius, Vector3 origin, Vector2 zoneSize, int seed, int triesTillRejection = 30)
    {
        System.Random randomGenerator = new System.Random(seed);

        //Como el radio es la hipotenusa de un triángulo isósceles utilizamos tma. pitágoras para obtener las dimensiones de la celda cuadrada
        float cellSize = radius / Mathf.Sqrt(2);

        //Listas de puntos de la celda y de spawn
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();
        //Almacenamos los índices de los puntos situados en cada celda
        
        int[,] grid = new int[Mathf.CeilToInt(zoneSize.x / cellSize) + 1, Mathf.CeilToInt(zoneSize.y / cellSize) + 1];

        //Inicializamos la lista de puntos con el centro de la zona
        spawnPoints.Add(zoneSize / 2f);

        while (spawnPoints.Count > 0)
        {
            //Partimos de un punto aleatorio de los que ya hemos creado
            int spawnIndex = randomGenerator.Next(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];

            bool accepted = false;

            //Obtenemos un punto en una direccion aleatoria a partir del punto cogido como referencia y comprobamos si es válido
            //Si no es válido se vuelve a probar hasta alcanzar un máximo de intentos
            for (int i = 0; i < triesTillRejection; i++)
            {
                Vector2 dir = new Vector2(randomGenerator.NextFloat(-1, 1), randomGenerator.NextFloat(-1, 1)).normalized * randomGenerator.NextFloat(radius, radius * 2);
                Vector2 candidate = spawnCenter + dir;

                //Si es válido se añade a las listas y se para de probar
                if (IsPoissonValidPoint(candidate, zoneSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    //Añadir indice a la casilla de la grid
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    accepted = true;
                    break;
                }
            }

            if (!accepted)
                spawnPoints.RemoveAt(spawnIndex);
        }

        for (int i = 0; i < points.Count; i++)
        {
            points[i] += new Vector2(origin.x, origin.y) - zoneSize/2f;
        }

        return points;
    }

    private static bool IsPoissonValidPoint(Vector2 candidate, Vector2 zoneSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        //Si el candidato está dentro de la zona disponible de spawn
        if (candidate.x >= 0 && candidate.x < zoneSize.y && candidate.y >= 0 && candidate.y < zoneSize.y)
        {
            //Celda que ocupa
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);

            //Rango de indices de celdas de alrededor en los que se buscará (2 casillas de distancia)
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Max(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Max(cellY + 2, grid.GetLength(1) - 1);

            //Recorremos este bloque de 5x5 celdas 
            for (int x = searchStartX; x < searchEndX; x++)
            {
                for (int y = searchStartY; y < searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    //Si la celda contiene un punto...
                    if (pointIndex != -1)
                    {
                        //Si el punto de la celda esta a menos de la distancia estipulada no será valido
                        float sqrDist = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDist < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            //Si se cumplen la distancia con los puntos circundantes, es valido
            return true;
        }

        return false;
    }

    //Proyecta un rayo en el eje Y para obtener la posicion 3D de un punto en el plano x,z
    public static bool GetGroundPoint(Vector2 point2D, Bounds bounds, out Vector3 res)
    {
        RaycastHit hit;
        Vector3 origin = new Vector3(point2D.x, 100, point2D.y) + bounds.center;
        Ray ray = new Ray(origin, Vector3.down);
        //Ignoramos los triggers detectores de los enemigos
        int layerMask = ~(1 >> LayerMask.NameToLayer("IATriggers"));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) && hit.collider.gameObject.CompareTag(GroundTag))
        {
            res = hit.point;
            return true;
        }

        res = Vector3.zero;
        return false;
    }

    #endregion


    #region NEAR RANDOM POITNS

    /// <summary>
    /// Devuelve un punto aleatorio
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    public static Vector3 GetRandomPointInCircle(Vector3 origin, float maxRadius)
    {
        Vector2 randomDirection = Random.insideUnitCircle;
        Vector3 point = origin + new Vector3(randomDirection.x, 0, randomDirection.y) * maxRadius;

        return point;
    }

    public static Vector2 GetRandomPointInCircle2D(Vector2 origin, float maxRadius)
    {
        Vector2 randomDirection = Random.insideUnitCircle;
        Vector2 point = origin + randomDirection * maxRadius;

        return point;
    }

    #endregion
}
