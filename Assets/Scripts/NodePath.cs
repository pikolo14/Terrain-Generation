using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class NodePath :MonoBehaviour
{
    [NonSerialized]
    public NodePoint P1, P2;
    public Vector2 M1, M2;
    public LineRenderer Line;
    public NodePath Opposite1, Opposite2;


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
    public void DrawRandomCurve(float maxRandomCurveRadius)
    {
        Vector3[] positions = CurveGeneration.GetRandomBezierCurve(P1.GO.transform.position, P2.GO.transform.position, maxRandomCurveRadius);
        Line.positionCount = positions.Length;
        Line.SetPositions(positions);
    }

    /// <summary>
    /// Dibuja la l�nea de Bezier seg�n la posici�n de los extremos y modificadores. Requiere haber hecho la preparacion previa
    /// </summary>
    public void DrawPreparedCurve()
    {
        Vector3 m1 = new Vector3(M1.x, P1.GO.transform.position.y, M1.y);
        Vector3 m2 = new Vector3(M2.x, P2.GO.transform.position.y, M2.y);
        var positions = CurveGeneration.GetBezierCurveSection(P1.GO.transform.position, P2.GO.transform.position, m1, m2);
        Line.positionCount = positions.Length;
        Line.SetPositions(positions);
    }

    #endregion


    #region CONTINUOUS CURVES PREPARATION

    public void PrepareDrawingOppositeContinuousCurve(Vector2 preferredDirection, float randomTangentVariation, Vector2 randomTangentMagnitudeRange)
    {
        PrepareOneOppositeContinuousCurve(0, ref P1, ref M1, preferredDirection, randomTangentVariation, randomTangentMagnitudeRange);
        PrepareOneOppositeContinuousCurve(1, ref P2, ref M2, preferredDirection, randomTangentVariation, randomTangentMagnitudeRange);
    }

    private void PrepareOneOppositeContinuousCurve( int pointIndex, ref NodePoint point, ref Vector2 modifier, Vector2 preferredDirection, float randomTangentVariation, Vector2 randomTangentMagnitudeRange)
    {
        //Recogemos el path opuesto para cada nodo
        Vector2 pathDir;
        Vector2 tangent;
        NodePath opposite = point.GetOppositePathTo(this, out pathDir);

        //Si hay path opuesto y tiene tangente...
        if (pathDir != Vector2.zero && opposite.TryGetTangent(point, out tangent))
        {
            //Aplicamos la misma pero en sentido contrario para nuestro path actual
            //TODO: Poner una maginitud aleatoria ente un rango?
            modifier = point.Position2D - tangent;
        }
        //Si no hay path opuesto o no hay tangente...
        else
        {
            ////Asignamos un valor completamente aleatorio
            //var rand = PointsGeneration.GetRandomPointInCircle2D(point.Position2D, maxRandomCurveRadius);
            //modifier = rand;

            //Tomamos la direccion preferida para obtener la tangente (en una direccion u otra en funci�n del extremo que sea)
            Vector2 relativePathDirection = GetPathDirectionFrom(point);
            tangent = relativePathDirection.Project(preferredDirection).normalized * UnityEngine.Random.Range(randomTangentMagnitudeRange.x, randomTangentMagnitudeRange.y);
            tangent = GetRandomVectorInAngle(tangent, randomTangentVariation);
            modifier = point.Position2D + tangent;
        }
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
    /// <param name="point">NodePoint del extremo del cual se devolver� la tangente</param>
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
    /// <param name="point">Extremo del path que se tomar� como origen para calcular la distancia (el vector cambiar� de sentido en funcion del punto pasado)</param>
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
    /// Devuelve un vector con un �ngulo aleatorio dentro de un rango alrededor de un vector 2D
    /// </summary>
    /// <param name="mainDirection"></param>
    /// <param name="angleRange"></param>
    /// <returns></returns>
    public static Vector2 GetRandomVectorInAngle(Vector2 mainDirection, float angleRange)
    {
        float angle = UnityEngine.Random.Range(-angleRange, angleRange);
        return Quaternion.AngleAxis(angle, Vector3.forward) * mainDirection;
    }

    #endregion


    #region GIZMOS AND EDITOR

    private void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(P1.GO.transform.position, new Vector3(M1.x, P1.GO.transform.position.y, M1.y));
            Gizmos.DrawLine(P2.GO.transform.position, new Vector3(M2.x, P2.GO.transform.position.y, M2.y));
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

    public void UpdateOpposites()
    {
        Opposite1 = P1.GetOppositePathTo(this, out _);
        Opposite2 = P2.GetOppositePathTo(this, out _);
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
