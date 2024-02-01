using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomExtensions
{
    public static Vector2 Project(this Vector2 v1, Vector2 v2)
    {
        return v1*v2/(v2.magnitude*v2.magnitude) * v2;
    }
    
}
