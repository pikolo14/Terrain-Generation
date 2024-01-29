using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


[Serializable]
public class NodePath
{
    [NonSerialized]
    public NodePoint P1, P2;
    public Vector2 M1, M2;
    public LineRenderer Line;

    /// <summary>
    /// Devuelve la tangente de la curva en uno de los extremos descrita entre el modificador y el punto
    /// </summary>
    /// <param name="pointIndex">Índice del extremo del cual se devolverá la tangente</param>
    /// <param name="tangent">Vector tangente resultante de ese extremo</param>
    /// <returns>Devuelve true si se ha especificado una tantente (vector distinto de 0,0)</returns>
    public bool TryGetTangent(int pointIndex, out Vector2 tangent)
    {
        tangent = Vector2.zero;

        if (pointIndex == 0 && M1 != P1.Position2D)
        {
            tangent = M1 - P1.Position2D;
            return true;
        }
        else if (pointIndex == 1 && M2 != P2.Position2D)
        {
            tangent = M2 - P2.Position2D;
            return true;
        }

        return false;
    }


    #region NORMAL PATH DRAWING

    public void DrawStaightLine()
    {
        Line.positionCount = 2;
        Line.SetPositions(new Vector3[] { P1.GO.transform.position, P2.GO.transform.position });
    }

    public void DrawRandomCurve(float maxRandomCurveRadius)
    {
        Vector3[] positions = CurveGeneration.GetRandomBezierCurve(P1.GO.transform.position, P2.GO.transform.position, maxRandomCurveRadius);
        Line.positionCount = positions.Length;
        Line.SetPositions(positions);
    }

    #endregion


    #region CONTINUOUS CURVES DRAWING
    public void PrepareDrawingOppositeContinuousCurve(float maxRandomCurveRadius, Vector2 preferredDirection)
    {
        PrepareOneOppositeContinuousCurve(0, ref P1, ref M1, maxRandomCurveRadius);
        PrepareOneOppositeContinuousCurve(1, ref P2, ref M2, maxRandomCurveRadius);
    }

    private void PrepareOneOppositeContinuousCurve( int pointIndex, ref NodePoint point, ref Vector2 modifier, float maxRandomCurveRadius)
    {
        //Recogemos el path opuesto para cada nodo
        Vector2 pathDir;
        Vector2 tangent;
        NodePath opposite = point.GetOppositePathTo(this, out pathDir);

        //Si hay path opuesto y tiene tangente...
        if (pathDir != Vector2.zero && opposite.TryGetTangent(pointIndex, out tangent))
        {
            //Aplicamos la misma pero en sentido contrario para nuestro path actual
            //TODO: Poner una maginitud aleatoria ente un rango?
            modifier = point.Position2D - tangent;
        }
        //Si no hay path opuesto o no hay tangente...
        else
        {
            //Asignamos un valor aleatorio
            //TODO: siguiendo la direccion preferida (tormenta)
            var rand = CurveGeneration.GetRandomPointInCircle2D(point.Position2D, maxRandomCurveRadius);
            modifier = rand;
        }
    }

    public void DrawPreparedContinuousCurve()
    {
        Vector3 m1 = new Vector3(M1.x, P1.GO.transform.position.y, M1.y);
        Vector3 m2 = new Vector3(M2.x, P2.GO.transform.position.y, M2.y);
        var positions = CurveGeneration.GetBezierCurveSection(P1.GO.transform.position, P2.GO.transform.position, m1, m2);

        Debug.DrawLine(m1, P1.GO.transform.position, Color.blue, 3);
        Debug.DrawLine(m2, P2.GO.transform.position, Color.blue, 3);

        Line.positionCount = positions.Length;
        Line.SetPositions(positions);
    }

    #endregion


    #region PATH VECTORS

    public Vector2 GetDirection()
    {
        Vector2 direction = Vector2.zero;

        if (P1 != null && P2 != null)
            direction = P2.Position2D - P1.Position2D;

        return direction;
    }

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

        Debug.LogError("");
        direction = Vector2.zero;
        return false;
    }

    public void GetOppositePathsDirections(out Vector2 direction1, out Vector2 direction2)
    {
        P1.GetOppositePathTo(this, out direction1);
        P2.GetOppositePathTo(this, out direction2);
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
