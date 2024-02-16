using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class NodePath :MonoBehaviour
{
    private readonly static Vector2 _randomOppositeDistanceVariation = new Vector2(0.5f,2f);

    [NonSerialized]
    public NodePoint P1, P2;
    public Vector2 M1, M2;
    public LineRenderer Line;

    //Seccion principal de la curva
    private NodePathSection _section;


    #region LINE DRAWING

    /// <summary>
    /// Dibuja una linea recta entre los extremos del path
    /// </summary>
    public void DrawStaightLine()
    {
        Line.positionCount = 2;
        Line.SetPositions(new Vector3[] { P1.GO.transform.position, P2.GO.transform.position });
    }

    /// <summary>
    /// Dibuja una curva de bezier con los modificadores colocados aleatoriamente
    /// </summary>
    /// <param name="maxRandomCurveRadius"></param>
    public void DrawRandomCurve(float maxRandomCurveRadius, int subdivisions)
    {
        Vector3[] positions = CurveGeneration.GetRandomBezierCurve(P1.GO.transform.position, P2.GO.transform.position, maxRandomCurveRadius, subdivisions);
        Line.positionCount = positions.Length;
        Line.SetPositions(positions);
    }

    /// <summary>
    /// Dibuja la línea de Bezier según la posición de los extremos y modificadores. Requiere haber hecho la preparacion previa
    /// </summary>
    public void DrawPreparedCurveSimply(int subdivisions)
    {
        Vector3 m1 = new Vector3(M1.x, P1.GO.transform.position.y, M1.y);
        Vector3 m2 = new Vector3(M2.x, P2.GO.transform.position.y, M2.y);
        var positions = CurveGeneration.GetBezierCurveSection(P1.GO.transform.position, P2.GO.transform.position, m1, m2, subdivisions);
        Line.positionCount = positions.Length;
        Line.SetPositions(positions);
    }

    /// <summary>
    /// Dibuja una curva más compleja compuesta de varias subsecciones compuestas a su vez por subsecciones recursivamente
    /// </summary>
    /// <param name="subdivisions">Número de puntos de curva de bezier que se obtendrán de cada sección situada en el nivel mas bajo de recursividad</param>
    public void DrawPreparedCurveWithSections(int subdivisions)
    {
        List<Vector3> linePoints = new List<Vector3>();
        linePoints.Add(P1.GO.transform.position);
        _section.GetSectionPointsRecursively(ref linePoints, subdivisions);
        Line.positionCount = linePoints.Count;
        Line.SetPositions(linePoints.ToArray());
    }

    #endregion


    #region CONTINUOUS CURVES PREPARATION

    /// <summary>
    /// Prepara ambos extremos del path para el dibujo de paths que intenten continuar a su camino opuesto o una direccion
    /// </summary>
    /// <param name="preferredDirection">Direccion preferida de la tangente de los caminos</param>
    /// <param name="randomTangentVariation">Variación aleatoria máxima del ángulo de la tangente respecto a la dirección preferida</param>
    /// <param name="randomTangentMagnitudeRange">Rango de variación aleatoria de la distancia a la que se colocará el punto modificador</param>
    public void PrepareDrawingOppositeContinuousCurve(Vector2 preferredDirection, float randomTangentVariation, Vector2 randomTangentMagnitudeRange)
    {
        PrepareOneOppositeContinuousCurve(0, ref P1, ref M1, preferredDirection, randomTangentVariation, randomTangentMagnitudeRange);
        PrepareOneOppositeContinuousCurve(1, ref P2, ref M2, preferredDirection, randomTangentVariation, randomTangentMagnitudeRange);
    }

    /// <summary>
    /// Dibuja
    /// </summary>
    /// <param name="pointIndex"></param>
    /// <param name="point"></param>
    /// <param name="modifier"></param>
    /// <param name="preferredDirection"></param>
    /// <param name="randomTangentVariation"></param>
    /// <param name="randomTangentMagnitudeRange"></param>
    private void PrepareOneOppositeContinuousCurve( int pointIndex, ref NodePoint point, ref Vector2 modifier, Vector2 preferredDirection, float randomTangentVariation, Vector2 randomTangentMagnitudeRange)
    {
        //Obtenemos el path opuesto
        Vector2 pathDir;
        Vector2 tangent;
        NodePath opposite = point.GetOppositePathTo(this, out pathDir);

        //Si hay path opuesto y tiene tangente...
        if (pathDir != Vector2.zero && opposite.TryGetTangent(point, out tangent))
        {
            //Aplicamos la misma pero en sentido contrario para nuestro path actual, cambiando un poco la distancia del punto
            modifier = point.Position2D - tangent*UnityEngine.Random.Range(_randomOppositeDistanceVariation.x, _randomOppositeDistanceVariation.y);
        }
        //Si no hay path opuesto o no hay tangente...
        else
        {
            //Tomamos la direccion preferida para obtener la tangente (en una direccion u otra en función del extremo que sea)
            Vector2 relativePathDirection = GetPathDirectionFrom(point);
            tangent = relativePathDirection.Project(preferredDirection).normalized * UnityEngine.Random.Range(randomTangentMagnitudeRange.x, randomTangentMagnitudeRange.y);
            tangent = GetRandomVectorInAngle2D(tangent, randomTangentVariation);
            modifier = point.Position2D + tangent;
        }
    }

    #endregion


    #region SUBSECTIONS

    /// <summary>
    /// Divide la curva principal del path actual en varias secciones. Estas subsecciones se pueden seguir subdividiendo sucesivamente hasta el número de niveles indicados por la recursividad.
    /// </summary>
    /// <param name="sectionsCount">Número de secciones en las que se dividira la curva en este nivel</param>
    /// <param name="recursivity">Número de veces consecutivas que se subdividirá cada subsección recursivamente (subsecciones de subsecciones de subsecciones, etc.)</param>
    public void DoRecursiveSubsection(int sectionsPerLevel, int recursivityLevels, float maxAngleSubsectionVariation, float customSubsectionsTangentMultiplier)
    {
        Vector3 m1_3D = new Vector3(M1.x, P1.GO.transform.position.y, M1.y);
        Vector3 m2_3D = new Vector3(M2.x, P2.GO.transform.position.y, M2.y);

        _section = new NodePathSection(P1.GO.transform.position, P2.GO.transform.position, m1_3D, m2_3D, sectionsPerLevel, maxAngleSubsectionVariation, customSubsectionsTangentMultiplier, recursivityLevels);
    }

    #endregion


    #region PATH VECTORS

    /// <summary>
    /// Devuelve el vector direccion entre los 2 nodos que forman el path, partiendo desde el nodo espeficado
    /// </summary>
    /// <param name="startPoint">Nodo del cual se empieza a calcular el vector direccion</param>
    /// <param name="direction">Direccion resultante del path</param>
    /// <returns>Si el nodo pasado no coincide con ningun extermo del path se devuelve false</returns>
    public bool TryGetDirectionFrom(NodePoint startPoint, out Vector2 direction)
    {
        if (startPoint == P1)
        {
            direction = P2.Position2D - P1.Position2D;
            return true;
        }
        else if(startPoint == P2)
        {
            direction = P1.Position2D - P2.Position2D;
            return true;
        }

        direction = Vector2.zero;
        return false;
    }

    /// <summary>
    /// Devuelve la tangente de la curva en uno de los extremos descrita entre el modificador y el punto
    /// </summary>
    /// <param name="point">NodePoint del extremo del cual se devolverá la tangente</param>
    /// <param name="tangent">Vector tangente resultante de ese extremo</param>
    /// <returns>Devuelve true si se ha especificado una tantente (vector distinto de 0,0)</returns>
    public bool TryGetTangent(NodePoint point, out Vector2 tangent)
    {
        tangent = Vector2.zero;

        if (point == P1 && M1 != P1.Position2D)
        {
            tangent = M1 - P1.Position2D;
            return true;
        }
        else if (point == P2 && M2 != P2.Position2D)
        {
            tangent = M2 - P2.Position2D;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Devuelve el vector 2D direccion que sigue el path tomando como origen uno de sus extremos
    /// </summary>
    /// <param name="point">Extremo del path que se tomará como origen para calcular la distancia (el vector cambiará de sentido en funcion del punto pasado)</param>
    /// <returns></returns>
    public Vector2 GetPathDirectionFrom(NodePoint point)
    {
        if (point == P1)
            return P2.Position2D - P1.Position2D;
        else if (point == P2)
            return P1.Position2D - P2.Position2D;
        return Vector2.zero;
    }

    /// <summary>
    /// Devuelve un vector con un ángulo aleatorio dentro de un rango alrededor de un vector 2D
    /// </summary>
    /// <param name="mainDirection"></param>
    /// <param name="angleRange"></param>
    /// <returns></returns>
    public static Vector2 GetRandomVectorInAngle2D(Vector2 mainDirection, float angleRange)
    {
        float angle = UnityEngine.Random.Range(-angleRange, angleRange);
        return Quaternion.AngleAxis(angle, Vector3.forward) * mainDirection;
    }

    /// <summary>
    /// Devuelve un vector con un ángulo aleatorio dentro de un rango alrededor de un vector 2D
    /// </summary>
    /// <param name="mainDirection"></param>
    /// <param name="angleRange"></param>
    /// <returns></returns>
    public static Vector3 GetRandomVectorInAngle3D(Vector3 mainDirection, float angleRange)
    {
        Vector2 tangent2D = new Vector2(mainDirection.x, mainDirection.z);
        Vector2 aux = GetRandomVectorInAngle2D (tangent2D, angleRange);
        Vector3 tangent3D = new Vector3(aux.x, mainDirection.y, aux.y);
        return tangent3D;
    }

    #endregion


    #region EDITOR & GIZMOS

    private void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.blue;
            //Gizmos.DrawLine(P1.GO.transform.position, new Vector3(M1.x, P1.GO.transform.position.y, M1.y));
            //Gizmos.DrawLine(P2.GO.transform.position, new Vector3(M2.x, P2.GO.transform.position.y, M2.y));
        }
        catch (Exception) { }
    }

    private void OnDrawGizmosSelected()
    {
        try
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(P1.GO.transform.position, new Vector3(M1.x, P1.GO.transform.position.y, M1.y));
            Gizmos.DrawLine(P2.GO.transform.position, new Vector3(M2.x, P2.GO.transform.position.y, M2.y));
            Gizmos.DrawSphere(new Vector3(M1.x, P1.GO.transform.position.y, M1.y), 0.5f);
            Gizmos.DrawSphere(new Vector3(M2.x, P2.GO.transform.position.y, M2.y), 0.5f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(P1.GO.transform.position, 1.5f);
        }
        catch (Exception) { }
    }

    #endregion


    #region OPERATORS OVERRIDE

    public static bool operator ==(NodePath obj1, NodePath obj2)
    {
        return (obj1.P1 == obj2.P1 && obj1.P2 == obj2.P2);
    }
    public static bool operator !=(NodePath obj1, NodePath obj2) => !(obj1 == obj2);
   
    #endregion
}
