using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct NodePath
{
    [NonSerialized]
    public NodePoint P1, P2;
    public LineRenderer Line;

    public Vector2 GetDirection()
    {
        Vector2 direction = Vector2.zero;

        if (P1 != null && P2 != null)
            direction = P2.Position2D - P1.Position2D;

        return direction;
    }

    public NodePath GetOppositePath()
    {
        NodePath opposite = default;
        //TODO: 
        //Encontrar los paths opuestos de cada extremo y obtener sus vectores direccion
        //El path opuesto debe de estar a mas de 90º absolutos repecto a la direccion recta inicial
        //Calcular la posicion del modificador de cada extremo del path que siga en la tangente de los paths opuestos
        //Poner una maginitud aleatoria ente un rango?
        //Si no hay opuesto coger direccion aleatoria?
        return opposite;
    }
}
