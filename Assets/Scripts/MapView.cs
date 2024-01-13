using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine.Events;

public class MapView : MonoBehaviour 
{
	public Renderer TextureRender;
	public Gradient HeightGradient;
	public UnityEvent OnViewParametersChanged = new UnityEvent();

    public bool ShowNoiseTexture = false;


	/// <summary>
	/// Actualiza la visualizacion del mapa introducido
	/// </summary>
	/// <param name="heightMap"></param>
	public void DrawMap(float[,] heightMap)
	{
        Color[] colorArray = GetColorArray(heightMap, ShowNoiseTexture);
        Texture2D texture = TextureUtils.GetTextureFromColorArray(colorArray, heightMap.GetLength(0), heightMap.GetLength(1));
        DrawTexture(texture);
	}

    /// <summary>
    /// Devuelve el array con los colores necesario para pasarselo a una textura
    /// </summary>
    /// <param name="heightMap"></param>
    /// <param name="showNoiseTexture"></param>
    /// <returns></returns>
    public Color[] GetColorArray(float[,] heightMap, bool showNoiseTexture)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colorArray = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (showNoiseTexture)
                    colorArray[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                else
                    colorArray[y * width + x] = HeightGradient.Evaluate(heightMap[x, y]);
            }
        }

        return colorArray;
    }

    /// <summary>
    /// Coloca la textura introducida en el en render
    /// </summary>
    /// <param name="texture"></param>
    public void DrawTexture(Texture2D texture)
	{
        TextureRender.sharedMaterial.mainTexture = texture;
        TextureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    private void OnValidate()
    {
		OnViewParametersChanged?.Invoke();
    }
}