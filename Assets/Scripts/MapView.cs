using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine.Events;

public class MapView : MonoBehaviour 
{
    public enum DrawMode
    {
        Noise,
        Color,
        Mesh
    }

	public Renderer TextureRender;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    public DrawMode Mode = DrawMode.Mesh;
	public Gradient HeightGradient;
	public UnityEvent OnViewParametersChanged = new UnityEvent();


	/// <summary>
	/// Actualiza la visualizacion del mapa introducido
	/// </summary>
	/// <param name="heightMap"></param>
	public void DrawMap(float[,] heightMap, float heightMultiplier)
	{
        //Generate texture
        Color[] colorArray = GetColorArray(heightMap, Mode, HeightGradient);
        Texture2D texture = TextureUtils.GetTextureFromColorArray(colorArray, heightMap.GetLength(0), heightMap.GetLength(1));

        //Draw 3D mesh or plane texture
        if (Mode == DrawMode.Mesh)
            DrawMeshMap(heightMap, texture, heightMultiplier);
        else
            DrawTexture(texture);
	}

    
    #region MESH GENERATION

    public void DrawMeshMap(float[,] heightMap, Texture2D texture, float heightMultiplier)
    {
        MeshData meshData = MeshGeneration.GenerateTerrainMesh(heightMap, heightMultiplier);
        MeshFilter.sharedMesh = meshData.GetMesh();
        MeshRenderer.sharedMaterial.mainTexture = texture;
    }

    #endregion


    #region TEXTURE GENERATION

    /// <summary>
    /// Devuelve el array con los colores necesario para pasarselo a una textura
    /// </summary>
    /// <param name="heightMap"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static Color[] GetColorArray(float[,] heightMap, DrawMode mode, Gradient colorGradient = null)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Color[] colorArray = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mode == DrawMode.Noise)
                    colorArray[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                else
                    colorArray[y * width + x] = colorGradient.Evaluate(heightMap[x, y]);
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

    #endregion


    #region EDITOR

    private void OnValidate()
    {
        OnViewParametersChanged?.Invoke();
    }

    #endregion
}