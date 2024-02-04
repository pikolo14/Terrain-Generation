using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class NodePoint
{
    [HideInInspector]
    public Vector2 Position2D;
    public GameObject GO;
    [HideInInspector]
    public List<NodePath> Paths;

    public NodePoint(Vector2 position2D, GameObject gO)
    {
        Position2D = position2D;
        GO = gO;
        Paths = new List<NodePath>();
    }

    /// <summary>
    /// Devuelve el path dentro de este nodo mas opuesto al indicado. Se devolverá un path con una direccion a mas de 90º de la direcion de este path y lo más cercano a 180º posible.
    /// </summary>
    /// <param name="path">Path para el cual debemos buscar su opuesto</param>
    /// <param name="oppositeDirection">Direccion del path opuesto devuelto. Será 0 si no se encuentra un path</param>
    /// <returns>Path más opuesto al pasado por parámetro. Será igual al path parametro si no se encuentra un path</returns>
    public NodePath GetOppositePathTo(NodePath path, out Vector2 oppositeDirection)
    {
        Vector2 currentDirection;
        NodePath oppositePath = path;
        float maxAngle = 0f;
        oppositeDirection = Vector2.zero;

        if(path.TryGetDirectionFrom(this, out currentDirection))
        {
            //Encontrar los otros paths que salen de cada extremo y obtener sus vectores direccion
            foreach (var candidatePath in Paths)
            {
                //Obtenemos el angulo que forman nuestro camino y cada otro camino que hay en este nodo
                Vector2 candidateDirection;
                if (!candidatePath.TryGetDirectionFrom(this, out candidateDirection))
                    continue;

                float angle = Vector2.Angle(candidateDirection, currentDirection);

                //El path opuesto elegido debe de estar a mas de 90º y estar lo mas cercano posible a 180º
                if(candidatePath!=path && angle > 90 && angle > maxAngle)
                {
                    maxAngle = angle;
                    oppositePath = candidatePath;
                    oppositeDirection = candidateDirection;
                }
            }
        }

        //if (oppositePath == path)
        //    Debug.Log("Path opuesto valido no encontrado");
        return oppositePath;
    }

    #region OPERATORS OVERRIDE

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
    public override bool Equals(object other)
    {
        if (other is NodePoint)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Position2D == (other as NodePoint).Position2D;
        }

        return false;
    }

    #endregion
}
