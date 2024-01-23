using System;
using System.Collections;
using System.Collections.Generic;
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
    public override int GetHashCode()
    {
        return HashCode.Combine(Position2D, GO, Paths);
    }
}
