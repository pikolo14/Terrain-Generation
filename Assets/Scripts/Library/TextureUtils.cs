using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AKA TextureGenerator 
/// </summary>
public static class TextureUtils
{
    /// <summary>
    /// Devuelve una textura con el array de colores y dimensiones de imagen introducidos
    /// </summary>
    /// <param name="colorArray"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Texture2D GetTextureFromColorArray(Color[] colorArray, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.SetPixels(colorArray);
        texture.Apply();

        return texture;
    }
}