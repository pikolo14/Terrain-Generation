using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomExtensions
{
    public static Vector2 Project(this Vector2 v1, Vector2 v2)
    {
        return v1*v2/(v2.magnitude*v2.magnitude) * v2;
    }
    
    public static Vector2 GetXZ(this Vector3 v)
    {
        return new Vector2 (v.x, v.z);
    }

    public static Vector3 ToVector3XZ(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static Vector3 ToVector3_XZ(this Vector2Int v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static T[] SubArray<T>(this T[] data, int index, int length)
    {
        T[] result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
}
