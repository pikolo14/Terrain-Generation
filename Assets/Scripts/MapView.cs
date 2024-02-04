using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class MapView : MonoBehaviour 
{
    public enum DrawMode
    {
        Noise,
        Color
    }

    [SerializeField]
    public DrawMode Mode { get => _mode; set => _mode = value; }
    private static DrawMode _mode = DrawMode.Color;

    public Renderer TextureRender;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

	public Gradient HeightGradient;

    [SerializeField][HideInInspector]
	public UnityEvent OnViewParametersChanged = new UnityEvent();

    [SerializeField]
    public Texture2D MeshTexture;


    /// <summary>
    /// Dibuja la malla, aplica su textura previamente creada y genera su collider
    /// </summary>
    /// <param name="meshData"></param>
    public void DrawFinalMesh(MeshData meshData)
    {
        MeshFilter.sharedMesh = meshData.GetMesh();
        MeshRenderer.sharedMaterial.mainTexture = MeshTexture;

        GenerateCollider(meshData);
    }


    #region COLLIDER GENERATION

    /// <summary>
    /// Genera el collider del terreno con la malla indicada
    /// </summary>
    /// <param name="meshData"></param>
    public void GenerateCollider(MeshData meshData)
    {
        MeshCollider collider = MeshRenderer.gameObject.GetComponent<MeshCollider>();
        if(!collider)
            collider = MeshRenderer.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = meshData.GetMesh();
    }

    #endregion


    #region TEXTURE GENERATION

    public void PrepareTerrainTexture(float[,] heightMap)
    {
        Color[] colorArray = GetColorArray(heightMap);
        MeshTexture = TextureUtils.GetTextureFromColorArray(colorArray, heightMap.GetLength(0), heightMap.GetLength(1));
    }

    public void PrepareTerrainTexture(MeshData meshData, Vector2Int mapSize, float maxHeight)
    {
        Color[] colorArray = GetColorArray(meshData, mapSize, maxHeight);
        MeshTexture = TextureUtils.GetTextureFromColorArray(colorArray, mapSize.x, mapSize.y);
    }

    /// <summary>
    /// Devuelve el array con los colores necesario para pasarselo a una textura
    /// </summary>
    /// <param name="heightMap"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    private Color[] GetColorArray(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Color[] colorArray = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (_mode == DrawMode.Noise)
                    colorArray[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                else
                    colorArray[y * width + x] = HeightGradient.Evaluate(heightMap[x, y]);
            }
        }

        return colorArray;
    }

    private Color[] GetColorArray(MeshData data, Vector2Int mapSize, float maxHeight)
    {
        Color[] colorArray = new Color[mapSize.x * mapSize.y];

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                int currentIndex = y * mapSize.x + x;
                float currentProportionalHeight = data.Vertices[currentIndex].y/maxHeight;

                if (_mode == DrawMode.Noise)
                    colorArray[currentIndex] = Color.Lerp(Color.black, Color.white, currentProportionalHeight);
                else
                    colorArray[currentIndex] = HeightGradient.Evaluate(currentProportionalHeight);
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